using UnityEngine;
using UnityEngine.UIElements;

public class BattleController : MonoBehaviour
{
    [SerializeField]
    private StateController _stateController;

    private Player _player;
    private Enemy _enemy;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
    }

    public void StartBattle(Player player, Enemy enemy)
    {
        _player = player;
        _enemy = enemy;
    }

    private void EndBattle()
    {
        _stateController.ChangeState(State.Field);
    }
}
