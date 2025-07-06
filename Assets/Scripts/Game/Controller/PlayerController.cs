using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Fusion;
using VContainer;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private Transform _playerParent;
    private NetworkManager _networkManager;

    [Networked, Capacity(4), OnChangedRender(nameof(OnPlayerListChanged))]
    public NetworkArray<NetworkObject> PlayerNetworkObjectList { get; }

    public List<Entity> SyncedPlayerList { get; } = new List<Entity>();

    public Observable<List<Entity>> OnPlayersInitialized => _onPlayersInitialized;

    public Subject<List<Entity>> _onPlayersInitialized = new();

    private MainController _mainController;

    [Inject]
    public void Construct(MainController mainController)
    {
        _mainController = mainController;
        _networkManager = NetworkManager.Instance;
    }

    // PlayerEntitiesが変更されたときに呼ばれるコールバック
    private void OnPlayerListChanged()
    {
        UpdateLocalPlayerList();
    }

    private void UpdateLocalPlayerList()
    {
        SyncedPlayerList.Clear();
        foreach (var netObj in PlayerNetworkObjectList)
        {
            if (netObj != null)
            {
                SyncedPlayerList.Add(netObj.GetComponent<Entity>());
            }
        }
    }

    public async UniTask InitializePlayersAsync()
    {
        var parameterAsset = await Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).Task;
        var playerParameter = parameterAsset.ParameterList.FirstOrDefault(p => p.EntityType == EntityType.Player);

        for (int i = 0; i < Constants.MaxPlayerCount; i++)
        {
            var playerId = i + 1;
            var playerPrefab = await Addressables.LoadAssetAsync<GameObject>(Constants.GetAssetReferencePlayer(playerId)).ToUniTask();
            var clonedParameter = playerParameter.Clone();
            var isNpc = i >= _mainController.PlayerCount;

            clonedParameter.Name = isNpc ? $"{Constants.GetNpcNames(Settings.Language)[i]}" : $"{Constants.GetPlayerName(Settings.Language, playerId)}";

            // プレイヤーをスポーンさせる
            var playerGameObject = _networkManager.SpawnPlayer(playerPrefab, new Vector3(Constants.PlayerSpownPositions[i].x, Constants.PlayerSpownPositions[i].y, playerPrefab.transform.position.z), _playerParent);

            // スポーンしたオブジェクトのEntityコンポーネントを初期化
            var playerEntity = playerGameObject.GetComponent<Entity>();
            playerEntity.Initialize(clonedParameter, playerId, Constants.GetAssetReferencePlayerFieldImage(playerId), Constants.GetAssetReferencePlayerBattleImage(playerId), isNpc);
            playerEntity.SetName(Constants.GetPlayerName(Settings.Language, playerId));

            PlayerNetworkObjectList.Set(i, playerGameObject.GetComponent<NetworkObject>());
        }
    }
}
