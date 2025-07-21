using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using R3;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using VContainer;
using BossSlayingTourney.Core;
using BossSlayingTourney.Game.Battle;
using BossSlayingTourney.Game.Effects;
using BossSlayingTourney.Network;
using BossSlayingTourney.Skills;

namespace BossSlayingTourney.Game.Controllers
{
    public class BattleController : MonoBehaviour
    {
        #region Injected Fields
        private StateController _stateController;
        private UserController _userController;
        private MainController _mainController;
        private BattleLogController _battleLogController;
        private BattleAnimator _battleAnimator;
        private EnemyController _enemyController;
        private ImageAnimationHolder _imageAnimationHolder;
        #endregion

        private RewardSelecter _rewardSelecter;
        private NetworkManager _networkManager;
        private BattleUIController _battleUIController;
        private BattleTurnManager _battleTurnManager;
        private BattleStatusManager _battleStatusManager;
        private BattleAnimationManager _battleAnimationManager;
        private BattleNetworkHandler _battleNetworkHandler;

        private BattleStatus _battleStatus;

        private Entity _leftEntity;
        private Entity _rightEntity;
        private Entity _currentTurnEntity;
        private Entity _waitingTurnEntity;
        private Entity _winnerEntity;
        private Entity _loserEntity;

        private Vector2 _entityLeftsPreviousPos;

        [Networked]
        public int TurnCount { get; set; }

        [Networked, OnChangedRender(nameof(OnTurnEntityChanged))]
        private NetworkObject _currentTurnEntityNO { get; set; }


        private bool _hasActionEnded;

        /// <summary>
        /// HPやMPの変化に対するリアクティブなアニメーションの実行中カウンター（スレッドセーフ）
        /// 0の場合はすべてのアニメーションが終了している状態
        /// </summary>
        private int _reactiveNumberAnimationCounter;

        private CompositeDisposable _disposable = new();

        public List<int> RewardChoices
        {
            get
            {
                return _rewardSelecter?.GetAvailableRewardChoices() ?? new List<int> { 0 };
            }
        }

        [Inject]
        public void Construct(
            StateController stateController,
            MainController mainController,
            UserController userController,
            BattleLogController battleLogController,
            EnemyController enemyController,
            ImageAnimationHolder imageAnimationHolder
            )
        {
            _networkManager = NetworkManager.Instance;
            _stateController = stateController;
            _mainController = mainController;
            _userController = userController;
            _battleLogController = battleLogController;
            _enemyController = enemyController;
            _imageAnimationHolder = imageAnimationHolder;
        }

        public static class ClassNames
        {
            public const string RewardButton = "rewardButton";
            public const string RewardTitle = "rewardTitle";
            public const string RewardDescription = "rewardDescription";
        }

        private void InitializeBattleUIController()
        {
            _battleUIController = new BattleUIController();
            _battleUIController.Initialize(GetComponent<UIDocument>().rootVisualElement);

            // イベント購読
            _battleUIController.OnAttackClicked.Subscribe(_ => Attack()).AddTo(_disposable);
            _battleUIController.OnSkillScrollOpenClicked.Subscribe(_ => OnOpenSkillScrollClicked()).AddTo(_disposable);
            _battleUIController.OnSkillScrollCloseClicked.Subscribe(_ => OnCloseSkillScrollClicked()).AddTo(_disposable);
            _battleUIController.OnSkillUsed.Subscribe(skillName => UseSkill(skillName)).AddTo(_disposable);
        }

        private void InitializeBattleAnimationManager()
        {
            _battleAnimationManager = new BattleAnimationManager(_battleAnimator, _battleUIController, _imageAnimationHolder);

            // イベント購読
            _battleAnimationManager.OnAllAnimationsCompleted.Subscribe(_ => OnAllAnimationsCompleted()).AddTo(_disposable);
        }

        private void OnAllAnimationsCompleted()
        {
            // アニメーション完了時の処理
        }

        private void InitializeBattleStatusManager()
        {
            _battleStatusManager = new BattleStatusManager(_battleLogController, _battleUIController, _battleAnimationManager);

            // イベント購読
            _battleStatusManager.OnConditionProcessed.Subscribe(condition => OnConditionProcessed(condition)).AddTo(_disposable);
        }

        private void OnConditionProcessed(Condition condition)
        {
            // 状態異常処理完了時の処理
        }

        private void InitializeBattleNetworkHandler()
        {
            _battleNetworkHandler = new BattleNetworkHandler(
                _battleLogController,
                _battleAnimationManager,
                _battleStatusManager,
                _battleUIController,
                _stateController,
                _mainController,
                _enemyController,
                _rewardSelecter
            );

            // イベント購読
            _battleNetworkHandler.OnAttackRequested.Subscribe(_ => AttackProcessByHostAsync().Forget()).AddTo(_disposable);
            _battleNetworkHandler.OnSkillRequested.Subscribe(skillName => SkillProcessByHostAsync(skillName).Forget()).AddTo(_disposable);
        }

        private void InitializeBattleTurnManager()
        {
            _battleTurnManager = new BattleTurnManager(_battleLogController, _networkManager, _userController, _battleUIController);

            // イベント購読
            _battleTurnManager.OnTurnChanged.Subscribe(_ => OnTurnEntityChanged()).AddTo(_disposable);
            _battleTurnManager.OnActionEnded.Subscribe(_ => OnActionEnded()).AddTo(_disposable);
        }
        private void InitializeBattleLogController()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _battleLogController.Initialize(root.Q<Label>("Label-Log"));
            _battleLogController.OnAllLogsRead.Subscribe(_ =>
            {
                OnAllLogsRead();
            }).AddTo(_disposable);
        }

        private void InitializeRewardSelecter()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _rewardSelecter = new RewardSelecter(root.Q<VisualElement>("RewardElement"), _userController);

            // イベント購読
            _rewardSelecter.OnStatusRewardSelected.Subscribe(_ => OnStatusRewardSelected()).AddTo(_disposable);
            _rewardSelecter.OnSkillRewardSelected.Subscribe(index => OnSkillRewardSelected(index)).AddTo(_disposable);
        }

        private void SetEntities()
        {
            // UIにエンティティ情報を設定
            _battleUIController.SetEntityInfo(_leftEntity, _rightEntity);

            // アニメーション用クラスにエンティティのイメージを設定
            _battleAnimator = new BattleAnimator(_battleUIController.EntityImageLeft, _battleUIController.EntityImageRight, _leftEntity);

            Rpc_WinnerEntity(null);
            Rpc_LoserEntity(null);

            // アクションエレメントの位置を左側に初期化
            _battleUIController.ResetActionElementPosition();
            // リアクティブプロパティの監視を設定
            InitializeReactivePropertiesOfStatusChange();
        }

        public void StartBattle(Entity leftEntity, Entity rightEntity, Vector2 entityLeftsPreviousPos)
        {
            _battleStatus = BattleStatus.BeforeAction;
            _entityLeftsPreviousPos = entityLeftsPreviousPos;

            _leftEntity = _currentTurnEntity = leftEntity;
            _rightEntity = _waitingTurnEntity = rightEntity;

            // ターンマネージャーを初期化
            _battleTurnManager.Initialize(leftEntity, rightEntity);

            // マスタークライアントのみが初期状態を設定する
            if (_networkManager.GetNetworkRunner().IsSharedModeMasterClient)
            {
                TurnCount = 0;
                _currentTurnEntityNO = leftEntity.Object;
            }

            _battleUIController.SwitchArrowVisibility(_currentTurnEntity, _leftEntity);

            _battleUIController.DisplayBattleElement();
            _battleUIController.CloseCommandView();
            _battleUIController.CloseSkillScroll();
            CloseRewardView();
            Rpc_SetConditionImage();

            SetEntities();

            _battleLogController.ClearLogs();
            _battleLogController.AddLog(Constants.GetSentenceWhenStartBattle(Settings.Language.ToString(), _leftEntity.name, _rightEntity.name));
            _battleLogController.EnableFlip();

            InitializeBattleUIController();
            InitializeBattleAnimationManager();
            InitializeBattleStatusManager();
            InitializeBattleNetworkHandler();
            InitializeBattleTurnManager();
            InitializeBattleLogController();
            InitializeRewardSelecter();
        }
        private void StartNewTurn()
        {
            _battleTurnManager.StartNewTurn(TurnCount);
        }

        private void SwitchArrowVisibility()
        {
            _battleUIController.SwitchArrowVisibility(_currentTurnEntity, _leftEntity);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_BackToField()
        {
            ResetAbnormalCondition();
            HandleDiedEntity(_loserEntity);
            _stateController.ChangeState(State.Field);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_GoToResult()
        {
            HandleDiedEntity(_loserEntity);
            _mainController.WinnerEntity = _winnerEntity;
            _stateController.ChangeState(State.Result);
        }

        public void Attack()
        {
            _battleUIController.CloseCommandView();
            Rpc_RequestAttack();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void Rpc_RequestAttack()
        {
            // 共有ホスト以外のクライアントでアニメーションを再生する
            Rpc_PlayAttackAnimation();
            // 共有ホストが行う必要のある処理を実行する
            AttackProcessByHostAsync().Forget();
        }

        /// <summary>
        /// ホスト以外のプレイヤーが攻撃のアニメーションを再生するためのRPC
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void Rpc_PlayAttackAnimation()
        {
            PlayAttackAnimationAsync().Forget();
        }

        private async UniTask PlayAttackAnimationAsync()
        {
            // ステップアニメーションを実行
            await _battleAnimator.AnimateEntityStepAsync(_currentTurnEntity);
            // 攻撃時のエフェクトを再生
            VisualElement targetImage = _waitingTurnEntity == _leftEntity ? _battleUIController.EntityImageLeft : _battleUIController.EntityImageRight;
            await _battleAnimator.PlayImageAnimationAsync(Constants.ImageAnimationKeySlash, targetImage, _imageAnimationHolder);
        }

        private async UniTask AttackProcessByHostAsync()
        {
            // 「○○の攻撃！」のログ
            _battleLogController.AddLog(Constants.GetAttackSentence(Settings.Language, _currentTurnEntity.name));
            // 共有ホストのクライアントでアニメーションを再生する
            await PlayAttackAnimationAsync();
            // ダメージ計算処理
            int damage = _currentTurnEntity.Attack(_waitingTurnEntity);

            // 結果のログ
            if (damage == 0)
            {
                _battleLogController.AddLog(Constants.GetAttackResultSentence(Settings.Language, _waitingTurnEntity.name, damage));
            }

            OnActionEnded();
            await ResumeFlipIfAllReactiveAnimationsEndsAsync();
        }

        public void UseSkill(string skillName)
        {
            _battleUIController.CloseCommandView();
            Rpc_RequestSkill(skillName);
        }


        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void Rpc_RequestSkill(string skillName)
        {
            // 共有ホスト以外のクライアントでアニメーションを再生する
            Rpc_PlaySkillAnimation(skillName);
            // 共有ホストが行う必要のある処理を実行する
            SkillProcessByHostAsync(skillName).Forget();
        }

        /// <summary>
        /// ホスト以外のプレイヤーが攻撃のアニメーションを再生するためのRPC
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void Rpc_PlaySkillAnimation(string skillName)
        {
            PlaySkillAnimationAsync(skillName).Forget();
        }

        private async UniTask PlaySkillAnimationAsync(string skillName)
        {
            // ステップアニメーションを実行
            await _battleAnimator.AnimateEntityStepAsync(_currentTurnEntity);
            // 攻撃時のエフェクトを再生
            VisualElement targetImage = _waitingTurnEntity == _leftEntity ? _battleUIController.EntityImageLeft : _battleUIController.EntityImageRight;
            await _battleAnimator.PlayImageAnimationAsync(SkillList.GetSkillEffectKey(skillName), targetImage, _imageAnimationHolder);
        }

        private async UniTask SkillProcessByHostAsync(string skillName)
        {
            _battleLogController.AddLog(Constants.GetSkillSentence(Settings.Language, _currentTurnEntity.name, skillName));

            await PlaySkillAnimationAsync(skillName);

            Skill.SkillResult result = _currentTurnEntity.UseSkill(skillName, _currentTurnEntity, _waitingTurnEntity);

            var skillEffectType = SkillList.GetSkillEffectType(skillName);
            // スキルのエフェクトを再生
            if (skillEffectType is SkillList.SkillEffectType.Heal or SkillList.SkillEffectType.Buff)
            {
                VisualElement targetImage = _currentTurnEntity == _leftEntity ? _battleUIController.EntityImageLeft : _battleUIController.EntityImageRight;
                await _battleAnimator.PlayImageAnimationAsync(result.EffectKey, targetImage, _imageAnimationHolder);
            }
            else if (skillEffectType is SkillList.SkillEffectType.Damage)
            {
                VisualElement targetImage = _waitingTurnEntity == _leftEntity ? _battleUIController.EntityImageLeft : _battleUIController.EntityImageRight;
                await _battleAnimator.PlayImageAnimationAsync(result.EffectKey, targetImage, _imageAnimationHolder);
            }

            // スキルの結果のログ
            foreach (var log in result.Logs)
            {
                _battleLogController.AddLog(log);
            }

            OnActionEnded();
            await ResumeFlipIfAllReactiveAnimationsEndsAsync();
        }

        private void OnOpenSkillScrollClicked()
        {
            _battleUIController.CloseCommandView();
            _battleUIController.OpenSkillScroll();
        }

        private void OnCloseSkillScrollClicked()
        {
            _battleUIController.CloseSkillScroll();
            _battleUIController.OpenCommandView();
        }

        private void OnActionEnded()
        {
            _hasActionEnded = true;
            ApplyCurrentBattleStatus();
            AddLogByCurrentBattleStatus();
        }

        private void OnAllLogsRead()
        {
            if (!_networkManager.GetNetworkRunner().IsSharedModeMasterClient)
            {
                return;
            }

            if (TurnCount == 0)
            {
                StartNewTurn();
                return;
            }

            switch (_battleStatus)
            {
                case BattleStatus.AfterAction:
                    Condition condition = CheckAbnormalCondition();
                    if (condition is Condition.Poison or Condition.Regen or Condition.Fire or Condition.Stun)
                    {
                        _battleStatus = BattleStatus.CheckAbnormalCondition;
                        ApplyCurrentBattleStatus();
                        ResumeFlipIfAllReactiveAnimationsEndsAsync().Forget();
                    }
                    else
                    {
                        StartNewTurn();
                    }
                    break;
                case BattleStatus.CheckAbnormalCondition:
                    StartNewTurn();
                    break;

                case BattleStatus.LeftWin or BattleStatus.RightWin:
                    if (_winnerEntity.EntityType == EntityType.Player)
                    {
                        Rpc_HideBattleElement();
                        Rpc_OpenRewardView();
                        SetRewardsProcess();
                        _battleLogController.SetText(Constants.GetSentenceWhenSelectingReward(Settings.Language, _winnerEntity == _userController.MyEntity, _winnerEntity.name));
                        _battleStatus = BattleStatus.SelectReward;
                        if (_winnerEntity.IsNpc)
                        {
                            NpcActionController.SelectReward(this).Forget();
                        }
                    }
                    else
                    {
                        _battleLogController.AddLog(Constants.GetSentenceWhenEnemyWins(Settings.Language, _winnerEntity.name));
                        _battleLogController.EnableFlip();
                        _battleStatus = BattleStatus.BattleEnding;
                    }
                    break;

                case BattleStatus.BattleEnding:
                    _disposable.Dispose();
                    _disposable = new CompositeDisposable();
                    Rpc_BackToField();
                    break;

                case BattleStatus.GameClear:
                    _disposable.Dispose();
                    _disposable = new CompositeDisposable();
                    Rpc_GoToResult();
                    break;
            }
        }

        /// <summary>
        ///  現在のバトルステータスを判定する
        /// </summary>
        private void ApplyCurrentBattleStatus()
        {
            if (_leftEntity.Hp <= 0)
            {
                _battleStatus = BattleStatus.RightWin;
                Rpc_WinnerEntity(_rightEntity);
                Rpc_LoserEntity(_leftEntity);
            }
            else if (_rightEntity.Hp <= 0)
            {
                _battleStatus = BattleStatus.LeftWin;
                Rpc_WinnerEntity(_leftEntity);
                Rpc_LoserEntity(_rightEntity);
                if (_rightEntity.EntityType == EntityType.Satan)
                {
                    _battleStatus = BattleStatus.GameClear;
                }
            }
            else if (!_hasActionEnded)
            {
                _battleStatus = BattleStatus.BeforeAction;
            }
            else if (_battleStatus != BattleStatus.CheckAbnormalCondition)
            {
                _battleStatus = BattleStatus.AfterAction;
            }
        }

        /// <summary>
        ///  特定のバトルステータスの場合はログを追加する
        /// </summary>
        private void AddLogByCurrentBattleStatus()
        {
            switch (_battleStatus)
            {
                case BattleStatus.BeforeAction:
                    return;
                case BattleStatus.AfterAction:
                    return;
                case BattleStatus.LeftWin:
                    _battleLogController.AddLog(Constants.GetResultSentence(Settings.Language, _winnerEntity.name, _loserEntity.name));
                    _battleLogController.EnableFlip();
                    break;
                case BattleStatus.RightWin:
                    _battleLogController.AddLog(Constants.GetResultSentence(Settings.Language, _winnerEntity.name, _loserEntity.name));
                    _battleLogController.EnableFlip();
                    break;
                case BattleStatus.GameClear:
                    _battleLogController.AddLog(Constants.GetResultSentence(Settings.Language, _winnerEntity.name, _loserEntity.name));
                    _battleLogController.AddLog(Constants.GetSentenceWhenGameClear(Settings.Language, _winnerEntity.name));
                    _battleLogController.EnableFlip();
                    break;
            }
        }

        /// <summary>
        /// 状態異常の際の処理を行う
        /// </summary>
        private Condition CheckAbnormalCondition()
        {
            Condition condition = Condition.None;
            //　状態異常の画像をセット
            Rpc_SetConditionImage();

            var currentTurnEntitiesCondition = _currentTurnEntity.AbnormalConditionType;
            switch (currentTurnEntitiesCondition)
            {
                case Condition.Poison:
                    int poisonDamage = (int)(_currentTurnEntity.Hp * Constants.PoisonDamageRateOfHitPoint);
                    _currentTurnEntity.SetHitPoint(_currentTurnEntity.Hp - poisonDamage);
                    _battleLogController.AddLog(Constants.GetPoisonSentence(Settings.Language, _currentTurnEntity.name));
                    Rpc_PlayImageAnimation(Constants.ImageAnimationKeyPoisonMushroom, _currentTurnEntity);
                    condition = Condition.Poison;
                    break;

                case Condition.Fire:
                    int damage = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                            Constants.FireDamage,
                            Constants.FireDamageOffsetPercent,
                            0
                        );

                    _currentTurnEntity.SetHitPoint(_currentTurnEntity.Hp - damage);
                    _battleLogController.AddLog(Constants.GetFireDamageSentence(Settings.Language, _currentTurnEntity.name));
                    Rpc_PlayImageAnimation(Constants.ImageAnimationKeyIgnition, _currentTurnEntity);
                    condition = Condition.Fire;
                    break;

                case Condition.Regen:
                    int regenAmount = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                            Constants.RegenAmount,
                            Constants.RegenAmountOffsetPercent,
                            30
                        );
                    _currentTurnEntity.SetHitPoint(_currentTurnEntity.Hp + regenAmount);
                    _battleLogController.AddLog(Constants.GetRegenSentence(Settings.Language, _currentTurnEntity.name, regenAmount));
                    if (regenAmount > 0)
                    {
                        Rpc_PlayImageAnimation(Constants.ImageAnimationKeyRegen, _currentTurnEntity);
                    }
                    condition = Condition.Regen;
                    break;

                default:
                    break;
            }

            var waitingTurnEntitiesCondition = _waitingTurnEntity.AbnormalConditionType;
            switch (waitingTurnEntitiesCondition)
            {
                case Condition.Stun:
                    _battleLogController.AddLog(Constants.GetStunSentence(Settings.Language, _waitingTurnEntity.name));
                    condition = Condition.Stun;
                    break;

                default:
                    break;
            }
            return condition;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_SetConditionImage()
        {
            _battleUIController.SetConditionImage(_leftEntity, _rightEntity);
        }

        public void OnStatusRewardSelected()
        {
            string result = _rewardSelecter.ExecuteStatusReward();
            _battleLogController.AddLog(result);
            _battleLogController.EnableFlip();
            _rewardSelecter.HideRewardView();
            _battleStatus = BattleStatus.BattleEnding;
        }

        public void OnSkillRewardSelected(int index)
        {
            string log = _rewardSelecter.ExecuteSkillReward(index);
            _battleLogController.AddLog(log);
            _battleLogController.EnableFlip();
            _rewardSelecter.HideRewardView();
            _battleStatus = BattleStatus.BattleEnding;
        }

        private void SetRewardsProcess()
        {
            _rewardSelecter.SetupRewards(_winnerEntity, _loserEntity);
            Rpc_SetRewards();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_SetRewards()
        {
            _rewardSelecter.SetupRewards(_winnerEntity, _loserEntity);
        }

        private void HandleDiedEntity(Entity entity)
        {
            switch (entity.EntityType)
            {
                case EntityType.Player:
                    // leftEntity(さいころを振って動いたエンティティ)が負けた場合エンカウント地点の1マス前の位置に戻す
                    if (_loserEntity == _leftEntity)
                    {
                        _mainController.SetPlayerAsDead(entity, _entityLeftsPreviousPos);
                        break;
                    }
                    _mainController.SetPlayerAsDead(entity, _loserEntity.transform.position);
                    break;
                default:
                    Destroy(entity.gameObject);
                    _enemyController.EnemyList.Remove(entity);
                    break;
            }
        }

        private void ResetAbnormalCondition()
        {
            _leftEntity.ResetAbnormalCondition();
            _rightEntity.ResetAbnormalCondition();
        }

        private void DisplayBattleElement()
        {
            _battleUIController.DisplayBattleElement();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_HideBattleElement()
        {
            _battleUIController.HideBattleElement();
        }

        private void OpenCommandView()
        {
            _battleUIController.OpenCommandView();
        }

        private void CloseCommandView()
        {
            _battleUIController.CloseCommandView();
        }

        private void OpenSkillScroll()
        {
            _battleUIController.OpenSkillScroll();
        }

        private void CloseSkillScroll()
        {
            _battleUIController.CloseSkillScroll();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_OpenRewardView()
        {
            _rewardSelecter.ShowRewardView();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void CloseRewardView()
        {
            _rewardSelecter.HideRewardView();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_SetCurrentTurnEntity(Entity entity)
        {
            _currentTurnEntity = entity;
            _currentTurnEntityNO = entity.Object;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_SetWaitingTurnEntity(Entity entity)
        {
            _waitingTurnEntity = entity;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_WinnerEntity(Entity entity)
        {
            _winnerEntity = entity;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_LoserEntity(Entity entity)
        {
            _loserEntity = entity;
        }

        private void OnTurnEntityChanged()
        {
            // ターン表示の矢印を切り替える
            SwitchArrowVisibility();

            // 自分のターンが来たかどうかを判定し、コマンドUIを開く
            if (_currentTurnEntity == _userController.MyEntity)
            {
                OpenCommandView();
                _battleUIController.SetSkillButtons(_currentTurnEntity);
                _battleUIController.ToggleSkillButtonClickable(_currentTurnEntity);
            }
        }

        private void ResetActionElementPosition()
        {
            _battleUIController.ResetActionElementPosition();
        }

        // エンティティのHPとMPの変化を監視して、ダメージや回復のエフェクトを表示する
        private void InitializeReactivePropertiesOfStatusChange()
        {
            var delayOnHpGainMills = 700;
            var delayOnHpDecreaseMills = 0;
            var delayOnManaChangeMills = 0;

            _leftEntity.HitPointRp.Subscribe(newHp =>
            {
                StatusChangeReaction(newHp, _leftEntity.Hp, _battleUIController.HealthLabelLeft, delayOnHpGainMills, delayOnHpDecreaseMills).Forget();
            }).AddTo(_disposable);

            _leftEntity.ManaPointRp.Subscribe(newMp =>
            {
                StatusChangeReaction(newMp, _leftEntity.Mp, _battleUIController.ManaLabelLeft, delayOnManaChangeMills, delayOnManaChangeMills).Forget();
            }).AddTo(_disposable);

            _rightEntity.HitPointRp.Subscribe(newHp =>
            {
                StatusChangeReaction(newHp, _rightEntity.Hp, _battleUIController.HealthLabelRight, delayOnHpGainMills, delayOnHpDecreaseMills).Forget();
            }).AddTo(_disposable);

            _rightEntity.ManaPointRp.Subscribe(newMp =>
            {
                StatusChangeReaction(newMp, _rightEntity.Mp, _battleUIController.ManaLabelRight, delayOnManaChangeMills, delayOnManaChangeMills).Forget();
            }).AddTo(_disposable);
        }

        private async UniTask StatusChangeReaction(int newNumber, int oldNumber, Label targetLabel, int delayOnGainMills, int delayOnDecreaseMills)
        {
            // 競合状態を避けるため，スレッドセーフにカウンターを管理
            Interlocked.Increment(ref _reactiveNumberAnimationCounter);

            try
            {
                if (newNumber > oldNumber)
                {
                    await UniTask.Delay(delayOnGainMills);
                    await _battleAnimator.ChangeNumberWithAnimationAsync(targetLabel, newNumber);
                }
                else
                {
                    await UniTask.Delay(delayOnDecreaseMills);
                    await _battleAnimator.ChangeNumberWithAnimationAsync(targetLabel, newNumber);
                }
            }
            finally
            {
                Interlocked.Decrement(ref _reactiveNumberAnimationCounter);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_PlayImageAnimation(string key, Entity targetEntity)
        {
            if (targetEntity == _leftEntity)
            {
                _battleAnimator.PlayImageAnimationAsync(key, _battleUIController.EntityImageLeft, _imageAnimationHolder).Forget();
            }
            else
            {
                _battleAnimator.PlayImageAnimationAsync(key, _battleUIController.EntityImageRight, _imageAnimationHolder).Forget();
            }
        }

        /// <summary>
        /// 全てのリアクティブなアニメーションが終了しているかどうかを確認
        /// </summary>
        /// <returns>全てのアニメーションが終了している場合はtrue</returns>
        public bool AreAllReactiveAnimationsEnded()
        {
            return Interlocked.CompareExchange(ref _reactiveNumberAnimationCounter, 0, 0) == 0;
        }

        private async UniTask ResumeFlipIfAllReactiveAnimationsEndsAsync()
        {
            await UniTask.DelayFrame(2);
            while (!AreAllReactiveAnimationsEnded())
            {
                await UniTask.Delay(200);
            }
            _battleLogController.EnableFlip();
        }
    }
}