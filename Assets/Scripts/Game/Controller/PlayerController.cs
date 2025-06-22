using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Fusion;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private NetworkManager _networkManager;
    [SerializeField]
    private Transform _playerParent;

    [Networked, Capacity(4), OnChangedRender(nameof(OnPlayerListChanged))]
    public NetworkArray<NetworkObject> PlayerNetworkObjectList { get; }

    public List<Entity> SyncedPlayerList { get; } = new List<Entity>();

    public Observable<List<Entity>> OnPlayersInitialized => _onPlayersInitialized;

    public Subject<List<Entity>> _onPlayersInitialized = new();

    private MainController _mainController;

    public void Initialize(MainController mainController)
    {
        _mainController = mainController;
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

            clonedParameter.BattleSprite = await Addressables.LoadAssetAsync<Sprite>(Constants.GetAssetReferencePlayerBattleImage(playerId)).ToUniTask();
            clonedParameter.FieldSprite = await Addressables.LoadAssetAsync<Sprite>(Constants.GetAssetReferencePlayerFieldImage(playerId)).ToUniTask();
            clonedParameter.Name = isNpc ? $"{Constants.GetNpcNames(Settings.Language)[i]}" : $"{Constants.GetPlayerName(Settings.Language, playerId)}";

            // プレイヤーをスポーンさせる
            var playerGameObject = _networkManager.SpawnPlayer(playerPrefab, new Vector3(Constants.PlayerSpownPositions[i].x, Constants.PlayerSpownPositions[i].y, playerPrefab.transform.position.z), _playerParent);

            // スポーンしたオブジェクトのEntityコンポーネントを初期化
            var playerEntity = playerGameObject.GetComponent<Entity>();
            playerEntity.Initialize(clonedParameter, isNpc);
            playerEntity.SetName(Constants.GetPlayerName(Settings.Language, playerId));

            PlayerNetworkObjectList.Set(i, playerGameObject.GetComponent<NetworkObject>());
        }

        // OnChangedコールバックが全クライアントで呼ばれ、SyncedPlayerListが更新されるので、
        // ここで手動でOnNextを呼ぶ必要はなくなります。
        // _onPlayersInitialized.OnNext(SyncedPlayerList);
    }
}
