using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using R3;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class BattleController : MonoBehaviour
{
    private StateController _stateController;
    private UserController _userController;
    private MainController _mainController;
    private BattleLogController _battleLogController;
    private EnemyController _enemyController;
    private ImageAnimationHolder _imageAnimationHolder;
    private NetworkManager _networkManager;

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

    private VisualElement _root;
    private VisualElement _battleElement;
    private VisualElement _rewardElement;
    private VisualElement _actionElement;
    private VisualElement _commanndView;
    private VisualElement _skillView;
    private VisualElement _skillScrollContainer;

    private VisualElement _currentActionerArrowLeft;
    private VisualElement _currentActionerArrowRight;
    private Label _entityNameLeft;
    private Label _entityNameRight;

    private VisualElement _entityImageLeft;
    private VisualElement _entityImageRight;

    private VisualElement _leftConditionImage;
    private VisualElement _rightConditionImage;

    private Button _closeSkillScrollViewButton;
    private List<(Button, int)> _skillButtons = new();

    private Label _healthLabelLeft;
    private Label _manaLabelLeft;
    private Label _healthLabelRight;
    private Label _manaLabelRight;

    private CompositeDisposable _disposable = new();

    private Reward _currentReward;
    private List<Skill> _currentRewardSkills;

    private void OnTurnEntityChanged()
    {
        // ターン表示の矢印を切り替える
        SwitchArrowVisibility();

        // 自分のターンが来たかどうかを判定し、コマンドUIを開く
        if (_currentTurnEntity == _userController.MyEntity)
        {
            OpenCommandView();
            SetSkillButtons(_currentTurnEntity);
            ToggleSkillButonClickable(_currentTurnEntity);
        }
    }

    private void Start()
    {
        InitializeUIElements();
        InitializeBattleLogController();
    }

    public void Initialize(StateController stateController, MainController mainController, UserController userController, BattleLogController battleLogController, EnemyController enemyController, ImageAnimationHolder imageAnimationHolder)
    {
        _stateController = stateController;
        _mainController = mainController;
        _userController = userController;
        _battleLogController = battleLogController;
        _enemyController = enemyController;
        _imageAnimationHolder = imageAnimationHolder;
        _networkManager = NetworkManager.Instance;
    }

    public static class ClassNames
    {
        public const string RewardButton = "rewardButton";
        public const string RewardTitle = "rewardTitle";
        public const string RewardDescription = "rewardDescription";
    }

    private void InitializeUIElements()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        _battleElement = _root.Q<VisualElement>("BattleElement");
        _rewardElement = _root.Q<VisualElement>("RewardElement");

        _actionElement = _root.Q<VisualElement>("ActionElement");
        _commanndView = _root.Q<VisualElement>("CommandView");

        _skillView = _root.Q<VisualElement>("SkillView");
        _skillScrollContainer = _root.Q<VisualElement>("unity-content-container");

        _root.Q<Button>("Button-Attack").clicked += Attack;
        _root.Q<Button>("Button-Skill").clicked += OnOpenSkillScrollClicked;
        _closeSkillScrollViewButton = _root.Q<Button>("Button-CloseSkillScroll");
        _closeSkillScrollViewButton.clicked += OnCloseSkillScrollClicked;

        var rootLeftElement = _root.Q<VisualElement>("Element-Left");
        _entityImageLeft = rootLeftElement.Q<VisualElement>("Image-Entity");
        _entityNameLeft = rootLeftElement.Q<Label>("Label-EntityName");
        _currentActionerArrowLeft = rootLeftElement.Q<VisualElement>("Image-Arrow");

        _healthLabelLeft = rootLeftElement.Q<VisualElement>("Element-HitPoint").Q<Label>("Label");
        _manaLabelLeft = rootLeftElement.Q<VisualElement>("Element-ManaPoint").Q<Label>("Label");
        _leftConditionImage = rootLeftElement.Q<VisualElement>("Element-Condition").Q<VisualElement>("Icon");

        var rootRightElement = _root.Q<VisualElement>("Element-Right");
        _entityImageRight = rootRightElement.Q<VisualElement>("Image-Entity");
        _entityNameRight = rootRightElement.Q<Label>("Label-EntityName");
        _currentActionerArrowRight = rootRightElement.Q<VisualElement>("Image-Arrow");

        _healthLabelRight = rootRightElement.Q<VisualElement>("Element-HitPoint").Q<Label>("Label");
        _manaLabelRight = rootRightElement.Q<VisualElement>("Element-ManaPoint").Q<Label>("Label");
        _rightConditionImage = rootRightElement.Q<VisualElement>("Element-Condition").Q<VisualElement>("Icon");
    }

    private void InitializeBattleLogController()
    {
        _battleLogController.Initialize(_root.Q<Label>("Label-Log"));
        _battleLogController.OnAllLogsRead.Subscribe(_ =>
        {
            OnAllLogsRead();
        });
    }

    private void SetEntities()
    {
        // 左側のエンティティのUIの設定
        _entityNameLeft.text = _leftEntity.name;
        var texture = Addressables.LoadAssetAsync<Sprite>(_leftEntity.FieldSpriteAssetReference).WaitForCompletion().texture;
        _entityImageLeft.style.backgroundImage = texture;

        _healthLabelLeft.text = _leftEntity.Hp.ToString();
        _manaLabelLeft.text = _leftEntity.Mp.ToString();
        _leftConditionImage.style.display = DisplayStyle.None;

        // 右側のエンティティのUIの設定
        _entityNameRight.text = _rightEntity.name;
        var rightTexture = Addressables.LoadAssetAsync<Sprite>(_rightEntity.BattleSpriteAssetReference).WaitForCompletion().texture;
        _entityImageRight.style.backgroundImage = rightTexture;

        _healthLabelRight.text = _rightEntity.Hp.ToString();
        _manaLabelRight.text = _rightEntity.Mp.ToString();
        _rightConditionImage.style.display = DisplayStyle.None;

        Rpc_WinnerEntity(null);
        Rpc_LoserEntity(null);

        // アクションエレメントの位置を左側に初期化
        ResetActionElementPosition();
        // リアクティブプロパティの監視を設定
        InitializeReactivePropertiesOfStatusChange();
    }

    private void SetSkillButtons(Entity entity)
    {
        _skillButtons.Clear();
        _skillScrollContainer.Clear();

        foreach (var skill in entity.SyncedSkills)
        {
            var skillButton = new Button(() =>
            {
                UseSkill(skill.Name);
                CloseSkillScroll();
            })
            {
                text = skill.Name
            };
            skillButton.AddToClassList("skillbutton");
            _skillScrollContainer.Add(skillButton);
            _skillButtons.Add((skillButton, skill.ManaCost));
        }
    }

    private void ToggleSkillButonClickable(Entity entity)
    {
        foreach (var (button, manaCost) in _skillButtons)
        {
            button.SetEnabled(entity.Mp >= manaCost);
        }
    }

    public void StartBattle(Entity leftEntity, Entity rightEntity, Vector2 entityLeftsPreviousPos)
    {
        _battleStatus = BattleStatus.BeforeAction;
        _entityLeftsPreviousPos = entityLeftsPreviousPos;

        _leftEntity = _currentTurnEntity = leftEntity;
        _rightEntity = _waitingTurnEntity = rightEntity;

        // マスタークライアントのみが初期状態を設定する
        if (_networkManager.GetNetworkRunner().IsSharedModeMasterClient)
        {
            TurnCount = 0;
            _currentTurnEntityNO = leftEntity.Object;
        }

        _currentActionerArrowLeft.style.visibility = Visibility.Visible;
        _currentActionerArrowRight.style.visibility = Visibility.Hidden;

        DisplayBattleElement();
        CloseCommandView();
        CloseSkillScroll();
        CloseRewardView();
        Rpc_SetConditionImage();

        SetEntities();

        _battleLogController.ClearLogs();
        _battleLogController.AddLog(Constants.GetSentenceWhenStartBattle(Settings.Language.ToString(), _leftEntity.name, _rightEntity.name));
        _battleLogController.EnableFlip();
    }
    private void StartNewTurn()
    {
        if (!_networkManager.GetNetworkRunner().IsSharedModeMasterClient)
        {
            return;
        }

        _hasActionEnded = false;
        _battleStatus = BattleStatus.BeforeAction;

        bool isFirstTurn = TurnCount == 0;

        // _currentTurnEntityと_waitingTurnEntityを入れ替える
        if (!isFirstTurn)
        {
            Entity tmp = _currentTurnEntity;
            Rpc_SetCurrentTurnEntity(_waitingTurnEntity);
            Rpc_SetWaitingTurnEntity(tmp);
            _currentTurnEntityNO = _waitingTurnEntity.Object;
        }

        // ターン数を更新
        if (_currentTurnEntity == _leftEntity)
        {
            TurnCount++;
        }

        if (_currentTurnEntity.AbnormalConditionType == Condition.Stun)
        {
            _currentTurnEntity.SetAbnormalCondition(Condition.None, null);
            Rpc_SetConditionImage();
            StartNewTurn();
            return;
        }

        HandleTurnStartActions();
    }

    private void SwitchArrowVisibility()
    {
        if (_currentTurnEntity == _leftEntity)
        {
            _currentActionerArrowLeft.style.visibility = Visibility.Visible;
            _currentActionerArrowRight.style.visibility = Visibility.Hidden;
        }
        else
        {
            _currentActionerArrowLeft.style.visibility = Visibility.Hidden;
            _currentActionerArrowRight.style.visibility = Visibility.Visible;
        }
    }

    private void HandleTurnStartActions()
    {
        // NPCのターンの場合は行動を実行する
        if (_currentTurnEntity.IsNpc)
        {
            NpcActionController.ActAsync(this, _currentTurnEntity, _waitingTurnEntity).Forget();
        }
        // プレイヤーのターンの場合は待機ログを出す
        else
        {
            _battleLogController.SetText(Constants.GetSentenceWhileWaitingAction(Settings.Language.ToString(), _currentTurnEntity.name));
        }
    }

    private void BackToField()
    {
        ResetAbnormalCondition();
        HandleDiedEntity(_loserEntity);
        _stateController.ChangeState(State.Field);
    }

    private void GoToResult()
    {
        HandleDiedEntity(_loserEntity);
        _mainController.WinnerEntity = _winnerEntity;
        _stateController.ChangeState(State.Result);
    }

    public void Attack()
    {
        CloseCommandView();
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
        await AnimateEntityStepAsync(_currentTurnEntity);
        // 攻撃時のエフェクトを再生
        await PlayImageAnimationAsync(Constants.ImageAnimationKeySlash, _waitingTurnEntity);
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
        CloseCommandView();
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
        await AnimateEntityStepAsync(_currentTurnEntity);
        // 攻撃時のエフェクトを再生
        await PlayImageAnimationAsync(SkillList.GetSkillEffectKey(skillName), _waitingTurnEntity);
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
            await PlayImageAnimationAsync(result.EffectKey, _currentTurnEntity);
        }
        else if (skillEffectType is SkillList.SkillEffectType.Damage)
        {
            await PlayImageAnimationAsync(result.EffectKey, _waitingTurnEntity);
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
        CloseCommandView();
        OpenSkillScroll();
    }

    private void OnCloseSkillScrollClicked()
    {
        CloseSkillScroll();
        OpenCommandView();
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
                        NpcActionController.SelectReward(this, _battleLogController).Forget();
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
                BackToField();
                break;

            case BattleStatus.GameClear:
                _disposable.Dispose();
                _disposable = new CompositeDisposable();
                GoToResult();
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
        var leftCondition = _leftEntity.AbnormalConditionType;
        var rightCondition = _rightEntity.AbnormalConditionType;

        _leftConditionImage.style.display = leftCondition == Condition.None ? DisplayStyle.None : DisplayStyle.Flex;
        _rightConditionImage.style.display = rightCondition == Condition.None ? DisplayStyle.None : DisplayStyle.Flex;

        switch (leftCondition)
        {
            case Condition.Poison:
                _leftConditionImage.style.backgroundImage = Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferencePoisonCondition).WaitForCompletion().texture;
                break;
            case Condition.Regen:
                _leftConditionImage.style.backgroundImage = Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceRegenCondition).WaitForCompletion().texture;
                break;
            case Condition.Stun:
                _leftConditionImage.style.backgroundImage = Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceStunCondition).WaitForCompletion().texture;
                break;
            case Condition.Fire:
                _leftConditionImage.style.backgroundImage = Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceFireCondition).WaitForCompletion().texture;
                break;
        }

        switch (rightCondition)
        {
            case Condition.Poison:
                _rightConditionImage.style.backgroundImage = Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferencePoisonCondition).WaitForCompletion().texture;
                break;
            case Condition.Regen:
                _rightConditionImage.style.backgroundImage = Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceRegenCondition).WaitForCompletion().texture;
                break;
            case Condition.Stun:
                _rightConditionImage.style.backgroundImage = Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceStunCondition).WaitForCompletion().texture;
                break;
            case Condition.Fire:
                _rightConditionImage.style.backgroundImage = Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceFireCondition).WaitForCompletion().texture;
                break;
        }
    }

    public void OnStatusRewardSelected()
    {
        string result = _currentReward.Execute(_winnerEntity);
        _battleLogController.AddLog(result);
        _battleLogController.EnableFlip();
        CloseRewardView();
        _battleStatus = BattleStatus.BattleEnding;
    }

    public void OnSkillRewardSelected(int index)
    {
        AddSkillToWinnerEntity(_currentRewardSkills[index], out string log);
        _battleLogController.AddLog(log);
        _battleLogController.EnableFlip();
        CloseRewardView();
        _battleStatus = BattleStatus.BattleEnding;
    }

    public List<int> RewardChoices
    {
        get
        {
            var rewardChoices = new List<int> { 0 };
            for (int i = 0; i < _currentRewardSkills.Count; i++)
            {
                if (_winnerEntity.SyncedSkills.Find(s => s.Name == _currentRewardSkills[i].Name) == null)
                {
                    rewardChoices.Add(i);
                }
            }
            return rewardChoices;
        }
    }

    private void SetRewardsProcess()
    {
        // スキル報酬のセット
        List<Skill> loserSkills = new List<Skill>(_loserEntity.SyncedSkills);
        _currentRewardSkills = GetListOfRandomTwoElementsFromList(loserSkills);

        Rpc_SetRewards();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_SetRewards()
    {
        _rewardElement.Clear();
        var rewardButtons = new List<Button>();

        // ステータス報酬のセット
        _currentReward = new Reward();
        var statusRewardButton = new Button(OnStatusRewardSelected);
        statusRewardButton.AddToClassList(ClassNames.RewardButton);
        statusRewardButton.Add(CreateNewLabelWithDetails(ClassNames.RewardTitle, Constants.GetStatusRewardTitle(Settings.Language)));
        statusRewardButton.Add(CreateNewLabelWithDetails(ClassNames.RewardDescription, _currentReward.Description));
        rewardButtons.Add(statusRewardButton);

        for (int i = 0; i < _currentRewardSkills.Count; i++)
        {
            int index = i;
            var skillRewardButton = new Button(() => OnSkillRewardSelected(index));
            skillRewardButton.AddToClassList(ClassNames.RewardButton);
            var labelRewardTitle = CreateNewLabelWithDetails(ClassNames.RewardTitle, _currentRewardSkills[index].Name);
            skillRewardButton.Add(labelRewardTitle);
            var labelRewardDescription = CreateNewLabelWithDetails(ClassNames.RewardDescription, _currentRewardSkills[index].Description);
            skillRewardButton.Add(labelRewardDescription);
            if (_winnerEntity.SyncedSkills.Find(s => s.Name == _currentRewardSkills[index].Name) != null)
            {
                skillRewardButton.SetEnabled(false);
                labelRewardDescription.text = Constants.GetSentenceWhenAlreadyHoldingTheSkill(Settings.Language);
            }
            rewardButtons.Add(skillRewardButton);
        }

        foreach (var button in rewardButtons)
        {
            _rewardElement.Add(button);
            if (button.enabledSelf == false)
            {
                continue;
            }
            button.SetEnabled(_winnerEntity == _userController.MyEntity);
        }
    }

    private Label CreateNewLabelWithDetails(string className, string text)
    {
        var label = new Label();
        label.AddToClassList(className);
        label.text = text;
        return label;
    }

    private List<T> GetListOfRandomTwoElementsFromList<T>(List<T> list)
    {
        List<T> result = new List<T>();
        if (list.Count > 0)
        {
            int index1 = UnityEngine.Random.Range(0, list.Count);
            result.Add(list[index1]);
            list.RemoveAt(index1);
        }
        if (list.Count > 0)
        {
            int index2 = UnityEngine.Random.Range(0, list.Count);
            result.Add(list[index2]);
        }
        return result;
    }

    private void AddSkillToWinnerEntity(Skill skill, out string log)
    {
        _winnerEntity.SyncedSkills.Add(skill);
        log = string.Format(Constants.GetSkillGetSentence(Settings.Language), _winnerEntity.name, skill.Name);
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
        _battleElement.style.display = DisplayStyle.Flex;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_HideBattleElement()
    {
        _battleElement.style.display = DisplayStyle.None;
    }

    private void OpenCommandView()
    {
        _commanndView.style.visibility = Visibility.Visible;
    }

    private void CloseCommandView()
    {
        _commanndView.style.visibility = Visibility.Hidden;
    }

    private void OpenSkillScroll()
    {
        _skillView.style.display = DisplayStyle.Flex;
        _closeSkillScrollViewButton.style.display = DisplayStyle.Flex;
        _commanndView.style.display = DisplayStyle.None;
    }

    private void CloseSkillScroll()
    {
        _skillView.style.display = DisplayStyle.None;
        _commanndView.style.display = DisplayStyle.Flex;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_OpenRewardView()
    {
        _rewardElement.style.display = DisplayStyle.Flex;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void CloseRewardView()
    {
        _rewardElement.style.display = DisplayStyle.None;
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

    private void ResetActionElementPosition()
    {
        var parent = _actionElement.parent;
        var index = parent.IndexOf(_actionElement);

        // 一度アクションエレメントを削除
        parent.RemoveAt(index);
        // アクションエレメントを元の位置に戻す
        parent.Insert(0, _actionElement);
    }

    // エンティティのHPとMPの変化を監視して、ダメージや回復のエフェクトを表示する
    private void InitializeReactivePropertiesOfStatusChange()
    {
        var delayOnHpGainMills = 700;
        var delayOnHpDecreaseMills = 0;
        var delayOnManaChangeMills = 0;

        _leftEntity.HitPointRp.Subscribe(newHp =>
        {
            StatusChangeReaction(newHp, _leftEntity.Hp, _healthLabelLeft, delayOnHpGainMills, delayOnHpDecreaseMills).Forget();
        }).AddTo(_disposable);

        _leftEntity.ManaPointRp.Subscribe(newMp =>
        {
            StatusChangeReaction(newMp, _leftEntity.Mp, _manaLabelLeft, delayOnManaChangeMills, delayOnManaChangeMills).Forget();
        }).AddTo(_disposable);

        _rightEntity.HitPointRp.Subscribe(newHp =>
        {
            StatusChangeReaction(newHp, _rightEntity.Hp, _healthLabelRight, delayOnHpGainMills, delayOnHpDecreaseMills).Forget();
        }).AddTo(_disposable);

        _rightEntity.ManaPointRp.Subscribe(newMp =>
        {
            StatusChangeReaction(newMp, _rightEntity.Mp, _manaLabelRight, delayOnManaChangeMills, delayOnManaChangeMills).Forget();
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
                await ChangeNumberWithAnimationAsync(targetLabel, newNumber);
            }
            else
            {
                await UniTask.Delay(delayOnDecreaseMills);
                await ChangeNumberWithAnimationAsync(targetLabel, newNumber);
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
        PlayImageAnimationAsync(key, targetEntity).Forget();
    }

    private async UniTask PlayImageAnimationAsync(string key, Entity targetEntity)
    {
        var imageAnimation = ImageAnimationPool.Instance.GetFromPool<ImageAnimation>("ImageAnimation");
        imageAnimation.SetSprites(_imageAnimationHolder.GetSpriteps(key));

        var ratio = Screen.width / Constants.BaseScreenSize.x;
        if (targetEntity == _leftEntity)
        {
            var rect = _entityImageLeft.worldBound;
            var screenPos = new Vector3(rect.center.x * ratio, Screen.height - rect.center.y * ratio, 0);
            imageAnimation.transform.position = screenPos;
        }
        else if (targetEntity == _rightEntity)
        {
            var rect = _entityImageRight.worldBound;
            var screenPos = new Vector3(rect.center.x * ratio, Screen.height - rect.center.y * ratio, 0);
            imageAnimation.transform.position = screenPos;
        }

        await imageAnimation.PlayAnimationAsync();
        ImageAnimationPool.Instance.ReturnToPool("ImageAnimation", imageAnimation.gameObject);
    }

    private async UniTask AnimateEntityStepAsync(Entity targetEntity)
    {
        VisualElement targetImage = targetEntity == _leftEntity ? _entityImageLeft : _entityImageRight;

        const float stepDistance = 30f;
        const float animationDuration = 0.2f;

        if (targetEntity == _leftEntity)
        {
            // 左エンティティの場合、MarginLeftを操作
            var originalMarginLeft = targetImage.style.marginLeft;

            await DOTween.To(
                () => targetImage.style.marginLeft.value.value,
                value => targetImage.style.marginLeft = new StyleLength(value),
                originalMarginLeft.value.value + stepDistance,
                animationDuration
            ).SetEase(Ease.OutQuad).AsyncWaitForCompletion();

            await DOTween.To(
                () => targetImage.style.marginLeft.value.value,
                value => targetImage.style.marginLeft = new StyleLength(value),
                originalMarginLeft.value.value,
                animationDuration
            ).SetEase(Ease.InQuad).AsyncWaitForCompletion();
        }
        else
        {
            // 右エンティティの場合、MarginRightを操作
            var originalMarginRight = targetImage.style.marginRight;

            await DOTween.To(
                () => targetImage.style.marginRight.value.value,
                value => targetImage.style.marginRight = new StyleLength(value),
                originalMarginRight.value.value + stepDistance,
                animationDuration
            ).SetEase(Ease.OutQuad).AsyncWaitForCompletion();

            await DOTween.To(
                () => targetImage.style.marginRight.value.value,
                value => targetImage.style.marginRight = new StyleLength(value),
                originalMarginRight.value.value,
                animationDuration
            ).SetEase(Ease.InQuad).AsyncWaitForCompletion();
        }
    }

    /// <summary>
    /// HPやMPの変化をアニメーションで表示するメソッド
    /// 一度ラベルが透明になり、値変更後に透明度を戻す
    /// </summary>
    /// <param name="label"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private async UniTask ChangeNumberWithAnimationAsync(Label label, int value)
    {
        var ChangeDuration = 0.3f;

        await DOTween.To(
            () => label.style.color.value,
            color => label.style.color = color,
            new Color(1, 1, 1, 0),
            ChangeDuration
        );

        label.text = value.ToString();

        await DOTween.To(
            () => label.style.color.value,
            color => label.style.color = color,
            new Color(1, 1, 1, 1),
            ChangeDuration
        );
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