using Cysharp.Threading.Tasks;
using System.Linq;
using TMPro;
using UIToolkit;
using UnityEngine;
using UnityEngine.UIElements;

public class MainContrller : MonoBehaviour
{
    [SerializeField]
    private UserController _userController;
    [SerializeField]
    private CameraController _cameraController;
    [SerializeField]
    private StateController _stateController;
    [SerializeField]
    private PlayerController _playerController;
    [SerializeField]
    private EnemyController _enemyController;

    public int TurnCount { get; private set; } = 0;

    private int _playerCount = 1;
    private int _currentTurnPlayerId = 1;
    private Entity _currentTurnPlayerEntity;
    private DiceBoxComponent _diceBoxComponent;

    private Button _stopButton => _diceBoxComponent.StopButton;

    private void Start()
    {
        _diceBoxComponent = GetComponent<UIDocument>().rootVisualElement.Q<DiceBoxComponent>("DiceBoxComponent");
        _diceBoxComponent.style.display = DisplayStyle.None;
    }

    public async UniTask InitializeGame()
    {
        await _playerController.InitializePlayersAsync();
        await _enemyController.InitializeAllEnemiesAsync();
    }

    public async UniTask StartNewTurnAsync()
    {
        _currentTurnPlayerId = _currentTurnPlayerId == _playerCount ? 1 : _currentTurnPlayerId + 1;

        _currentTurnPlayerEntity = _playerController.PlayerList.FirstOrDefault(p => p.Id == _currentTurnPlayerId);

        await _cameraController.MoveCameraAsync(_currentTurnPlayerEntity.transform.position);

        var moves = await GetDiceResultAsync();
        _currentTurnPlayerEntity.GetComponent<ControllableCharacter>().SetMoves(moves);

        TurnCount++;
    }

    private async UniTask<int> GetDiceResultAsync()
    {
        _stateController.BlackoutField();

        _diceBoxComponent.style.display = DisplayStyle.Flex;
        _diceBoxComponent.StartRolling();
        if (_userController.MyEntity == _currentTurnPlayerEntity)
        {
            _stopButton.style.display = DisplayStyle.Flex;
        }

        await UniTask.WaitUntil(() => _diceBoxComponent.IsRolling == false);
        _diceBoxComponent.style.display = DisplayStyle.None;

        _stateController.RevealField();
        return _diceBoxComponent.GetCurrentNumber();
    }

    public Entity GetCurrentTurnPlayerEntity()
    {
        return _currentTurnPlayerEntity;
    }
}
