using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

public class MainContrller : MonoBehaviour
{
    [SerializeField]
    private CameraController _cameraController;
    [SerializeField]
    private PlayerController _playerController;
    [SerializeField]
    private EnemyController _enemyController;

    private int _playerCount = 1;
    private int _currentTurnPlayerId = 1;

    public async UniTask StartTurn()
    {
        await _playerController.InitializePlayersAsync();
        await _enemyController.InitializeAllEnemiesAsync();

        var player = _playerController.PlayerList.FirstOrDefault(p => p.Id == _currentTurnPlayerId);

        await _cameraController.MoveCamera(player.transform.position);
    }
}
