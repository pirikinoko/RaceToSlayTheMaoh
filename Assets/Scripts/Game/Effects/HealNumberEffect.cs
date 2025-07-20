using UnityEngine;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace BossSlayingTourney.Game.Effects
{

public class HealNumberEffect : NumberEffect
{
    private const float moveUpHeight = 120f;
    private const float moveUpDuration = 1.2f;

    protected override void OnEnable()
    {
        transform.localScale = Vector3.one;
    }

    public override async UniTask ShowEffect(int healAmount)
    {
        gameObject.SetActive(true);
        effectText.text = healAmount.ToString();
        effectText.color = Color.green;

        // ジャンプと落下のシーケンス
        Sequence jumpSequence = DOTween.Sequence();

        // 上方向に跳ねる
        jumpSequence.Append(transform.DOMoveY(transform.position.y + moveUpHeight, moveUpDuration).SetEase(Ease.Linear));

        // フェードアウト
        jumpSequence.Join(effectText.DOFade(0f, fadeOutDuration));

        // オブジェクトプールに戻す
        jumpSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
            effectText.alpha = 1f;
        });

        await jumpSequence.Play().ToUniTask();
    }
}
}