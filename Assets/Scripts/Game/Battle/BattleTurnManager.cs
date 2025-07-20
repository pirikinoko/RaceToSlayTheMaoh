using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using UnityEngine;
using BossSlayingTourney.Core;
using BossSlayingTourney.Game.Controllers;
using BossSlayingTourney.Network;

namespace BossSlayingTourney.Game.Battle
{
    public class BattleTurnManager
    {
        #region Dependencies
        private readonly BattleLogController _battleLogController;
        private readonly NetworkManager _networkManager;
        private readonly UserController _userController;
        private readonly BattleUIController _battleUIController;
        #endregion

        #region State
        private Entity _currentTurnEntity;
        private Entity _waitingTurnEntity;
        private Entity _leftEntity;
        private Entity _rightEntity;
        private bool _hasActionEnded;
        #endregion

        #region Events
        public readonly Subject<Entity> OnTurnChanged = new();
        public readonly Subject<Unit> OnActionEnded = new();
        #endregion

        public Entity CurrentTurnEntity => _currentTurnEntity;
        public Entity WaitingTurnEntity => _waitingTurnEntity;
        public bool HasActionEnded => _hasActionEnded;

        public BattleTurnManager(
            BattleLogController battleLogController,
            NetworkManager networkManager,
            UserController userController,
            BattleUIController battleUIController)
        {
            _battleLogController = battleLogController;
            _networkManager = networkManager;
            _userController = userController;
            _battleUIController = battleUIController;
        }

        public void Initialize(Entity leftEntity, Entity rightEntity)
        {
            _leftEntity = leftEntity;
            _rightEntity = rightEntity;
            _currentTurnEntity = leftEntity;
            _waitingTurnEntity = rightEntity;
            _hasActionEnded = false;
        }

        public void StartNewTurn(int turnCount)
        {
            if (!_networkManager.GetNetworkRunner().IsSharedModeMasterClient)
            {
                return;
            }

            _hasActionEnded = false;
            bool isFirstTurn = turnCount == 0;

            // _currentTurnEntityと_waitingTurnEntityを入れ替える
            if (!isFirstTurn)
            {
                Entity tmp = _currentTurnEntity;
                _currentTurnEntity = _waitingTurnEntity;
                _waitingTurnEntity = tmp;
            }

            // スタン状態の場合はターンをスキップ
            if (_currentTurnEntity.AbnormalConditionType == Condition.Stun)
            {
                _currentTurnEntity.SetAbnormalCondition(Condition.None, null);
                OnTurnChanged.OnNext(_currentTurnEntity);
                return;
            }

            OnTurnChanged.OnNext(_currentTurnEntity);
            HandleTurnStartActions();
        }

        private void HandleTurnStartActions()
        {
            // NPCのターンの場合は行動を実行する
            if (_currentTurnEntity.IsNpc)
            {
                NpcActionController.ActAsync(null, _currentTurnEntity, _waitingTurnEntity).Forget();
            }
            // プレイヤーのターンの場合は待機ログを出す
            else
            {
                _battleLogController.SetText(Constants.GetSentenceWhileWaitingAction(Settings.Language.ToString(), _currentTurnEntity.name));
            }
        }

        public void OnTurnEntityChanged()
        {
            // ターン表示の矢印を切り替える
            _battleUIController.SwitchArrowVisibility(_currentTurnEntity, _leftEntity);

            // 自分のターンが来たかどうかを判定し、コマンドUIを開く
            if (_currentTurnEntity == _userController.MyEntity)
            {
                _battleUIController.OpenCommandView();
                _battleUIController.SetSkillButtons(_currentTurnEntity);
                _battleUIController.ToggleSkillButtonClickable(_currentTurnEntity);
            }
        }

        public void SetCurrentTurnEntity(Entity entity)
        {
            _currentTurnEntity = entity;
        }

        public void SetWaitingTurnEntity(Entity entity)
        {
            _waitingTurnEntity = entity;
        }

        public void MarkActionEnded()
        {
            _hasActionEnded = true;
            OnActionEnded.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            OnTurnChanged?.Dispose();
            OnActionEnded?.Dispose();
        }
    }
}
