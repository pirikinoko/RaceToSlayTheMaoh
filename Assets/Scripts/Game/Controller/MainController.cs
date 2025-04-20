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

    public int TurnCount { get; private set; } = 0;

    private int _playerCount = 1;
    private int _currentTurnPlayerId = 1;
    private Entity _currentTurnPlayerEntity;
    public async UniTask InitializeGame()
    {
        await _playerController.InitializePlayersAsync();
        await _enemyController.InitializeAllEnemiesAsync();
    }

    public async UniTask StartNewTurnAsync()
    {
        _currentTurnPlayerId = _currentTurnPlayerId == _playerCount ? 1 : _currentTurnPlayerId + 1;

        _currentTurnPlayerEntity = _playerController.PlayerList.FirstOrDefault(p => p.Id == _currentTurnPlayerId);
        _currentTurnPlayerEntity.GetComponent<ControllableCharacter>().SetMoves(GetMovesByRandom());

        await _cameraController.MoveCameraAsync(_currentTurnPlayerEntity.transform.position);

        TurnCount++;
    }

    private int GetMovesByRandom()
    {
        return Random.Range(1, Constants.MaxMoves + 1);
    }

    public Entity GetCurrentTurnPlayerEntity()
    {
        return _currentTurnPlayerEntity;
    }
}
