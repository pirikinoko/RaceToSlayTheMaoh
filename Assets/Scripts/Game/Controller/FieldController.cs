using BossSlayingTourney.Core;
using Cysharp.Threading.Tasks;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIToolkit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using VContainer;

namespace BossSlayingTourney.Game.Controllers
{

    public class FieldController : MonoBehaviour
    {
        private MainController _mainController;
        private StateController _stateController;
        private EnemyController _enemyController;
        private PlayerController _playerController;
        private BattleController _battleController;

        private List<StatusBoxComponent> _statusBoxComponents = new();

        [Inject]
        public void Construct(MainController mainController, StateController stateController, EnemyController enemyController, PlayerController playerController, BattleController battleController)
        {
            _mainController = mainController;
            _stateController = stateController;
            _enemyController = enemyController;
            _playerController = playerController;
            _battleController = battleController;
        }

        private void Start()
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

        /// <summary>
        /// エンカウントチェックを行い、エンカウントが発生した場合はバトル状態に遷移する。
        /// 歩いたほうのエンティティがentityLeftになる
        /// </summary>
        /// <param name="entityLeft"></param>
        /// <param name="entityLeftsPreviousPos"></param>
        /// <returns></returns>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public bool Rpc_CheckEncount(Entity entityLeft, Vector2 entityLeftsPreviousPos)
        {
            var allEntity = new List<Entity>();
            allEntity.AddRange(_playerController.SyncedPlayerList);
            allEntity.AddRange(_enemyController.EnemyList);

            foreach (var entityRight in allEntity)
            {
                // 自分同士 || 魔物同士はスルー
                if (entityLeft == entityRight || (entityLeft.EntityType != EntityType.Player && entityRight.EntityType != EntityType.Player))
                {
                    continue;
                }

                if (!entityRight.IsAlive)
                {
                    continue;
                }

                if (Vector2.Distance(entityLeft.transform.position, entityRight.transform.position) < 0.1f)
                {
                    _stateController.ChangeState(Core.State.Battle);

                    _battleController.StartBattle(entityLeft, entityRight, entityLeftsPreviousPos);
                    return true;
                }
            }
            return false;
        }

        public void DisplayStatusBoxes()
        {
            _statusBoxComponents.ForEach(statusBox => statusBox.style.display = DisplayStyle.None);
            for (int i = 0; i < _playerController.SyncedPlayerList.Count; i++)
            {
                _statusBoxComponents[i].style.display = DisplayStyle.Flex;
            }
        }

        public async UniTask UpdateStatusBoxesAsync()
        {
            var heartIcon = await Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceHeartIcon).Task;
            var manaIcon = await Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferenceManaIcon).Task;
            var powerIcon = await Addressables.LoadAssetAsync<Sprite>(Constants.AssetReferencePowerIcon).Task;

            for (int i = 0; i < _playerController.SyncedPlayerList.Count; i++)
            {
                _statusBoxComponents[i].UpdateStatuBoxElments(_playerController.SyncedPlayerList[i], heartIcon, manaIcon, powerIcon);
                _statusBoxComponents[i].style.scale = Constants.ScaleForWaitingPlayersStatusBox;
                var playerIcon = await Addressables.LoadAssetAsync<Sprite>(Constants.GetAssetReferencePlayerBattleImage(i + 1)).Task;
                _statusBoxComponents[i].Q<VisualElement>("PlayerIcon").style.backgroundImage = new StyleBackground(playerIcon);
            }
            _statusBoxComponents[_mainController.CurrentTurnPlayerId - 1].style.scale = Constants.ScaleForActivePlayerStatusBox;
        }
    }
}