using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIToolkit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class BattleController : MonoBehaviour
{
    [SerializeField]
    private StateController _stateController;
    [SerializeField]
    private UserController _userController;
    [SerializeField]
    private BattleLogController _battleLogController;
    [SerializeField]
    private PlayerController _playerController;
    [SerializeField]
    private EnemyController _enemyController;
    [SerializeField]
    private ImageAnimationHolder _imageAnimationHolder;
    [SerializeField]
    private Canvas _overlayCanvas;

    private BattleStatus _battleStatus;

    private Entity _leftEntity;
    private Entity _rightEntity;
    private Entity _currentTurnEntity;
    private Entity _waitingTurnEntity;
    private Entity _winnerEntity;
    private Entity _loserEntity;

    private int _turnCount = 0;
    private bool _hasActionEnded;

    private VisualElement _root;
    private VisualElement _battleElement;
    private VisualElement _rewardElement;
    private VisualElement _commanndView;
    private VisualElement _skillScrollView;
    private VisualElement _skillContainer;

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

    private void Start()
    {
        InitializeUIElements();
        InitializeBattleLogController();
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

        _commanndView = _root.Q<VisualElement>("CommandView");
        _turnCountLabel = _root.Q<Label>("Label-TurnCount");
        _skillScrollView = _root.Q<VisualElement>("SkillScrollView");
        _skillContainer = _root.Q<VisualElement>("unity-content-container");

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
            if (_battleStatus == BattleStatus.AfterAction || _turnCount == 0)
            {
                StartNewTurn();
            }
            else if (_battleStatus is BattleStatus.LeftWin or BattleStatus.RightWin
            or BattleStatus.TurnOver or BattleStatus.BothDied)
            {
                GoToNextStatus();
            }
            else if (_battleStatus == BattleStatus.SelectReword)
            {
                HideBattleElement();
                OpenRewardView();
                SetRewards();
            }
            else if (_battleStatus == BattleStatus.Ending)
            {
                BackToField();
            }
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

        // リアクティブプロパティの監視を設定
        InitializeReactiveProperties();
    }


    private void SetSkillButtons()
    {
        _skillButtons.Clear();
        _skillContainer.Clear();

        foreach (var skill in _userController.MyEntity.Parameter.Skills)
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
            _skillContainer.Add(skillButton);
            _skillButtons.Add((skillButton, skill.ManaCost));
        }
    }

    private void ToggleSkillButonClickable()
    {
        foreach (var (button, manaCost) in _skillButtons)
        {
            button.SetEnabled(_userController.MyEntity.Parameter.ManaPoint >= manaCost);
        }
    }

    public void StartBattle(Entity left, Entity right)
    {
        _turnCount = 0;
        _battleStatus = BattleStatus.BeforeAction;

        DisplayBattleElement();
        CloseCommandView();
        CloseSkillScroll();
        CloseRewardView();

        _leftEntity = _currentTurnEntity = left;
        _rightEntity = _waitingTurnEntity = right;

        SetEntities();
        SetSkillButtons();

        _battleLogController.AddLog(Constants.GetSentenceWhenStartBattle(Settings.Language.ToString(), _leftEntity.name, _rightEntity.name));
    }

    private void StartNewTurn()
    {
        ApplyCurrentBattleStatus(isInResult: false);
        bool isFirstTurn = _turnCount == 0;
        if (!isFirstTurn)
        {
            Entity tmp = _currentTurnEntity;
            _currentTurnEntity = _waitingTurnEntity;
            _waitingTurnEntity = tmp;
        }

        if (_currentTurnEntity == _userController.MyEntity)
        {
            OpenCommandView();
            ToggleSkillButonClickable();
        }

        if (_currentTurnEntity.Parameter.EntityType == EntityType.Player)
        {
            _battleLogController.SetText(Constants.GetSentenceWhileWaitingAction(Settings.Language.ToString(), _currentTurnEntity.name));
        }
        else
        {
            EnemyActer.ActAsync(this, _currentTurnEntity).Forget();
        }

        _turnCount++;
        _turnCountLabel.text = $"{_turnCount}/{Constants.MaxTurn}";
    }

    private void GoToNextStatus()
    {
        switch (_battleStatus)
        {
            case BattleStatus.LeftWin:
                _winnerEntity = _leftEntity;
                _loserEntity = _rightEntity;
                if (_winnerEntity.EntityType == EntityType.Player)
                {
                    _battleStatus = BattleStatus.SelectReword;
                }
                _battleLogController.AddLog(Constants.GetResultSentence(Settings.Language, _leftEntity.name, _rightEntity.name));
                break;
            case BattleStatus.RightWin:
                _winnerEntity = _rightEntity;
                _loserEntity = _leftEntity;
                if (_winnerEntity.EntityType == EntityType.Player)
                {
                    _battleStatus = BattleStatus.SelectReword;
                }
                _battleLogController.AddLog(Constants.GetResultSentence(Settings.Language, _rightEntity.name, _leftEntity.name));
                break;
            case BattleStatus.SelectReword:
                _battleStatus = BattleStatus.Ending;
                break;
            case BattleStatus.BothDied:
                _battleStatus = BattleStatus.Ending;
                _battleLogController.AddLog(Constants.GetSentenceWhenBothDied(Settings.Language));
                break;
            case BattleStatus.TurnOver:
                _battleStatus = BattleStatus.Ending;
                _battleLogController.AddLog(Constants.GetSentenceWhenTurnOver(Settings.Language));
                break;
        }
    }

    private void BackToField()
    {
        RemoveEntity(_loserEntity);
        _stateController.ChangeState(State.Field);
    }

    public void Attack()
    {
        _battleLogController.AddLog(Constants.GetAttackSentence(Settings.Language, _currentTurnEntity.name));

        int damage = _currentTurnEntity.Attack(_waitingTurnEntity);

        PlayImageAnimationAsync(Constants.ImageAnimationKeySlash, _waitingTurnEntity).Forget();

        _battleLogController.AddLog(Constants.GetAttackResultSentence(Settings.Language, _waitingTurnEntity.name, damage));

        OnActionEnded();
    }

    public void UseSkill(string name)
    {
        string[] result = _currentTurnEntity.UseSkill(name, _currentTurnEntity, _waitingTurnEntity);

        _battleLogController.AddLog(Constants.GetSkillSentence(Settings.Language, _currentTurnEntity.name, name));

        foreach (var log in result)
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
        // 攻撃時のステップアニメーションを実行
        AnimateEntityStepAsync(_currentTurnEntity).Forget();

        _hasActionEnded = true;
        CloseCommandView();
        ApplyCurrentBattleStatus(isInResult: false);

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
        else if (_turnCount > Constants.MaxTurn)
        {
            _battleStatus = BattleStatus.TurnOver;
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

    private void SetRewards()
    {
        _rewardElement.Clear();
        var rewardButtons = new List<Button>();

        // ステータス報酬のセット
        var reward = new Reward();
        var statusRewardButton = new Button(() =>
        {
            string result = reward.Execute(_winnerEntity);
            _battleLogController.AddLog(result);
        });
        statusRewardButton.AddToClassList(ClassNames.RewardButton);

        statusRewardButton.Add(CreateNewLabelWithDetails(ClassNames.RewardTitle, Constants.GetStatusRewardTitle(Settings.Language)));
        statusRewardButton.Add(CreateNewLabelWithDetails(ClassNames.RewardDescription, reward.Description));
        rewardButtons.Add(statusRewardButton);

        // スキル報酬のセット
        List<Skill> loserSkills = _loserEntity.Parameter.Skills;
        List<Skill> rewardSelectedSkills = GetListOfRandomTwoElementsFromList(loserSkills);

        for (int i = 0; i < rewardSelectedSkills.Count; i++)
        {
            int index = i;
            var skillRewardButton = new Button();
            skillRewardButton.AddToClassList(ClassNames.RewardButton);
            skillRewardButton.clicked += () =>
            {
                AddSkillToMyEntity(rewardSelectedSkills[index], out string log);
                _battleLogController.AddLog(log);
            };

            skillRewardButton.Add(CreateNewLabelWithDetails(ClassNames.RewardTitle, rewardSelectedSkills[index].Name));
            skillRewardButton.Add(CreateNewLabelWithDetails(ClassNames.RewardDescription, rewardSelectedSkills[index].Description));
            rewardButtons.Add(skillRewardButton);
        }

        foreach (var button in rewardButtons)
        {
            button.clicked += CloseRewardView;
            button.clicked += () => _battleStatus = BattleStatus.Ending;
            _rewardElement.Add(button);
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
            int index1 = Random.Range(0, list.Count);
            result.Add(list[index1]);
            list.RemoveAt(index1);
        }
        if (list.Count > 0)
        {
            int index2 = Random.Range(0, list.Count);
            result.Add(list[index2]);
        }
        return result;
    }

    private void AddSkillToMyEntity(Skill skill, out string log)
    {
        _userController.MyEntity.Parameter.Skills.Add(skill);
        log = string.Format(Constants.GetSkillGetSentence(Settings.Language), _userController.MyEntity.name, skill.Name);
    }

    private void RemoveEntity(Entity entity)
    {
        Destroy(entity.gameObject);
        switch (entity.EntityType)
        {
            case EntityType.Player:
                _playerController.PlayerList.Remove(entity);
                break;
            default:
                _enemyController._enemyList.Remove(entity);
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
        _skillScrollView.style.display = DisplayStyle.Flex;
        _closeSkillScrollViewButton.style.display = DisplayStyle.Flex;
        _commanndView.style.display = DisplayStyle.None;
    }

    private void CloseSkillScroll()
    {
        _skillScrollView.style.display = DisplayStyle.None;
        _closeSkillScrollViewButton.style.display = DisplayStyle.None;
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

    // エンティティのHPとMPの変化を監視して、ダメージや回復のエフェクトを表示する
    private void InitializeReactiveProperties()
    {
        _leftEntity.HitPointRp.Subscribe(newHp =>
        {
            int oldHp = _leftEntity.Parameter.HitPoint;
            if (newHp < oldHp)
            {
                ChangeNumberWithAnimationAsync(_healthLabelLeft, newHp).Forget();
            }
            else if (newHp > oldHp)
            {
                ChangeNumberWithAnimationAsync(_healthLabelLeft, newHp).Forget();
                PopHealNumberEffectAsync(_leftEntity, newHp - oldHp).Forget();
            }
        });

        _leftEntity.ManaPointRp.Subscribe(newMp =>
        {
            int oldMp = _leftEntity.Parameter.ManaPoint;
            if (newMp < oldMp)
            {
                ChangeNumberWithAnimationAsync(_manaLabelLeft, newMp).Forget();
            }
        });

        _rightEntity.HitPointRp.Subscribe(newHp =>
        {
            int oldHp = _rightEntity.Parameter.HitPoint;
            if (newHp < oldHp)
            {
                ChangeNumberWithAnimationAsync(_healthLabelRight, newHp).Forget();
            }
            else if (newHp > oldHp)
            {
                ChangeNumberWithAnimationAsync(_healthLabelRight, newHp).Forget();
                PopHealNumberEffectAsync(_rightEntity, newHp - oldHp).Forget();
            }
        });

        _rightEntity.ManaPointRp.Subscribe(newMp =>
        {
            int oldMp = _rightEntity.Parameter.ManaPoint;
            if (newMp < oldMp)
            {
                ChangeNumberWithAnimationAsync(_manaLabelRight, newMp).Forget();
            }
        });
    }

    private async UniTask PopHealNumberEffectAsync(Entity targetEntity, int healAmount)
    {
        var healNumberEffect = NumberEffectPool.Instance.GetFromPool<HealNumberEffect>("HealNumberEffect");

        if (targetEntity == _leftEntity)
        {
            var rect = _healthLabelLeft.worldBound;
            // UIToolKitの座標はScreen座標とは異なるため、Screen座標に変換する(Screen座標は左下が原点)
            var screenPos = new Vector3(rect.center.x, Screen.height - rect.center.y, 0);
            healNumberEffect.transform.position = screenPos;
        }
        else if (targetEntity == _rightEntity)
        {
            var rect = _healthLabelRight.worldBound;
            var screenPos = new Vector3(rect.center.x, Screen.height - rect.center.y, 0);
            healNumberEffect.transform.position = screenPos;
        }

        await healNumberEffect.ShowEffect(healAmount);
        NumberEffectPool.Instance.ReturnToPool("HealNumberEffect", healNumberEffect.gameObject);
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