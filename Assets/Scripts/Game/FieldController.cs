using System.Collections.Generic;
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
        var allEntity = new List<Entity>();
        allEntity.AddRange(_playerController._playerList);
        allEntity.AddRange(_enemyController._enemyList);

        foreach (var entityLeft in allEntity)
        {
            foreach (var entityRight in allEntity)
            {
                if (entityLeft == entityRight)
                {
                    continue;
                }

                if (Vector3.Distance(entityLeft.transform.position, entityRight.transform.position) == 0)
                {
                    _stateController.ChangeState(State.Battle);
                    _battleController.StartBattle(entityLeft, entityRight);
                }
            }
        }
    }

}
