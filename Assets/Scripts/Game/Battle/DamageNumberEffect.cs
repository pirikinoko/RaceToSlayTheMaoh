using UnityEngine;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class DamageNumberEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    private const float jumpHeight = 80f;  // 跳ねる高さ
    private const float jumpDuration = 0.3f;  // 跳ねる時間
    private const float pushDistance = 50f; // 横方向に押し出される距離
    private const float pushDuration = 1.5f; // 横方向に押し出す時間
    private const float fallDistance = 150f;  // 落下距離
    private const float fallDuration = 1f;  // 落下時間             
    private const float scaleDuration = 1f;  // スケールアニメーション時間
    private const float fadeOutDuration = jumpDuration + fallDuration;  // フェードアウト時間

    private void OnEnable()
    {
        damageText.alpha = 1f;
        transform.localScale = Vector3.zero;
    }

    public async UniTask ShowDamage(int damage, Color color)
    {
        gameObject.SetActive(true);
        damageText.text = damage.ToString();
        damageText.color = color;

        // ジャンプと落下のシーケンス
        Sequence jumpSequence = DOTween.Sequence();

        // 上方向に跳ねる
        jumpSequence.Append(transform.DOMoveY(transform.position.y + jumpHeight, jumpDuration).SetEase(Ease.OutBounce));

        // 下方向に落下
        jumpSequence.Append(transform.DOMoveY(transform.position.y - fallDistance, fallDuration).SetEase(Ease.OutQuad));

        // フェードアウト
        jumpSequence.Join(damageText.DOFade(0f, fadeOutDuration));

        // オブジェクトプールに戻す
        jumpSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
            damageText.alpha = 1f;
        });

        // スケールアニメーション（ポップアップ）独立して実行
        transform.DOScale(Vector3.one * 1, scaleDuration).SetEase(Ease.OutBack);

        // 押し出しアニメーション（独立して実行）
        float pushDirection = Random.Range(0, 2) == 0 ? 1 : -1;
        transform.DOMoveX(transform.position.x + (pushDistance * pushDirection), pushDuration)
            .SetEase(Ease.InQuad);

        await jumpSequence.Play().ToUniTask();
    }
}