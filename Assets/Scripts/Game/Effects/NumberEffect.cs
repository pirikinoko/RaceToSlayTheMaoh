using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

namespace BossSlayingTourney.Game.Effects
{
    public abstract class NumberEffect : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI effectText;
        protected const float moveUpHeight = 80f;  // 跳ねる高さ
        protected const float moveUpDuration = 0.5f;  // 跳ねる時間
        protected const float horizontalMovementDistance = 50f; // 横方向に押し出される距離
        protected const float horizontalMovementDuration = 1.5f; // 横方向に押し出す時間
        protected const float moveDownDistance = 150f;  // 落下距離
        protected const float moveDownDuration = 1f;  // 落下時間             
        protected const float scaleDuration = 1.5f;  // スケールアニメーション時間
        protected const float fadeOutDuration = moveUpDuration + moveDownDuration;  // フェードアウト時間


        protected virtual void OnEnable()
        {
            effectText.alpha = 1f;
            transform.localScale = Vector3.zero;
        }

        public abstract UniTask ShowEffect(int value);
    }
}
