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
        var playerGameObject = await Addressables.LoadAssetAsync<GameObject>(Constants.AssetReferencePlayer).Task;
        var player = playerGameObject.GetComponent<Entity>();

        player.Initialize(parameter);
        PlayerList.Add(player);

        player.gameObject.transform.position = Constants.PlayerSpownPosition;
        Instantiate(player, Constants.PlayerSpownPosition, Quaternion.identity, _playerParent);
        _onPlayersInitialized.OnNext(PlayerList);
    }
}
