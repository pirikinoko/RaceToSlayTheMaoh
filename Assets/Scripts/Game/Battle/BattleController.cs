using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleController : MonoBehaviour
{
    private StateController _stateController;
    private UserController _userController;
    private MainController _mainController;
    private BattleLogController _battleLogController;
    private PlayerController _playerController;
    private EnemyController _enemyController;
    private ImageAnimationHolder _imageAnimationHolder;

    private BattleStatus _battleStatus;

    private Entity _leftEntity;
    private Entity _rightEntity;
    private Entity _currentTurnEntity;
    private Entity _waitingTurnEntity;
    private Entity _winnerEntity;
    private Entity _loserEntity;

    private Vector2 _entityLeftsPreviousPos;

    private int _turnCount = 0;
    private bool _hasActionEnded;

    private VisualElement _root;
    private VisualElement _battleElement;
    private VisualElement _rewardElement;
    private VisualElement _actionElement;
    private VisualElement _commanndView;
    private VisualElement _skillView;
    private VisualElement _skillScrollContainer;

    private Label _turnCountLabel;

    private Label _entityNameLeft;
    private Label _entityNameRight;

    private VisualElement _entityImageLeft;
    private VisualElement _entityImageRight;

    private Button _closeSkillScrollViewButton;
    private List<(Button, int)> _skillButtons = new();

    private Label _healthLabelLeft;
    private Label _manaLabelLeft;
    private Label _healthLabelRight;
    private Label _manaLabelRight;

    private CompositeDisposable _disposable = new();

    private Reward _currentReward;
    private List<Skill> _currentRewardSkills;

    private void Start()
    {
        InitializeUIElements();
        InitializeBattleLogController();
    }

    public void Initialize(StateController stateController, MainController mainController, UserController userController, BattleLogController battleLogController, PlayerController playerController, EnemyController enemyController, ImageAnimationHolder imageAnimationHolder)
    {
        _stateController = stateController;
        _mainController = mainController;
        _userController = userController;
        _battleLogController = battleLogController;
        _playerController = playerController;
        _enemyController = enemyController;
        _imageAnimationHolder = imageAnimationHolder;
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
        _turnCountLabel = _root.Q<Label>("Label-TurnCount");
        _skillView = _root.Q<VisualElement>("SkillView");
        _skillScrollContainer = _root.Q<VisualElement>("unity-content-container");

        _root.Q<Button>("Button-Attack").clicked += Attack;
        _root.Q<Button>("Button-Skill").clicked += OnOpenSkillScrollClicked;
        _closeSkillScrollViewButton = _root.Q<Button>("Button-CloseSkillScroll");
        _closeSkillScrollViewButton.clicked += OnCloseSkillScrollClicked;
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
        var rootLeftElement = _root.Q<VisualElement>("Element-Left");
        _entityImageLeft = rootLeftElement.Q<VisualElement>("Image-Entity");
        _entityImageLeft.style.backgroundImage = _leftEntity.Parameter.IconSprite.texture;
        _entityNameLeft = rootLeftElement.Q<Label>("Label-EntityName");
        _entityNameLeft.text = _leftEntity.name;
        _healthLabelLeft = rootLeftElement.Q<VisualElement>("Element-HitPoint").Q<Label>("Label");
        _manaLabelLeft = rootLeftElement.Q<VisualElement>("Element-ManaPoint").Q<Label>("Label");

        _healthLabelLeft.text = _leftEntity.Parameter.HitPoint.ToString();
        _manaLabelLeft.text = _leftEntity.Parameter.ManaPoint.ToString();

        var rootRightElement = _root.Q<VisualElement>("Element-Right");
        _entityImageRight = rootRightElement.Q<VisualElement>("Image-Entity");
        _entityImageRight.style.backgroundImage = _rightEntity.Parameter.IconSprite.texture;
        _entityNameRight = rootRightElement.Q<Label>("Label-EntityName");
        _entityNameRight.text = _rightEntity.name;
        _healthLabelRight = rootRightElement.Q<VisualElement>("Element-HitPoint").Q<Label>("Label");
        _manaLabelRight = rootRightElement.Q<VisualElement>("Element-ManaPoint").Q<Label>("Label");

        _healthLabelRight.text = _rightEntity.Parameter.HitPoint.ToString();
        _manaLabelRight.text = _rightEntity.Parameter.ManaPoint.ToString();

        _winnerEntity = null;
        _loserEntity = null;

        // アクションエレメントの位置を左側に初期化
        ResetActionElementPosition();
        // リアクティブプロパティの監視を設定
        InitializeReactiveProperties();
    }

    private void SetSkillButtons(Entity entity)
    {
        _skillButtons.Clear();
        _skillScrollContainer.Clear();

        foreach (var skill in entity.Parameter.Skills)
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
            button.SetEnabled(entity.Parameter.ManaPoint >= manaCost);
        }
    }

    public void StartBattle(Entity leftEntity, Entity rightEntity, Vector2 entityLeftsPreviousPos)
    {
        _turnCount = 0;
        _turnCountLabel.text = $"1/{Constants.MaxTurn}";
        _battleStatus = BattleStatus.BeforeAction;
        _entityLeftsPreviousPos = entityLeftsPreviousPos;

        DisplayBattleElement();
        CloseCommandView();
        CloseSkillScroll();
        CloseRewardView();

        _leftEntity = _currentTurnEntity = leftEntity;
        _rightEntity = _waitingTurnEntity = rightEntity;

        SetEntities();

        _battleLogController.ClearLogs();
        _battleLogController.AddLog(Constants.GetSentenceWhenStartBattle(Settings.Language.ToString(), _leftEntity.name, _rightEntity.name));
    }

    private void StartNewTurn()
    {
        _hasActionEnded = false;
        ApplyCurrentBattleStatus(isInResult: false);
        bool isFirstTurn = _turnCount == 0;

        // _currentTurnEntityと_waitingTurnEntityを入れ替える
        if (!isFirstTurn)
        {
            Entity tmp = _currentTurnEntity;
            _currentTurnEntity = _waitingTurnEntity;
            _waitingTurnEntity = tmp;
        }

        // ローカルモードまたはユーザーのターンの場合、コマンドビューを開く
        if (_mainController.GameMode == GameMode.Local || _currentTurnEntity == _userController.MyEntity)
        {
            OpenCommandView();
            SetSkillButtons(_currentTurnEntity);
            ToggleSkillButonClickable(_currentTurnEntity);
        }

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

        // プレイヤー同士の戦いの場合は,操作プレイヤーに近い位置にコマンドビューを出す
        if (_leftEntity.EntityType == EntityType.Player && _rightEntity.EntityType == EntityType.Player)
        {
            RepositionActionElementForCurrentTurnPlayer();
        }

        // ターン数を更新
        if (_currentTurnEntity == _leftEntity)
        {
            _turnCount++;
        }

        _turnCountLabel.text = $"Turn:{_turnCount}";
    }


    private void BackToField()
    {
        HandleDiedEntity(_loserEntity);
        _stateController.ChangeState(State.Field);
    }

    public void Attack()
    {
        CloseCommandView();
        // 攻撃時のアニメーションを再生
        RunAttackProcessAsync().Forget();
    }

    private async UniTask RunAttackProcessAsync()
    {
        // 「○○の攻撃！」のログ
        _battleLogController.AddLog(Constants.GetAttackSentence(Settings.Language, _currentTurnEntity.name));
        // ステップアニメーションを実行
        await AnimateEntityStepAsync(_currentTurnEntity);
        // 攻撃時のエフェクトを再生
        await PlayImageAnimationAsync(Constants.ImageAnimationKeySlash, _waitingTurnEntity);

        // ダメージ計算処理
        int damage = _currentTurnEntity.Attack(_waitingTurnEntity);

        // 結果のログ
        _battleLogController.AddLog(Constants.GetAttackResultSentence(Settings.Language, _waitingTurnEntity.name, damage));

        OnActionEnded();
    }

    public void UseSkill(string skillName)
    {
        CloseCommandView();
        RunOnUsingSkillProcessAsync(skillName).Forget();
    }

    private async UniTask RunOnUsingSkillProcessAsync(string skillName)
    {
        // 「○○のヒール！」(例)のログ
        _battleLogController.AddLog(Constants.GetSkillSentence(Settings.Language, _currentTurnEntity.name, skillName));

        // ステップアニメーションを実行
        await AnimateEntityStepAsync(_currentTurnEntity);

        // 相手のHPが減るかそれとも自分のHPが減るかでエフェクトをどちらに出すかを決める(1)
        int enemyHp = _waitingTurnEntity.Parameter.HitPoint;

        // スキルの使用
        Skill.SkillResult result = _currentTurnEntity.UseSkill(skillName, _currentTurnEntity, _waitingTurnEntity);

        // 相手のHPが減るかそれとも自分のHPが減るかでエフェクトをどちらに出すかを決める(2)
        bool hasEnemyTakenDamage = enemyHp > _waitingTurnEntity.Parameter.HitPoint;

        // スキルのエフェクトを再生
        if (hasEnemyTakenDamage)
        {
            await PlayImageAnimationAsync(result.EffectKey, _waitingTurnEntity);
        }
        else
        {
            await PlayImageAnimationAsync(result.EffectKey, _currentTurnEntity);
        }

        // スキルの結果のログ
        foreach (var log in result.Logs)
        {
            _battleLogController.AddLog(log);
        }

        OnActionEnded();
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
        ApplyCurrentBattleStatus(isInResult: false);
        AddLogByCurrentBattleStatus();
    }

    private void OnAllLogsRead()
    {
        if (_battleStatus == BattleStatus.AfterAction || _turnCount == 0)
        {
            StartNewTurn();
            return;
        }
        switch (_battleStatus)
        {
            case BattleStatus.LeftWin or BattleStatus.RightWin:
                _winnerEntity = (_leftEntity.Parameter.HitPoint > 0) ? _leftEntity : _rightEntity;
                _loserEntity = (_leftEntity.Parameter.HitPoint > 0) ? _rightEntity : _leftEntity;

                if (_winnerEntity.EntityType == EntityType.Player)
                {
                    HideBattleElement();
                    OpenRewardView();
                    SetRewards();
                    _battleLogController.AddLog(Constants.GetSentenceWhenSelectingReward(Settings.Language, _winnerEntity == _userController.MyEntity, _winnerEntity.name));
                    _battleStatus = BattleStatus.SelectReword;
                    if (_winnerEntity.IsNpc)
                    {
                        NpcActionController.SelectReword(this, _battleLogController).Forget();
                    }
                }
                else
                {
                    _battleLogController.AddLog(Constants.GetSentenceWhenEnemyWins(Settings.Language, _winnerEntity.name));
                    _battleStatus = BattleStatus.Ending;
                }
                break;

            case BattleStatus.BothDied:
                _battleLogController.AddLog(Constants.GetSentenceWhenBothDied(Settings.Language));
                _battleStatus = BattleStatus.Ending;
                break;

            case BattleStatus.Ending:
                _disposable.Dispose();
                _disposable = new CompositeDisposable();
                BackToField();
                break;
        }
    }

    /// <summary>
    ///  現在のバトルステータスを判定する
    /// </summary>
    /// <param name="isInResult"></param>
    private void ApplyCurrentBattleStatus(bool isInResult)
    {
        if (isInResult)
        {
            _battleStatus = BattleStatus.SelectReword;
        }
        else if (_leftEntity.Parameter.HitPoint <= 0 && _rightEntity.Parameter.HitPoint <= 0)
        {
            _battleStatus = BattleStatus.BothDied;
        }
        else if (_leftEntity.Parameter.HitPoint <= 0)
        {
            _battleStatus = BattleStatus.RightWin;
        }
        else if (_rightEntity.Parameter.HitPoint <= 0)
        {
            _battleStatus = BattleStatus.LeftWin;
        }
        else if (!_hasActionEnded)
        {
            _battleStatus = BattleStatus.BeforeAction;
        }
        else
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
                _battleLogController.AddLog(Constants.GetResultSentence(Settings.Language, _leftEntity.name, _rightEntity.name));
                break;
            case BattleStatus.RightWin:
                _battleLogController.AddLog(Constants.GetResultSentence(Settings.Language, _rightEntity.name, _leftEntity.name));
                break;
            case BattleStatus.BothDied:
                _battleLogController.AddLog(Constants.GetSentenceWhenBothDied(Settings.Language));
                break;
            case BattleStatus.TurnOver:
                _battleLogController.AddLog(Constants.GetSentenceWhenTurnOver(Settings.Language));
                break;
        }
    }

    public void OnStatusRewardSelected()
    {
        string result = _currentReward.Execute(_winnerEntity);
        _battleLogController.AddLog(result);
        CloseRewardView();
        _battleStatus = BattleStatus.Ending;
    }

    public void OnSkillRewardSelected(int index)
    {
        AddSkillToWinnerEntity(_currentRewardSkills[index], out string log);
        _battleLogController.AddLog(log);
        CloseRewardView();
        _battleStatus = BattleStatus.Ending;
    }

    public List<int> RewardChoices
    {
        get
        {
            var rewardChoices = new List<int> { 0 };
            for (int i = 0; i < _currentRewardSkills.Count; i++)
            {

                if (_winnerEntity.Parameter.Skills.Find(s => s.Name == _currentRewardSkills[i].Name) == null)
                {
                    rewardChoices.Add(i + 1);
                }
            }
            return rewardChoices;
        }
    }


    private void SetRewards()
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

        // スキル報酬のセット
        List<Skill> loserSkills = _loserEntity.Parameter.Skills;
        _currentRewardSkills = GetListOfRandomTwoElementsFromList(loserSkills);

        for (int i = 0; i < _currentRewardSkills.Count; i++)
        {
            int index = i;
            var skillRewardButton = new Button(() => OnSkillRewardSelected(index));
            skillRewardButton.AddToClassList(ClassNames.RewardButton);
            var labelRewardTitle = CreateNewLabelWithDetails(ClassNames.RewardTitle, _currentRewardSkills[index].Name);
            skillRewardButton.Add(labelRewardTitle);
            var labelRewardDescription = CreateNewLabelWithDetails(ClassNames.RewardDescription, _currentRewardSkills[index].Description);
            skillRewardButton.Add(labelRewardDescription);
            if (_winnerEntity.Parameter.Skills.Find(s => s.Name == _currentRewardSkills[index].Name) != null)
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
            button.SetEnabled(_winnerEntity == _userController.MyEntity || (_mainController.GameMode == GameMode.Local && !_winnerEntity.IsNpc));
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
        _winnerEntity.Parameter.Skills.Add(skill);
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

    private void DisplayBattleElement()
    {
        _battleElement.style.display = DisplayStyle.Flex;
    }

    private void HideBattleElement()
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

    private void OpenRewardView()
    {
        _rewardElement.style.display = DisplayStyle.Flex;
    }
    private void CloseRewardView()
    {
        _rewardElement.style.display = DisplayStyle.None;
    }


    /// <summary>
    ///  コマンド入力のUIを現在のターンプレイヤーに合わせて再配置する
    /// </summary>
    private void RepositionActionElementForCurrentTurnPlayer()
    {
        var parent = _actionElement.parent;
        var index = parent.IndexOf(_actionElement);

        // 一度アクションエレメントを削除
        parent.RemoveAt(index);
        if (_currentTurnEntity == _leftEntity)
        {
            // 左エンティティのターンの場合、アクションエレメントを最初に追加
            parent.Insert(0, _actionElement);
            return;
        }

        parent.Add(_actionElement);
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
    private void InitializeReactiveProperties()
    {
        var delayOnHpChangeMills = 0;
        var delayOnManaChangeMills = 0;

        _leftEntity.HitPointRp.Subscribe(async newHp =>
        {
            int oldHp = _leftEntity.Parameter.HitPoint;

            if (oldHp == newHp)
            {
                return;
            }

            await UniTask.Delay(delayOnHpChangeMills);
            ChangeNumberWithAnimationAsync(_healthLabelLeft, newHp).Forget();
        }).AddTo(_disposable);

        _leftEntity.ManaPointRp.Subscribe(async newMp =>
        {
            int oldMp = _leftEntity.Parameter.ManaPoint;

            if (oldMp == newMp)
            {
                return;
            }
            await UniTask.Delay(delayOnManaChangeMills);
            ChangeNumberWithAnimationAsync(_manaLabelLeft, newMp).Forget();
        }).AddTo(_disposable);

        _rightEntity.HitPointRp.Subscribe(async newHp =>
        {
            int oldHp = _rightEntity.Parameter.HitPoint;

            if (oldHp == newHp)
            {
                return;
            }

            await UniTask.Delay(delayOnHpChangeMills);
            ChangeNumberWithAnimationAsync(_healthLabelRight, newHp).Forget();
        }).AddTo(_disposable);

        _rightEntity.ManaPointRp.Subscribe(async newMp =>
        {
            int oldMp = _rightEntity.Parameter.ManaPoint;

            if (oldMp == newMp)
            {
                return;
            }

            await UniTask.Delay(delayOnManaChangeMills);
            ChangeNumberWithAnimationAsync(_manaLabelRight, newMp).Forget();
        }).AddTo(_disposable);
    }

    private async UniTask PlayImageAnimationAsync(string key, Entity targetEntity)
    {
        var imageAnimation = ImageAnimationPool.Instance.GetFromPool<ImageAnimation>("ImageAnimation");
        imageAnimation.SetSprites(_imageAnimationHolder.GetSpriteps(key));

        if (targetEntity == _leftEntity)
        {
            var rect = _entityImageLeft.worldBound;
            var screenPos = new Vector3(rect.center.x, Screen.height - rect.center.y, 0);
            imageAnimation.transform.position = screenPos;
        }
        else if (targetEntity == _rightEntity)
        {
            var rect = _entityImageRight.worldBound;
            var screenPos = new Vector3(rect.center.x, Screen.height - rect.center.y, 0);
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
}