using UnityEngine;
using UnityEngine.UIElements;

public class BattleController : MonoBehaviour
{
    [SerializeField]
    private StateController _stateController;

    private Entity _leftEntity;
    private Entity _rightEntity;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Button>("Button-Attack").clicked += Attack;
        root.Q<Button>("Button-Deffence").clicked += Deffence;
        root.Q<Button>("Button-Skill").clicked += UseSkill;
    }

    public void StartBattle(Entity left, Entity right)
    {
        _leftEntity = left;
        _rightEntity = right;
    }

    private void EndBattle()
    {
        _stateController.ChangeState(State.Field);
    }


    private void Attack()
    {
    }
    private void Deffence()
    {
    }
    private void UseSkill()
    {
    }
}
