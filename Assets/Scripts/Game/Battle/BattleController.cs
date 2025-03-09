using R3;
using System.Collections.Generic;
using System.Linq;
using UIToolkit;
using UnityEngine;
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
    private Settings _settings;

    private BattleStatus _battleStatus;

    private Entity _leftEntity;
    private Entity _rightEntity;
    private Entity _currentTurnEntity;
    private Entity _waitingTurnEntity;
    private Entity _winnerEntity;
    private Entity _loserEntity;

    private int _turn = 1;

    private VisualElement _root;
    private VisualElement _battleElement;
    private VisualElement _rewardElement;
    private VisualElement _commanndView;
    private VisualElement _skillScrollView;

    private Button _closeSkillScrollViewButton;
    private Button[] _rewordButtons = new Button[3];

    private IndicatorBarComponent _healthBarLeft;
    private IndicatorBarComponent _manaBarLeft;
    private IndicatorBarComponent _healthBarRight;
    private IndicatorBarComponent _manaBarRight;

    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        _battleElement = _root.Q<VisualElement>("BattleElement");
        _rewardElement = _root.Q<VisualElement>("RewordElement");
        _rewordButtons = _rewardElement.Children().OfType<Button>().ToArray();
        _rewordButtons.Select(b => b.style.display = DisplayStyle.None).ToArray();

        _commanndView = _root.Q<VisualElement>("CommandView");
        _root.Q<Label>("Label-TurnCount").text = $"{_turn}/{Constants.MaxTurn}";
        _skillScrollView = _root.Q<VisualElement>("SkillScrollView");
        _skillScrollView.Clear();

        _root.Q<Button>("Button-Attack").clicked += Attack;
        _root.Q<Button>("Button-Skill").clicked += OnOpenSkillScrollClicked;
        _closeSkillScrollViewButton = _root.Q<Button>("Button-CloseSkillScroll");
        _closeSkillScrollViewButton.clicked += OnCloseSkillScrollClicked;

        _battleLogController.Initialize(_root.Q<Label>("Label-Log"));
        _battleLogController.OnAllLogsRead.Subscribe(_ =>
        {
            if (_battleStatus == BattleStatus.InProgess)
            {
                StartNextTurn();
            }
            else if (_battleStatus != BattleStatus.Result)
            {
                EndBattle();
            }
            else if (_battleStatus == BattleStatus.Result)
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
        rootLeftElement.Q<VisualElement>("Image-Entity").style.backgroundImage = _leftEntity.Parameter.IconSprite.texture;
        rootLeftElement.Q<Label>("Label-EntityName").text = _leftEntity.name;
        _healthBarLeft = rootLeftElement.Q<IndicatorBarComponent>("HealthBar");
        _manaBarLeft = rootLeftElement.Q<IndicatorBarComponent>("ManaBar");

        var rootRightElement = _root.Q<VisualElement>("Element-Right");
        rootRightElement.Q<VisualElement>("Image-Entity").style.backgroundImage = _rightEntity.Parameter.IconSprite.texture;
        rootRightElement.Q<Label>("Label-EntityName").text = _rightEntity.name;
        _healthBarRight = rootRightElement.Q<IndicatorBarComponent>("HealthBar");
        _manaBarRight = rootRightElement.Q<IndicatorBarComponent>("ManaBar");
    }

    private void SetSkillButtons()
    {
        foreach (var skill in _userController.MyEntity.Parameter.Skills)
        {
            var skillButton = new Button(() =>
            {
                UseSkill(skill.Name);
                CloseSkillScroll();
                StartNextTurn();
            })
            {
                text = skill.Name
            };
            skillButton.AddToClassList("skillbutton");
            _skillScrollView.Add(skillButton);
        }
    }


    public void StartBattle(Entity left, Entity right)
    {
        _battleStatus = BattleStatus.InProgess;

        DisplayBattleElement();
        CloseSkillScroll();
        CloseRewardView();

        _leftEntity = _currentTurnEntity = left;
        _rightEntity = _waitingTurnEntity = right;

        SetEntities();
        SetSkillButtons();

        _healthBarLeft.CurrentValue = left.Parameter.HitPoint;
        _manaBarLeft.CurrentValue = left.Parameter.ManaPoint;
    }

    private void StartNextTurn()
    {
        Entity tmp = _currentTurnEntity;
        _currentTurnEntity = _waitingTurnEntity;
        _waitingTurnEntity = tmp;
        _turn++;

        if (_currentTurnEntity == _userController.MyEntity)
        {
            OpenCommandView();
        }
        _battleLogController.SetText(Constants.GetSentenceWhileWaitingAction(_settings.Language.ToString(), _currentTurnEntity.name));
    }

    private void EndBattle()
    {
        switch (_battleStatus)
        {
            case BattleStatus.LeftWin:
                _winnerEntity = _leftEntity;
                _battleLogController.AddLog(Constants.GetResultSentence(_settings.Language, _leftEntity.name, _rightEntity.name));
                break;
            case BattleStatus.RightWin:
                _winnerEntity = _rightEntity;
                _battleLogController.AddLog(Constants.GetResultSentence(_settings.Language, _rightEntity.name, _leftEntity.name));
                break;
            case BattleStatus.BothDied:
                _battleLogController.AddLog(Constants.GetSentenceWhenBothDied(_settings.Language));
                break;
            case BattleStatus.TurnOver:
                _battleLogController.AddLog(Constants.GetSentenceWhenTurnOver(_settings.Language));
                break;
        }
    }

    private void BackToField()
    {
        _stateController.ChangeState(State.Field);
    }

    private void Attack()
    {
        _currentTurnEntity.Attack(_waitingTurnEntity);
        _battleLogController.AddLog(Constants.GetAttackSentence(_settings.Language, _currentTurnEntity.name));
        OnActionEnded();
    }

    private void UseSkill(string name)
    {
        string[] result = _currentTurnEntity.UseSkill(name, _currentTurnEntity, _waitingTurnEntity);
        _battleLogController.AddLog(Constants.GetSkillSentence(_settings.Language, _currentTurnEntity.name, name));
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
        CloseCommandView();
        _healthBarLeft.CurrentValue = _leftEntity.Parameter.HitPoint;
        _manaBarLeft.CurrentValue = _leftEntity.Parameter.ManaPoint;
        _healthBarRight.CurrentValue = _rightEntity.Parameter.HitPoint;
        _manaBarRight.CurrentValue = _rightEntity.Parameter.ManaPoint;

        ApplyCurrentBattleStatus(false);
        switch (_battleStatus)
        {
            case BattleStatus.InProgess:
                return;
            case BattleStatus.LeftWin:
                _battleLogController.AddLog(Constants.GetResultSentence(_settings.Language, _leftEntity.name, _rightEntity.name));
                break;
            case BattleStatus.RightWin:
                _battleLogController.AddLog(Constants.GetResultSentence(_settings.Language, _rightEntity.name, _leftEntity.name));
                break;
            case BattleStatus.BothDied:
                _battleLogController.AddLog(Constants.GetSentenceWhenBothDied(_settings.Language));
                break;
            case BattleStatus.TurnOver:
                _battleLogController.AddLog(Constants.GetSentenceWhenTurnOver(_settings.Language));
                break;
        }
    }

    private void ApplyCurrentBattleStatus(bool isInResult)
    {
        if (isInResult)
        {
            _battleStatus = BattleStatus.Result;
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
        else if (_turn > Constants.MaxTurn)
        {
            _battleStatus = BattleStatus.TurnOver;
        }
        else
        {
            _battleStatus = BattleStatus.InProgess;
        }
    }

    private void SetRewards()
    {
        // ステータス報酬のセット
        _rewordButtons[0].style.display = DisplayStyle.Flex;
        _rewordButtons[0].clicked += () =>
        {
            string result = new Reword().Execute(_winnerEntity);
            _battleLogController.AddLog(result);
        };

        // スキル報酬のセット
        List<Skill> loserSkills = _winnerEntity.Parameter.Skills;
        List<Skill> rewordSelectedSkills = new List<Skill>();

        int index1 = Random.Range(0, loserSkills.Count);
        rewordSelectedSkills.Add(loserSkills[index1]);
        loserSkills.RemoveAt(index1);

        int index2;
        if (loserSkills.Count != 0)
        {
            index2 = Random.Range(0, loserSkills.Count);
            rewordSelectedSkills.Add(loserSkills[index2]);
        }

        for (int i = 0; i < _winnerEntity.Parameter.Skills.Count; i++)
        {
            _rewordButtons[i + 1].style.display = DisplayStyle.Flex;
            _rewordButtons[i + 1].text = _winnerEntity.Parameter.Skills[i].Name;
            _rewordButtons[i + 1].clicked += () => GetSkill(rewordSelectedSkills[i]);
        }
    }


    private void GetSkill(Skill skill)
    {
        _userController.MyEntity.Parameter.Skills.Add(skill);
        _battleLogController.AddLog(string.Format(Constants.GetSkillGetSentence(_settings.Language), _userController.MyEntity, skill.Name));
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
    }

    private void CloseSkillScroll()
    {
        _skillScrollView.style.display = DisplayStyle.None;
        _closeSkillScrollViewButton.style.display = DisplayStyle.None;
    }

    private void OpenRewardView()
    {
        _rewardElement.style.display = DisplayStyle.Flex;
    }
    private void CloseRewardView()
    {
        _rewardElement.style.display = DisplayStyle.None;
    }
}