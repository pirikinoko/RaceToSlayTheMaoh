using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using BossSlayingTourney.Core;
using BossSlayingTourney.Game.Controllers;

namespace BossSlayingTourney.Game.Battle
{
    public class BattleStatusManager
    {
        #region Dependencies
        private readonly BattleLogController _battleLogController;
        private readonly BattleUIController _battleUIController;
        private readonly BattleAnimationManager _animationManager;
        #endregion

        #region Events
        public readonly Subject<Condition> OnConditionProcessed = new();
        #endregion

        public BattleStatusManager(
            BattleLogController battleLogController,
            BattleUIController battleUIController,
            BattleAnimationManager animationManager)
        {
            _battleLogController = battleLogController;
            _battleUIController = battleUIController;
            _animationManager = animationManager;
        }

        /// <summary>
        /// 状態異常の際の処理を行う
        /// </summary>
        public Condition CheckAbnormalCondition(Entity currentTurnEntity, Entity waitingTurnEntity, Entity leftEntity, Entity rightEntity)
        {
            Condition condition = Condition.None;

            // 状態異常の画像をセット
            _battleUIController.SetConditionImage(leftEntity, rightEntity);

            var currentTurnEntitiesCondition = currentTurnEntity.AbnormalConditionType;
            condition = ProcessCurrentTurnEntityCondition(currentTurnEntity, currentTurnEntitiesCondition);

            var waitingTurnEntitiesCondition = waitingTurnEntity.AbnormalConditionType;
            if (condition == Condition.None)
            {
                condition = ProcessWaitingTurnEntityCondition(waitingTurnEntity, waitingTurnEntitiesCondition);
            }

            OnConditionProcessed.OnNext(condition);
            return condition;
        }

        private Condition ProcessCurrentTurnEntityCondition(Entity currentTurnEntity, Condition condition)
        {
            switch (condition)
            {
                case Condition.Poison:
                    return ProcessPoisonCondition(currentTurnEntity);

                case Condition.Fire:
                    return ProcessFireCondition(currentTurnEntity);

                case Condition.Regen:
                    return ProcessRegenCondition(currentTurnEntity);

                default:
                    return Condition.None;
            }
        }

        private Condition ProcessWaitingTurnEntityCondition(Entity waitingTurnEntity, Condition condition)
        {
            switch (condition)
            {
                case Condition.Stun:
                    _battleLogController.AddLog(Constants.GetStunSentence(Settings.Language, waitingTurnEntity.name));
                    return Condition.Stun;

                default:
                    return Condition.None;
            }
        }

        private Condition ProcessPoisonCondition(Entity entity)
        {
            int poisonDamage = (int)(entity.Hp * Constants.PoisonDamageRateOfHitPoint);
            entity.SetHitPoint(entity.Hp - poisonDamage);
            _battleLogController.AddLog(Constants.GetPoisonSentence(Settings.Language, entity.name));
            _animationManager.PlayImageAnimationForEntity(Constants.ImageAnimationKeyPoisonMushroom, entity);
            return Condition.Poison;
        }

        private Condition ProcessFireCondition(Entity entity)
        {
            int damage = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                Constants.FireDamage,
                Constants.FireDamageOffsetPercent,
                0
            );

            entity.SetHitPoint(entity.Hp - damage);
            _battleLogController.AddLog(Constants.GetFireDamageSentence(Settings.Language, entity.name));
            _animationManager.PlayImageAnimationForEntity(Constants.ImageAnimationKeyIgnition, entity);
            return Condition.Fire;
        }

        private Condition ProcessRegenCondition(Entity entity)
        {
            int regenAmount = Constants.GetRandomizedValueWithinOffsetWithMissPotential(
                Constants.RegenAmount,
                Constants.RegenAmountOffsetPercent,
                30
            );
            entity.SetHitPoint(entity.Hp + regenAmount);
            _battleLogController.AddLog(Constants.GetRegenSentence(Settings.Language, entity.name, regenAmount));
            if (regenAmount > 0)
            {
                _animationManager.PlayImageAnimationForEntity(Constants.ImageAnimationKeyRegen, entity);
            }
            return Condition.Regen;
        }

        public void ResetAbnormalCondition(Entity leftEntity, Entity rightEntity)
        {
            leftEntity.ResetAbnormalCondition();
            rightEntity.ResetAbnormalCondition();
        }

        public void Dispose()
        {
            OnConditionProcessed?.Dispose();
        }
    }
}
