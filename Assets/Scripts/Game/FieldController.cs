using UnityEngine;

public class FieldController : MonoBehaviour
{
    [SerializeField]
    private StateController _stateController;
    [SerializeField]
    private EnemyController _enemyController;
    [SerializeField]
    private PlayerController _playerController;
    [SerializeField]
    private BattleController _battleController;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CheckEncount()
    {
        foreach (var enemy in _enemyController._enemyList)
        {
            foreach (var player in _playerController._playerList)
            {
                if (Vector3.Distance(enemy.transform.position, player.transform.position) == 0)
                {
                    _stateController.ChangeState(State.Battle);
                    _battleController.StartBattle(player, enemy);
                }
            }
        }
    }

}
