using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerController : MonoBehaviour
{
    private MainController _mainController;
    [SerializeField]
    private Transform _playerParent;

    public List<Entity> PlayerList = new();

    public Observable<List<Entity>> OnPlayersInitialized => _onPlayersInitialized;

    public Subject<List<Entity>> _onPlayersInitialized = new();

    public void Initialize(MainController mainController)
    {
        _mainController = mainController;
    }

    public async UniTask InitializePlayersAsync()
    {
        var parameterAsset = await Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).Task;
        var parameter = parameterAsset.ParameterList.FirstOrDefault(p => p.EntityType == EntityType.Player);

        for (int i = 0; i < Constants.MaxPlayerCountIncludingNpc; i++)
        {
            var playerId = i + 1;
            var playerPrefab = await Addressables.LoadAssetAsync<GameObject>(Constants.GetAssetReferencePlayer(playerId)).ToUniTask();
            var clonedParameter = parameter.Clone();
            var isNpc = i >= _mainController.PlayerCount;

            clonedParameter.IconSprite = await Addressables.LoadAssetAsync<Sprite>(Constants.GetAssetReferencePlayerIcon(playerId)).ToUniTask();
            clonedParameter.Name = isNpc ? $"{Constants.GetNpcNames(Settings.Language)[i]}" : $"{Constants.GetPlayerName(Settings.Language, playerId)}";
            InitializePlayer(playerPrefab, clonedParameter, Constants.PlayerSpownPositions[i], isNpc);
        }
        _onPlayersInitialized.OnNext(PlayerList);
    }

    private async Task InitializePlayer(GameObject playerPrefab, Parameter clonedParameter, Vector3 spawnPosition, bool isNpc)
    {
        var playerGameObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity, _playerParent);
        var player = playerGameObject.GetComponent<Entity>();

        player.Initialize(clonedParameter, isNpc);

        PlayerList.Add(player);
    }
}
