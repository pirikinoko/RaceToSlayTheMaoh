using Cysharp.Threading.Tasks;
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

    private async UniTask Start()
    {
        while (true)
        {
            await UniTask.Delay(1000);

            if (_stateController.CurrentState != State.Field)
            {
                await UniTask.Delay(3000);
                continue;
            }
            CheckEncount();
        }
    }

    private void CheckEncount()
    {
        var allEntity = new List<Entity>();
        allEntity.AddRange(_playerController.PlayerList);
        allEntity.AddRange(_enemyController._enemyList);

        foreach (var entityLeft in allEntity)
        {
            foreach (var entityRight in allEntity)
            {
                if (entityLeft == entityRight || (entityLeft.EntityType != EntityType.Player && entityRight.EntityType != EntityType.Player))
                {
                    continue;
                }

                if (Vector2.Distance(entityLeft.transform.position, entityRight.transform.position) < 0.1f)
                {
                    _stateController.ChangeState(State.Battle);

                    // プレイヤーと敵が戦う場合はプレイヤーを左側にする
                    if (entityLeft.EntityType != EntityType.Player && entityRight.EntityType == EntityType.Player)
                    {
                        _battleController.StartBattle(entityRight, entityLeft);
                    }
                    _battleController.StartBattle(entityLeft, entityRight);
                    return;
                }
            }
        }
    }
}
