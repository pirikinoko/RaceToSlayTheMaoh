using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIToolkit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
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

    public Entity WinnerEntity { get; set; } = null;

    private void Start()
    {
        _diceBoxComponent = GetComponent<UIDocument>().rootVisualElement.Q<DiceBoxComponent>("DiceBoxComponent");
        _diceBoxComponent.style.display = DisplayStyle.None;
        for (int i = 0; i < Constants.MaxPlayerCountIncludingNpc; i++)
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
            if (CurrentTurnPlayerId > Constants.MaxPlayerCountIncludingNpc)
            {
                CurrentTurnPlayerId = 1;
            }
        }

        _fieldController.UpdateStatusBoxesAsync().Forget();
        CurrentTurnPlayerEntity = _playerController.PlayerList.FirstOrDefault(p => p.Id == CurrentTurnPlayerId);
        // 現在のターンのプレイヤーを最前面に表示する
        CurrentTurnPlayerEntity.gameObject.GetComponent<SpriteRenderer>().sortingOrder++;

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
            await NpcActionController.MoveAsync(CurrentTurnPlayerEntity.GetComponent<ControllableEntity>());
        }

        // レイヤーの順序を戻す
        CurrentTurnPlayerEntity.gameObject.GetComponent<SpriteRenderer>().sortingOrder--;

        if (_currentTurnPlayerId == 1)
        {
            TurnCount++;
        }
    }

    private async UniTask<int> GetDiceResultAsync()
    {
        _stateController.BlackoutField();

        _diceBoxComponent.style.display = DisplayStyle.Flex;
        _diceBoxComponent.StartRolling();
        _stopButton.style.display = DisplayStyle.Flex;
        if ((GameMode == GameMode.Online && CurrentTurnPlayerEntity != _userController.MyEntity) || CurrentTurnPlayerEntity.IsNpc)
        {
            _stopButton.style.display = DisplayStyle.None;
        }

        if (CurrentTurnPlayerEntity.IsNpc)
        {
            NpcActionController.StopRolling(_diceBoxComponent);
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
        // 復活時のHPとMPを設定
        var defaultParameter = await Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).Task;
        var parameter = defaultParameter.ParameterList.FirstOrDefault(p => p.EntityType == EntityType.Player);
        entity.SetHitPoint(parameter.HitPoint);
        entity.SetManaPoint(Mathf.Max(entity.Parameter.ManaPoint, parameter.ManaPoint));
        _coffinObjects[entity.Id].SetActive(false);
    }

    public void SetPlayerAsDead(Entity playerEntity, Vector2 coffinPosition)
    {
        playerEntity.IsAlive = false;
        playerEntity.ChangeVisibility(false);
        _coffinObjects[playerEntity.Id].SetActive(true);
        _coffinObjects[playerEntity.Id].transform.position = coffinPosition;
        playerEntity.transform.position = coffinPosition;
    }

    public void ResetGame()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
}
