using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

namespace BossSlayingTourney.Game.Effects
{

public class ImageAnimation : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private float frameDuration = 0.083f; // デフォルトは12fps相当

    private Sprite[] sprites;
    private CancellationTokenSource cts;
    private bool isPlaying;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }

    private void OnDestroy()
    {
        Stop();
    }

    public async UniTask PlayAnimationAsync()
    {
        if (sprites == null || sprites.Length == 0) return;

        Stop();
        cts = new CancellationTokenSource();
        var token = cts.Token;

        isPlaying = true;

        foreach (var sprite in sprites)
        {
            targetImage.sprite = sprite;
            await UniTask.Delay(TimeSpan.FromSeconds(frameDuration), cancellationToken: token);
        }
        isPlaying = false;
    }

    public void SetSprites(Sprite[] newSprites)
    {
        sprites = newSprites;
        if (sprites != null && sprites.Length > 0)
        {
            targetImage.sprite = sprites[0];
        }
    }

    public void Stop()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Resume()
    {
        if (!isPlaying)
        {
            PlayAnimationAsync().Forget();
        }
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}
}