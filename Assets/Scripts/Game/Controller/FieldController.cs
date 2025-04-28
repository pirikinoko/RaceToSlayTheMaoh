using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UIToolkit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class FieldController : MonoBehaviour
{
    private MainController _mainController;
    private StateController _stateController;
    private EnemyController _enemyController;
    private PlayerController _playerController;
    private BattleController _battleController;

    private List<StatusBoxComponent> _statusBoxComponents = new();

    public void Initialize(MainController mainController, StateController stateController, EnemyController enemyController, PlayerController playerController, BattleController battleController)
    {
        _mainController = mainController;
        _stateController = stateController;
        _enemyController = enemyController;
        _playerController = playerController;
        _battleController = battleController;
    }

    private async UniTask Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var topStatusBoxComponents = root.Q<VisualElement>("TopElements").Children()
            .Select(child => child as StatusBoxComponent)
            .Where(component => component != null)
            .ToArray();
        var bottomStatusBoxComponents = root.Q<VisualElement>("BottomElements").Children()
            .Select(child => child as StatusBoxComponent)
            .Where(component => component != null)
            .ToArray();

        _statusBoxComponents.AddRange(topStatusBoxComponents);
        _statusBoxComponents.AddRange(bottomStatusBoxComponents);

        _statusBoxComponents.ForEach(statusBox => statusBox.style.display = DisplayStyle.None);
    }

    public bool CheckEncount(Entity entityLeft)
    {
        var allEntity = new List<Entity>();
        allEntity.AddRange(_playerController.PlayerList);
        allEntity.AddRange(_enemyController.EnemyList);

        foreach (var entityRight in allEntity)
        {
            // 自分同士 || 魔物同士はスルー
            if (entityLeft == entityRight || (entityLeft.EntityType != EntityType.Player && entityRight.EntityType != EntityType.Player))
            {
                continue;
            }

            if (Vector2.Distance(entityLeft.transform.position, entityRight.transform.position) < 0.1f)
            {
                _stateController.ChangeState(State.Battle);

                // 敵とプレイヤー衝突した場合はプレイヤーを左側に変換
                if (entityLeft.EntityType == EntityType.Player && entityRight.EntityType != EntityType.Player)
                {
                    _battleController.StartBattle(entityLeft, entityRight);
                }
                else
                {
                    _battleController.StartBattle(entityRight, entityLeft);
                }
                _battleController.StartBattle(entityLeft, entityRight);
                return true;
            }
        }
        return false;
    }

    public void DisplayStatusBoxes()
    {
        _statusBoxComponents.ForEach(statusBox => statusBox.style.display = DisplayStyle.None);
        for (int i = 0; i < _playerController.PlayerList.Count; i++)
        {
            _statusBoxComponents[i].style.display = DisplayStyle.Flex;
        }
    }

    public async UniTask UpdateStatusBoxesAsync()
    {
        var heartIcon = await Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceHeartIcon).Task;
        var manaIcon = await Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceManaIcon).Task;
        var powerIcon = await Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferencePowerIcon).Task;

        for (int i = 0; i < _playerController.PlayerList.Count; i++)
        {
            _statusBoxComponents[i].UpdateStatuBoxElments(_playerController.PlayerList[i], heartIcon, manaIcon, powerIcon);
            _statusBoxComponents[i].style.opacity = Constants.opacityForWaitingPlayersStatusBox;
        }
        _statusBoxComponents[_mainController.CurrentTurnPlayerId - 1].style.opacity = Constants.opacityForActivePlayerStatusBox;
    }
}
