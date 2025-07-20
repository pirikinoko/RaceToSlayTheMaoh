using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using UnityEngine;
using BossSlayingTourney.Core;
using BossSlayingTourney.Game.Controllers;

namespace BossSlayingTourney.Game.Battle
{
    public class BattleNetworkHandler : NetworkBehaviour
    {
        #region Dependencies
        private readonly BattleLogController _battleLogController;
        private readonly BattleAnimationManager _animationManager;
        private readonly BattleStatusManager _statusManager;
        private readonly BattleUIController _battleUIController;
        private readonly StateController _stateController;
        private readonly MainController _mainController;
        private readonly EnemyController _enemyController;
        private readonly RewardSelecter _rewardSelecter;
        #endregion

        #region State
        private Entity _leftEntity;
        private Entity _rightEntity;
        private Entity _currentTurnEntity;
        private Entity _waitingTurnEntity;
        private Entity _winnerEntity;
        private Entity _loserEntity;
        private Vector2 _entityLeftsPreviousPos;
        #endregion

        #region Events
        public readonly Subject<Unit> OnAttackRequested = new();
        public readonly Subject<string> OnSkillRequested = new();
        public readonly Subject<Unit> OnBackToField = new();
        public readonly Subject<Unit> OnGoToResult = new();
        #endregion

        public BattleNetworkHandler(
            BattleLogController battleLogController,
            BattleAnimationManager animationManager,
            BattleStatusManager statusManager,
            BattleUIController battleUIController,
            StateController stateController,
            MainController mainController,
            EnemyController enemyController,
            RewardSelecter rewardSelecter)
        {
            _battleLogController = battleLogController;
            _animationManager = animationManager;
            _statusManager = statusManager;
            _battleUIController = battleUIController;
            _stateController = stateController;
            _mainController = mainController;
            _enemyController = enemyController;
            _rewardSelecter = rewardSelecter;
        }

        public void Initialize(Entity leftEntity, Entity rightEntity, Vector2 entityLeftsPreviousPos)
        {
            _leftEntity = leftEntity;
            _rightEntity = rightEntity;
            _entityLeftsPreviousPos = entityLeftsPreviousPos;
        }

        #region Attack RPCs
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_RequestAttack()
        {
            // 共有ホスト以外のクライアントでアニメーションを再生する
            Rpc_PlayAttackAnimation();
            OnAttackRequested.OnNext(Unit.Default);
        }

        /// <summary>
        /// ホスト以外のプレイヤーが攻撃のアニメーションを再生するためのRPC
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void Rpc_PlayAttackAnimation()
        {
            _animationManager.PlayAttackAnimationAsync(_currentTurnEntity, _waitingTurnEntity).Forget();
        }
        #endregion

        #region Skill RPCs
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_RequestSkill(string skillName)
        {
            // 共有ホスト以外のクライアントでアニメーションを再生する
            Rpc_PlaySkillAnimation(skillName);
            OnSkillRequested.OnNext(skillName);
        }

        /// <summary>
        /// ホスト以外のプレイヤーがスキルのアニメーションを再生するためのRPC
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void Rpc_PlaySkillAnimation(string skillName)
        {
            _animationManager.PlaySkillAnimationAsync(skillName, _currentTurnEntity, _waitingTurnEntity).Forget();
        }
        #endregion

        #region Entity State RPCs
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_SetCurrentTurnEntity(Entity entity)
        {
            _currentTurnEntity = entity;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_SetWaitingTurnEntity(Entity entity)
        {
            _waitingTurnEntity = entity;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_WinnerEntity(Entity entity)
        {
            _winnerEntity = entity;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_LoserEntity(Entity entity)
        {
            _loserEntity = entity;
        }
        #endregion

        #region UI State RPCs
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_SetConditionImage()
        {
            _battleUIController.SetConditionImage(_leftEntity, _rightEntity);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_HideBattleElement()
        {
            _battleUIController.HideBattleElement();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_OpenRewardView()
        {
            _rewardSelecter.ShowRewardView();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_CloseRewardView()
        {
            _rewardSelecter.HideRewardView();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_SetRewards()
        {
            _rewardSelecter.SetupRewards(_winnerEntity, _loserEntity);
        }
        #endregion

        #region Animation RPCs
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_PlayImageAnimation(string key, Entity targetEntity)
        {
            _animationManager.PlayImageAnimationForEntity(key, targetEntity);
        }
        #endregion

        #region Game State RPCs
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_BackToField()
        {
            _statusManager.ResetAbnormalCondition(_leftEntity, _rightEntity);
            HandleDiedEntity(_loserEntity);
            _stateController.ChangeState(State.Field);
            OnBackToField.OnNext(Unit.Default);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void Rpc_GoToResult()
        {
            HandleDiedEntity(_loserEntity);
            _mainController.WinnerEntity = _winnerEntity;
            _stateController.ChangeState(State.Result);
            OnGoToResult.OnNext(Unit.Default);
        }
        #endregion

        #region Helper Methods
        private void HandleDiedEntity(Entity entity)
        {
            switch (entity.EntityType)
            {
                case EntityType.Player:
                    // leftEntity(さいころを振って動いたエンティティ)が負けた場合エンカウント地点の1マス前の位置に戻す
                    if (_loserEntity == _leftEntity)
                    {
                        _mainController.SetPlayerAsDead(entity, _entityLeftsPreviousPos);
                        break;
                    }
                    _mainController.SetPlayerAsDead(entity, _loserEntity.transform.position);
                    break;
                default:
                    Destroy(entity.gameObject);
                    _enemyController.EnemyList.Remove(entity);
                    break;
            }
        }

        public void SetCurrentEntities(Entity currentTurnEntity, Entity waitingTurnEntity, Entity winnerEntity, Entity loserEntity)
        {
            _currentTurnEntity = currentTurnEntity;
            _waitingTurnEntity = waitingTurnEntity;
            _winnerEntity = winnerEntity;
            _loserEntity = loserEntity;
        }
        #endregion

        public void Dispose()
        {
            OnAttackRequested?.Dispose();
            OnSkillRequested?.Dispose();
            OnBackToField?.Dispose();
            OnGoToResult?.Dispose();
        }
    }
}
