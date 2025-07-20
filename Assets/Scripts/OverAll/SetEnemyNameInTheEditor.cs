using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;

namespace BossSlayingTourney.Core
{


[RequireComponent(typeof(Entity))]
public class SetEnemyPreferenceTheEditor : MonoBehaviour
{
    private void OnValidate()
    {
        SetEnemyName();
        SetEnemyImage();
    }

    private void SetEnemyName()
    {
        var entity = GetComponent<Entity>();

        var pos = transform.position;
        string posStr = $"({pos.x:F1},{pos.y:F1})";
        string typeStr = entity.EntityType.ToString();

        string newName = $"{typeStr} {posStr}";
        if (gameObject.name != newName)
        {
            Undo.RecordObject(gameObject, "Set Enemy Name");
            gameObject.name = newName;
            EditorUtility.SetDirty(gameObject);
        }
    }

    private void SetEnemyImage()
    {
        var entity = GetComponent<Entity>();
        var spriteRenderer = GetComponent<SpriteRenderer>();

        string path = $"Assets/VisualAssets/Enemy/{entity.EntityType}OnField.png";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            path = $"Assets/VisualAssets/Enemy/{entity.EntityType}.png";
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Debug.LogWarning($"{entity.EntityType}のフィールド用のスプライトが見つからなかったため戦闘用を使用します");
        }

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
#endif
}