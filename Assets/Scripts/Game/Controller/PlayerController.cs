using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Transform _playerParent;

    public List<Entity> PlayerList = new();

    public Observable<List<Entity>> OnPlayersInitialized => _onPlayersInitialized;

    public Subject<List<Entity>> _onPlayersInitialized = new();

    private MainController _mainController;

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

            clonedParameter.BattleSprite = await Addressables.LoadAssetAsync<Sprite>(Constants.GetAssetReferencePlayerBattleImage(playerId)).ToUniTask();
            clonedParameter.FieldSprite = await Addressables.LoadAssetAsync<Sprite>(Constants.GetAssetReferencePlayerFieldImage(playerId)).ToUniTask();
            clonedParameter.Name = isNpc ? $"{Constants.GetNpcNames(Settings.Language)[i]}" : $"{Constants.GetPlayerName(Settings.Language, playerId)}";
            await InitializePlayer(playerPrefab, clonedParameter, Constants.PlayerSpownPositions[i], isNpc);
            PlayerList[i].SetName(Constants.GetPlayerName(Settings.Language, playerId));
        }
        _onPlayersInitialized.OnNext(PlayerList);
    }

    private async Task InitializePlayer(GameObject playerPrefab, Parameter clonedParameter, Vector2 spawnPosition, bool isNpc)
    {
        var playerGameObject = Instantiate(playerPrefab, new Vector3(spawnPosition.x, spawnPosition.y, playerPrefab.transform.position.z), playerPrefab.transform.rotation, _playerParent);
        var player = playerGameObject.GetComponent<Entity>();

        player.Initialize(clonedParameter, isNpc);

        PlayerList.Add(player);
    }
}
