using Cysharp.Threading.Tasks;
using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIToolkit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainController : NetworkBehaviour
{
    [SerializeField]
    private NetworkManager _networkManager;
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

    private Entity _currentTurnPlayerEntity;
    public Entity CurrentTurnPlayerEntity { get => _currentTurnPlayerEntity; private set => _currentTurnPlayerEntity = value; }

    private DiceBoxComponent _diceBoxComponent;

    private Button _stopButton => _diceBoxComponent.StopButton;

    private Dictionary<int, GameObject> _coffinObjects = new();

    public Entity WinnerEntity { get; set; } = null;

    [Networked]
    private bool _isGameInitialized { get; set; } = false;

    [Networked, OnChangedRender(nameof(OnTurnPlayerChanged))]
    public int CurrentTurnPlayerId { get; set; }

    [Networked, OnChangedRender(nameof(OnDiceResultChanged))]
    private int NetworkedDiceResult { get; set; }

    private void OnDiceResultChanged()
    {
        // 必要に応じてUI更新など
    }

    // ターンプレイヤー変更時の処理
    private void OnTurnPlayerChanged()
    {
        CurrentTurnPlayerEntity = _playerController.SyncedPlayerList.FirstOrDefault(p => p.Id == CurrentTurnPlayerId);
        StartNewTurnAsync().Forget();
    }


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
        if (_gameMode == GameMode.Online && _networkManager.GetNetworkRunner().IsSharedModeMasterClient)
        {
            await _playerController.InitializePlayersAsync();
            await _enemyController.InitializeAllEnemiesAsync();
            _isGameInitialized = true;
            return;
        }
        else if (_gameMode == GameMode.Online)
        {
            await UniTask.WaitUntil(() => _isGameInitialized == true);
            return;
        }

        await _playerController.InitializePlayersAsync();
        await _enemyController.InitializeAllEnemiesAsync();
    }

    public void NewTurnProcess()
    {
        if (HasStateAuthority)
        {
            if (TurnCount != 0)
            {
                CurrentTurnPlayerId++;
                if (CurrentTurnPlayerId > Constants.MaxPlayerCount)
                    CurrentTurnPlayerId = 1;
            }
            else
            {
                CurrentTurnPlayerId = 1;
            }
        }
    }

    public async UniTask StartNewTurnAsync()
    {
        _fieldController.UpdateStatusBoxesAsync().Forget();
        // 現在のターンのプレイヤーを最前面に表示する
        CurrentTurnPlayerEntity.gameObject.GetComponent<SpriteRenderer>().sortingOrder++;

        await _cameraController.MoveCameraAsync(CurrentTurnPlayerEntity.transform.position);

        if (CurrentTurnPlayerEntity.IsAlive == false)
        {
            if (!HasStateAuthority)
            {
                return;
            }
            await Rpc_RevivePlayerAsync(CurrentTurnPlayerEntity);
            SetStatusOnRevive(CurrentTurnPlayerEntity);
            NewTurnProcess();
            return;
        }

        if (HasStateAuthority)
        {
            NetworkedDiceResult = await GetDiceResultAsync();
        }

        await UniTask.WaitUntil(() => NetworkedDiceResult != 0);

        if (CurrentTurnPlayerEntity == _userController.MyEntity)
        {
            CurrentTurnPlayerEntity.GetComponent<ControllableEntity>().SetMoves(NetworkedDiceResult);
            CurrentTurnPlayerEntity.GetComponent<ControllableEntity>().EnableClickMovement();
        }

        if (CurrentTurnPlayerEntity.IsNpc && HasStateAuthority)
        {
            NpcActionController.Move(CurrentTurnPlayerEntity.GetComponent<ControllableEntity>(), UnityEngine.Random.Range(0, 3) == 0);
        }

        // レイヤーの順序を戻す
        CurrentTurnPlayerEntity.gameObject.GetComponent<SpriteRenderer>().sortingOrder--;

        if (HasStateAuthority)
        {
            NetworkedDiceResult = 0;
        }

        if (CurrentTurnPlayerId == 1)
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

        if (CurrentTurnPlayerEntity.IsNpc && HasStateAuthority)
        {
            NpcActionController.StopRolling(_diceBoxComponent, UnityEngine.Random.Range(1, Constants.MaxDiceValue + 1));
        }

        await UniTask.WaitUntil(() => _diceBoxComponent.IsRolling == false);

        _diceBoxComponent.style.display = DisplayStyle.None;

        _stateController.RevealField();
        return _diceBoxComponent.GetCurrentNumber();
    }

    // サイコロ結果を全員に通知
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_SetDiceResult(int result)
    {
        NetworkedDiceResult = result;
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private async UniTask Rpc_RevivePlayerAsync(Entity entity)
    {
        await UniTask.Delay(1000);
        entity.ChangeVisibility(true);
        _coffinObjects[entity.Id].SetActive(false);
    }

    private void SetStatusOnRevive(Entity entity)
    {
        // 復活時のHPとMPを設定
        var defaultParameter = Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).WaitForCompletion();
        var parameter = defaultParameter.ParameterList.FirstOrDefault(p => p.EntityType == EntityType.Player);
        entity.SetHitPoint(parameter.HitPoint);
        entity.SetManaPoint(Mathf.Max(entity.Mp, parameter.ManaPoint));
    }

    public void SetPlayerAsDead(Entity playerEntity, Vector2 coffinPosition)
    {
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
