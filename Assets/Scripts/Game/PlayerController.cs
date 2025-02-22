using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerController : MonoBehaviour
{
    public List<Entity> _playerList = new();

    async void Start()
    {
        await InitializePlayersAsync();
    }

    private async UniTask InitializePlayersAsync()
    {
        var parameterAsset = await Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).Task;
        var parameter = parameterAsset.ParameterList.FirstOrDefault(p => p.Id == EntityType.Player);
        var player = await Addressables.LoadAssetAsync<Entity>(Constants.AssetReferencePlayer).Task;

        player.Initialize(parameter);
        _playerList.Add(player);
        Instantiate(player, Constants.PlayerSpownPosition, Quaternion.identity);
    }
}
