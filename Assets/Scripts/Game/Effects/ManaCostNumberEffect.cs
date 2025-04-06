using UnityEngine;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class ManaCostNumberEffect : NumberEffect
{
    public override async UniTask ShowEffect(int manaCost)
    {
        gameObject.SetActive(true);
        effectText.text = manaCost.ToString();
        effectText.color = Color.white;

        // ジャンプと落下のシーケンス
        Sequence jumpSequence = DOTween.Sequence();

        // 下方向に落下
        jumpSequence.Append(transform.DOMoveY(transform.position.y - moveDownDistance, moveDownDuration).SetEase(Ease.OutQuad));

        // フェードアウト
        jumpSequence.Join(effectText.DOFade(0f, fadeOutDuration));

        // オブジェクトプールに戻す
        jumpSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
            effectText.alpha = 1f;
        });

        // スケールアニメーション（ポップアップ）独立して実行
        transform.DOScale(Vector3.one * 1, scaleDuration).SetEase(Ease.OutBack);

        // 押し出しアニメーション（独立して実行）
        float horizontalMovementDirection = Random.Range(0, 2) == 0 ? 1 : -1;
        transform.DOMoveX(transform.position.x + (horizontalMovementDistance * horizontalMovementDirection), horizontalMovementDuration)
            .SetEase(Ease.InQuad);

        await jumpSequence.Play().ToUniTask();
    }
}