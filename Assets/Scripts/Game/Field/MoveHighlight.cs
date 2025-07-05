using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class MoveHighlight : MonoBehaviour
{
    private ControllableEntity _entity;

    // ハイライト位置までのパス
    private Queue<Vector2> _pathToHighlightPosition;
    private bool _isInitialized = false;

    Color _defaultColor = new Color(1, 1, 1, 0.3f);

    // 初期化メソッド
    public void Initialize(ControllableEntity entity, Queue<Vector2> pathToHighlightPosition, Color color)
    {
        _entity = entity;
        _pathToHighlightPosition = pathToHighlightPosition;
        // デフォルトのハイライト色を設定
        SetColor(color);
        _isInitialized = true;
    }

    private void SetColor(Color color)
    {
        color.a = 0.3f;
        _defaultColor = color;
        GetComponent<SpriteRenderer>().color = color;
    }

    private void OnMouseDown()
    {
        if (_isInitialized)
        {
            // クリックされたときに対象位置へ移動
            _entity.Rpc_TracePathAsync(_pathToHighlightPosition).Forget();
        }
    }

    // マウスホバー時のエフェクト
    private void OnMouseEnter()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // ハイライトを強調
            renderer.color = new Color(0, 1, 0, 0.5f); // より不透明に
        }
    }

    // マウスが離れた時のエフェクト
    private void OnMouseExit()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // 通常の状態に戻す
            renderer.color = _defaultColor;
        }
    }
}