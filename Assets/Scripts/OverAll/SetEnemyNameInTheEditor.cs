using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Entity))]
public class SetEnemyPreferenceTheEditor : MonoBehaviour
{
#if UNITY_EDITOR
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

        // EntityTypeからパスを組み立てる例
        string path = $"Assets/VisualAssets/Enemy/{entity.EntityType}.png";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning($"Texture not found at path: {path}");
            return;
        }

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
#endif

