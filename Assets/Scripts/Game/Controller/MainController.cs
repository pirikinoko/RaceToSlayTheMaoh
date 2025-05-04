using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UIToolkit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class MainController : MonoBehaviour
{
    [SerializeField]
    private Transform _objectsParent;
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

    private Dictionary<int, GameObject> _coffinObjects = new();

    private void Start()
    {
        _diceBoxComponent = GetComponent<UIDocument>().rootVisualElement.Q<DiceBoxComponent>("DiceBoxComponent");
        _diceBoxComponent.style.display = DisplayStyle.None;
        for (int i = 0; i < Constants.MaxPlayerCount; i++)
        {
            int coffinId = i + 1;
            var coffinObjectPrefab = Addressables.LoadAssetAsync<GameObject>(Constants.AssetReferenceCoffin).WaitForCompletion();
            var coffinObject = Instantiate(coffinObjectPrefab, _objectsParent);
            coffinObject.SetActive(false);
            _coffinObjects.Add(coffinId, coffinObject);
        }
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

        if (CurrentTurnPlayerEntity.IsAlive == false)
        {
            await RevivePlayerAsync(CurrentTurnPlayerEntity);
            StartNewTurnAsync().Forget();
            return;
        }

        var moves = await GetDiceResultAsync();

        CurrentTurnPlayerEntity.GetComponent<ControllableEntity>().SetMoves(moves);
        CurrentTurnPlayerEntity.GetComponent<ControllableEntity>().EnableClickMovement();

        if (CurrentTurnPlayerEntity.IsNpc)
        {
            await EnemyActer.MoveAsync(CurrentTurnPlayerEntity.GetComponent<ControllableEntity>());
        }

        TurnCount++;
    }

    private async UniTask<int> GetDiceResultAsync()
    {
        _stateController.BlackoutField();

        _diceBoxComponent.style.display = DisplayStyle.Flex;
        _diceBoxComponent.StartRolling();
        _stopButton.style.display = DisplayStyle.Flex;
        if (_userController.MyEntity == CurrentTurnPlayerEntity || CurrentTurnPlayerEntity.IsNpc)
        {
            _stopButton.style.display = DisplayStyle.None;
        }

        if (CurrentTurnPlayerEntity.IsNpc)
        {
            EnemyActer.StopRolling(_diceBoxComponent);
        }

        await UniTask.WaitUntil(() => _diceBoxComponent.IsRolling == false);

        _diceBoxComponent.style.display = DisplayStyle.None;

        _stateController.RevealField();
        return _diceBoxComponent.GetCurrentNumber();
    }

    private async UniTask RevivePlayerAsync(Entity entity)
    {
        await UniTask.Delay(1000);
        entity.IsAlive = true;
        entity.ChangeVisibility(true);
        _coffinObjects[entity.Id].SetActive(false);
    }

    public void SetPlayerAsDead(Entity playerEntity)
    {
        playerEntity.IsAlive = false;
        playerEntity.ChangeVisibility(false);
        _coffinObjects[playerEntity.Id].SetActive(true);
        _coffinObjects[playerEntity.Id].transform.position = playerEntity.transform.position;
    }
}
