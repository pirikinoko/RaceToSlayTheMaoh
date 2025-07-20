using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using BossSlayingTourney.Core;
using BossSlayingTourney.Game.Effects;

namespace BossSlayingTourney.Game.Battle
{
    public class BattleAnimator : MonoBehaviour
    {
        #region Fields
        private VisualElement _entityImageLeft;
        private VisualElement _entityImageRight;
        private Entity _leftEntity;
        #endregion

        #region Public Methods
        public BattleAnimator(VisualElement entityImageLeft, VisualElement entityImageRight, Entity leftEntity)
        {
            _entityImageLeft = entityImageLeft;
            _entityImageRight = entityImageRight;
            _leftEntity = leftEntity;
        }

        public async UniTask PlayImageAnimationAsync(string key, VisualElement targetElement, ImageAnimationHolder imageAnimationHolder)
        {
            var imageAnimation = ImageAnimationPool.Instance.GetFromPool<ImageAnimation>("ImageAnimation");
            imageAnimation.SetSprites(imageAnimationHolder.GetSpriteps(key));

            var ratio = Screen.width / Constants.BaseScreenSize.x;

            var rect = targetElement.worldBound;
            var screenPos = new Vector3(rect.center.x * ratio, Screen.height - rect.center.y * ratio, 0);
            imageAnimation.transform.position = screenPos;

            await imageAnimation.PlayAnimationAsync();
            ImageAnimationPool.Instance.ReturnToPool("ImageAnimation", imageAnimation.gameObject);
        }

        public async UniTask AnimateEntityStepAsync(Entity targetEntity)
        {
            VisualElement targetImage = targetEntity == _leftEntity ? _entityImageLeft : _entityImageRight;
            bool isLeftEntity = targetEntity == _leftEntity;

            const float stepDistance = 30f;
            const float animationDuration = 0.2f;

            // 共通のステップアニメーション処理
            await ExecuteStepAnimationAsync(targetImage, stepDistance, animationDuration, isLeftEntity);
        }

        /// <summary>
        /// HPやMPの変化をアニメーションで表示するメソッド
        /// 一度ラベルが透明になり、値変更後に透明度を戻す
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async UniTask ChangeNumberWithAnimationAsync(Label label, int value)
        {
            var ChangeDuration = 0.3f;

            await DOTween.To(
                () => label.style.color.value,
                color => label.style.color = color,
                new Color(1, 1, 1, 0),
                ChangeDuration
            );

            label.text = value.ToString();

            await DOTween.To(
                () => label.style.color.value,
                color => label.style.color = color,
                new Color(1, 1, 1, 1),
                ChangeDuration
            );
        }
        #endregion

        #region Private Methods

        private async UniTask ExecuteStepAnimationAsync(VisualElement targetImage, float stepDistance, float duration, bool isLeftEntity)
        {
            // 左右によってマージンプロパティを選択
            var (getMarginValue, setMarginValue) = GetMarginAccessors(targetImage, isLeftEntity);

            var originalMargin = getMarginValue();

            // 前進アニメーション
            await AnimateMarginAsync(getMarginValue, setMarginValue, originalMargin + stepDistance, duration, Ease.OutQuad);

            // 後退アニメーション
            await AnimateMarginAsync(getMarginValue, setMarginValue, originalMargin, duration, Ease.InQuad);
        }

        /// <summary>
        /// マージンアニメーションを実行する共通メソッド
        /// DoGetter<float>はfloatの値を返すAction
        /// DoSetter<float>はfloatの値を引数にとるAction
        /// </summary>
        private (DOGetter<float> getter, DOSetter<float> setter) GetMarginAccessors(VisualElement targetImage, bool isLeftEntity)
        {
            if (isLeftEntity)
            {
                return (
                    () => targetImage.style.marginLeft.value.value,
                    value => targetImage.style.marginLeft = new StyleLength(value)
                );
            }
            else
            {
                return (
                    () => targetImage.style.marginRight.value.value,
                    value => targetImage.style.marginRight = new StyleLength(value)
                );
            }
        }

        private async UniTask AnimateMarginAsync(DOGetter<float> getter, DOSetter<float> setter, float targetValue, float duration, Ease easeType)
        {
            await DOTween.To(
                getter,
                setter,
                targetValue,
                duration
            ).SetEase(easeType).AsyncWaitForCompletion();
        }
        #endregion
    }
}