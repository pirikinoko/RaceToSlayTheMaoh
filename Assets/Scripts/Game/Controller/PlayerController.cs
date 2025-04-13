using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Transform _playerParent;

    public List<Entity> PlayerList = new();

    public Observable<List<Entity>> OnPlayersInitialized => _onPlayersInitialized;

    public Subject<List<Entity>> _onPlayersInitialized = new();

    public async UniTask InitializePlayersAsync()
    {
        var parameterAsset = await Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).Task;
        var parameter = parameterAsset.ParameterList.FirstOrDefault(p => p.EntityType == EntityType.Player);
        var clonedParameter = parameter.Clone();
        var playerPrefab = await Addressables.LoadAssetAsync<GameObject>(Constants.AssetReferencePlayer).Task;

        var playerGameObject = Instantiate(playerPrefab, Constants.PlayerSpownPosition, Quaternion.identity, _playerParent);
        var player = playerGameObject.GetComponent<Entity>();
        player.Initialize(clonedParameter);

        PlayerList.Add(player);
        _onPlayersInitialized.OnNext(PlayerList);
    }
}
