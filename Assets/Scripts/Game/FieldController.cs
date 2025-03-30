using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UIToolkit;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class FieldController : MonoBehaviour
{
    [SerializeField]
    private StateController _stateController;
    [SerializeField]
    private EnemyController _enemyController;
    [SerializeField]
    private PlayerController _playerController;
    [SerializeField]
    private BattleController _battleController;

    private List<StatusBoxComponent> _statusBoxComponents = new();

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

        // エンカウントをチェックし続ける
        while (true)
        {
            await UniTask.Delay(1000);

            if (_stateController.CurrentState != State.Field)
            {
                await UniTask.Delay(3000);
                continue;
            }
            CheckEncount();
        }
    }

    private void CheckEncount()
    {
        var allEntity = new List<Entity>();
        allEntity.AddRange(_playerController.PlayerList);
        allEntity.AddRange(_enemyController._enemyList);

        foreach (var entityLeft in allEntity)
        {
            foreach (var entityRight in allEntity)
            {
                if (entityLeft == entityRight || (entityLeft.EntityType != EntityType.Player && entityRight.EntityType != EntityType.Player))
                {
                    continue;
                }

                if (Vector2.Distance(entityLeft.transform.position, entityRight.transform.position) < 0.1f)
                {
                    _stateController.ChangeState(State.Battle);

                    // 敵とプレイヤーが衝突した場合は敵をプレイヤーに変換
                    if (entityLeft.EntityType != EntityType.Player && entityRight.EntityType == EntityType.Player)
                    {
                        _battleController.StartBattle(entityRight, entityLeft);
                    }
                    _battleController.StartBattle(entityLeft, entityRight);
                    return;
                }
            }
        }
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
        }
    }
}
