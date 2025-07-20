using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using R3;
using BossSlayingTourney.Core;
using BossSlayingTourney.Game.Effects;
using BossSlayingTourney.Skills;

namespace BossSlayingTourney.Game.Battle
{
    public class BattleAnimationManager
    {
        #region Dependencies
        private readonly BattleAnimator _battleAnimator;
        private readonly BattleUIController _battleUIController;
        private readonly ImageAnimationHolder _imageAnimationHolder;
        #endregion

        #region State
        private Entity _leftEntity;
        private Entity _rightEntity;

        /// <summary>
        /// HPやMPの変化に対するリアクティブなアニメーションの実行中カウンター（スレッドセーフ）
        /// 0の場合はすべてのアニメーションが終了している状態
        /// </summary>
        private int _reactiveNumberAnimationCounter;
        #endregion

        #region Events
        public readonly Subject<Unit> OnAllAnimationsCompleted = new();
        #endregion

        public BattleAnimationManager(
            BattleAnimator battleAnimator,
            BattleUIController battleUIController,
            ImageAnimationHolder imageAnimationHolder)
        {
            _battleAnimator = battleAnimator;
            _battleUIController = battleUIController;
            _imageAnimationHolder = imageAnimationHolder;
        }

        public void Initialize(Entity leftEntity, Entity rightEntity)
        {
            _leftEntity = leftEntity;
            _rightEntity = rightEntity;
            _reactiveNumberAnimationCounter = 0;
        }

        public async UniTask PlayAttackAnimationAsync(Entity currentTurnEntity, Entity waitingTurnEntity)
        {
            // ステップアニメーションを実行
            await _battleAnimator.AnimateEntityStepAsync(currentTurnEntity);
            // 攻撃時のエフェクトを再生
            VisualElement targetImage = GetTargetImage(waitingTurnEntity);
            await _battleAnimator.PlayImageAnimationAsync(Constants.ImageAnimationKeySlash, targetImage, _imageAnimationHolder);
        }

        public async UniTask PlaySkillAnimationAsync(string skillName, Entity currentTurnEntity, Entity waitingTurnEntity)
        {
            // ステップアニメーションを実行
            await _battleAnimator.AnimateEntityStepAsync(currentTurnEntity);
            // スキル時のエフェクトを再生
            VisualElement targetImage = GetTargetImage(waitingTurnEntity);
            await _battleAnimator.PlayImageAnimationAsync(SkillList.GetSkillEffectKey(skillName), targetImage, _imageAnimationHolder);
        }

        public async UniTask PlaySkillEffectAnimationAsync(Skill.SkillResult result, Entity currentTurnEntity, Entity waitingTurnEntity)
        {
            var skillEffectType = SkillList.GetSkillEffectType(result.EffectKey);

            // スキルのエフェクトを再生
            if (skillEffectType is SkillList.SkillEffectType.Heal or SkillList.SkillEffectType.Buff)
            {
                VisualElement targetImage = GetTargetImage(currentTurnEntity);
                await _battleAnimator.PlayImageAnimationAsync(result.EffectKey, targetImage, _imageAnimationHolder);
            }
            else if (skillEffectType is SkillList.SkillEffectType.Damage)
            {
                VisualElement targetImage = GetTargetImage(waitingTurnEntity);
                await _battleAnimator.PlayImageAnimationAsync(result.EffectKey, targetImage, _imageAnimationHolder);
            }
        }

        public void PlayImageAnimationForEntity(string key, Entity targetEntity)
        {
            VisualElement targetImage = GetTargetImage(targetEntity);
            _battleAnimator.PlayImageAnimationAsync(key, targetImage, _imageAnimationHolder).Forget();
        }

        private VisualElement GetTargetImage(Entity targetEntity)
        {
            return targetEntity == _leftEntity ? _battleUIController.EntityImageLeft : _battleUIController.EntityImageRight;
        }

        public void InitializeReactivePropertiesOfStatusChange(Entity leftEntity, Entity rightEntity, CompositeDisposable disposable)
        {
            var delayOnHpGainMills = 700;
            var delayOnHpDecreaseMills = 0;
            var delayOnManaChangeMills = 0;

            leftEntity.HitPointRp.Subscribe(newHp =>
            {
                StatusChangeReaction(newHp, leftEntity.Hp, _battleUIController.HealthLabelLeft, delayOnHpGainMills, delayOnHpDecreaseMills).Forget();
            }).AddTo(disposable);

            leftEntity.ManaPointRp.Subscribe(newMp =>
            {
                StatusChangeReaction(newMp, leftEntity.Mp, _battleUIController.ManaLabelLeft, delayOnManaChangeMills, delayOnManaChangeMills).Forget();
            }).AddTo(disposable);

            rightEntity.HitPointRp.Subscribe(newHp =>
            {
                StatusChangeReaction(newHp, rightEntity.Hp, _battleUIController.HealthLabelRight, delayOnHpGainMills, delayOnHpDecreaseMills).Forget();
            }).AddTo(disposable);

            rightEntity.ManaPointRp.Subscribe(newMp =>
            {
                StatusChangeReaction(newMp, rightEntity.Mp, _battleUIController.ManaLabelRight, delayOnManaChangeMills, delayOnManaChangeMills).Forget();
            }).AddTo(disposable);
        }

        private async UniTask StatusChangeReaction(int newNumber, int oldNumber, Label targetLabel, int delayOnGainMills, int delayOnDecreaseMills)
        {
            // 競合状態を避けるため，スレッドセーフにカウンターを管理
            Interlocked.Increment(ref _reactiveNumberAnimationCounter);

            try
            {
                if (newNumber > oldNumber)
                {
                    await UniTask.Delay(delayOnGainMills);
                    await _battleAnimator.ChangeNumberWithAnimationAsync(targetLabel, newNumber);
                }
                else
                {
                    await UniTask.Delay(delayOnDecreaseMills);
                    await _battleAnimator.ChangeNumberWithAnimationAsync(targetLabel, newNumber);
                }
            }
            finally
            {
                Interlocked.Decrement(ref _reactiveNumberAnimationCounter);

                // すべてのアニメーションが完了した場合にイベントを発火
                if (AreAllReactiveAnimationsEnded())
                {
                    OnAllAnimationsCompleted.OnNext(Unit.Default);
                }
            }
        }

        /// <summary>
        /// 全てのリアクティブなアニメーションが終了しているかどうかを確認
        /// </summary>
        /// <returns>全てのアニメーションが終了している場合はtrue</returns>
        public bool AreAllReactiveAnimationsEnded()
        {
            return Interlocked.CompareExchange(ref _reactiveNumberAnimationCounter, 0, 0) == 0;
        }

        public async UniTask WaitForAllAnimationsToComplete()
        {
            await UniTask.DelayFrame(2);
            while (!AreAllReactiveAnimationsEnded())
            {
                await UniTask.Delay(200);
            }
        }

        public void Dispose()
        {
            OnAllAnimationsCompleted?.Dispose();
        }
    }
}
