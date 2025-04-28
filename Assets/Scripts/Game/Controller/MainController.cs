using Cysharp.Threading.Tasks;
using System.Linq;
using TMPro;
using UIToolkit;
using UnityEngine;
using UnityEngine.UIElements;

public class MainController : MonoBehaviour
{
    private UserController _userController;
    private FieldController _fieldController;
    private CameraController _cameraController;
    private StateController _stateController;
    private PlayerController _playerController;
    private EnemyController _enemyController;

    public int TurnCount { get; private set; } = 0;

    private GameMode _gameMode;
    public GameMode GameMode { get => _gameMode; set => _gameMode = value; }

    private int _playerCount = 1;
    public int PlayerCount { get => _playerCount; set => _playerCount = value; }

    private int _currentTurnPlayerId = 1;
    public int CurrentTurnPlayerId { get => _currentTurnPlayerId; private set => _currentTurnPlayerId = value; }

    private Entity _currentTurnPlayerEntity;
    public Entity CurrentTurnPlayerEntity { get => _currentTurnPlayerEntity; private set => _currentTurnPlayerEntity = value; }

    private DiceBoxComponent _diceBoxComponent;


    private Button _stopButton => _diceBoxComponent.StopButton;

    private void Start()
    {
        _diceBoxComponent = GetComponent<UIDocument>().rootVisualElement.Q<DiceBoxComponent>("DiceBoxComponent");
        _diceBoxComponent.style.display = DisplayStyle.None;
    }

    public void Initialize(UserController userController, FieldController fieldController, CameraController cameraController, StateController stateController, PlayerController playerController, EnemyController enemyController)
    {
        _userController = userController;
        _fieldController = fieldController;
        _cameraController = cameraController;
        _stateController = stateController;
        _playerController = playerController;
        _enemyController = enemyController;
    }

    public async UniTask InitializeGame()
    {
        await _playerController.InitializePlayersAsync();
        await _enemyController.InitializeAllEnemiesAsync();
    }

    public async UniTask StartNewTurnAsync()
    {
        if (TurnCount != 0)
        {
            CurrentTurnPlayerId++;
            if (CurrentTurnPlayerId > PlayerCount)
            {
                CurrentTurnPlayerId = 1;
            }
        }

        _fieldController.UpdateStatusBoxesAsync().Forget();
        CurrentTurnPlayerEntity = _playerController.PlayerList.FirstOrDefault(p => p.Id == CurrentTurnPlayerId);

        await _cameraController.MoveCameraAsync(CurrentTurnPlayerEntity.transform.position);

        var moves = await GetDiceResultAsync();
        CurrentTurnPlayerEntity.GetComponent<ControllableEntity>().SetMoves(moves);
        CurrentTurnPlayerEntity.GetComponent<ControllableEntity>().EnableClickMovement();

        TurnCount++;
    }

    private async UniTask<int> GetDiceResultAsync()
    {
        _stateController.BlackoutField();

        _diceBoxComponent.style.display = DisplayStyle.Flex;
        _diceBoxComponent.StartRolling();
        if (_userController.MyEntity == CurrentTurnPlayerEntity || GameMode == GameMode.Local)
        {
            _stopButton.style.display = DisplayStyle.Flex;
        }

        await UniTask.WaitUntil(() => _diceBoxComponent.IsRolling == false);
        _diceBoxComponent.style.display = DisplayStyle.None;

        _stateController.RevealField();
        return _diceBoxComponent.GetCurrentNumber();
    }
}
