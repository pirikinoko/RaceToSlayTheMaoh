using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerController : MonoBehaviour
{
    public List<Player> _playerList = new();

    async void Start()
    {
        await InitializePlayersAsync();
    }

    private async UniTask InitializePlayersAsync()
    {
        var parameterAsset = await Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).Task;
        var parameter = parameterAsset.ParameterList.FirstOrDefault(p => p.Id == EntityIdentifier.Player);
        var player = await Addressables.LoadAssetAsync<Player>(Constants.AssetReferencePlayer).Task;

        player.SetParameter(parameter);
        _playerList.Add(player);
        Instantiate(player, Constants.PlayerSpownPosition, Quaternion.identity);
    }
}
