using UnityEngine;
using UnityEngine.UIElements;

public class BattleController : MonoBehaviour
{
    [SerializeField]
    private StateController _stateController;
    [SerializeField]
    private MainController _mainController;

    private Entity _leftEntity;
    private Entity _rightEntity;
    private Entity _currentTurnEntity;
    private Entity _waitingTurnEntity;

    private int _turn = 1;
    private Phase _phase = Phase.WaitForAction;

    private VisualElement _commanndView;
    private VisualElement _skillScrollView;


    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _commanndView = root.Q<VisualElement>("CommandView");
        _skillScrollView = root.Q<VisualElement>("SkillScrollView");
        root.Q<Button>("Button-Attack").clicked += Attack;
        root.Q<Button>("Button-Skill").clicked += OpenSkillScroll;
    }

    public void StartBattle(Entity left, Entity right)
    {
        _leftEntity = left;
        _rightEntity = right;
        _currentTurnEntity = _leftEntity;
        _waitingTurnEntity = _rightEntity;
    }

    private void EndBattle()
    {
        _stateController.ChangeState(State.Field);
    }

    private void StartTurn()
    {

    }

    private void EndTurn()
    {
        _currentTurnEntity = _currentTurnEntity == _leftEntity ? _rightEntity : _leftEntity;
        _waitingTurnEntity = _waitingTurnEntity == _leftEntity ? _rightEntity : _leftEntity;
        _turn++;
    }

    private void Attack()
    {
        _currentTurnEntity.Attack(_waitingTurnEntity);
        _commanndView.style.display = DisplayStyle.None;
    }

    private void OpenSkillScroll()
    {
        _skillScrollView.style.display = DisplayStyle.Flex;
    }

    private void CloseSkillScroll()
    {
        _skillScrollView.style.display = DisplayStyle.None;
    }
}
