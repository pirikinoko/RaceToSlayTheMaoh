using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using System.Linq;
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
        var playerPrefab = await Addressables.LoadAssetAsync<GameObject>(Constants.AssetReferencePlayer).Task;

        for (int i = 0; i < Constants.MaxPlayerCount; i++)
        {
            var clonedParameter = parameter.Clone();
            var isNpc = i >= _mainController.PlayerCount;

            clonedParameter.Name = isNpc ? $"{Constants.GetNpcNames(Settings.Language)[i]}" : $"{Constants.GetPlayerName(Settings.Language, (i + 1))}";
            InitializePlayer(playerPrefab, clonedParameter, Constants.PlayerSpownPositions[i], Constants.PlayerColors[i], isNpc);
        }
        _onPlayersInitialized.OnNext(PlayerList);
    }

    private void InitializePlayer(GameObject playerPrefab, Parameter clonedParameter, Vector3 spawnPosition, Color color, bool isNpc)
    {
        var playerGameObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity, _playerParent);
        var player = playerGameObject.GetComponent<Entity>();
        player.Initialize(clonedParameter, isNpc);
        player.SetColor(color);

        PlayerList.Add(player);
    }
}
