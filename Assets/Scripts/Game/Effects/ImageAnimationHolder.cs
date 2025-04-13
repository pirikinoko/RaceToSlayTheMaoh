using System.Collections.Generic;
using UnityEngine;

public class ImageAnimationHolder : MonoBehaviour
{
    [SerializeField] private ImageAnimationConfig[] animatorConfigs;

    [System.Serializable]
    public class ImageAnimationConfig
    {
        public string name;
        public Sprite[] sprites;
    }

    private Dictionary<string, Sprite[]> spritesMap;

    private void Awake()
    {
        spritesMap = new Dictionary<string, Sprite[]>();
        foreach (var config in animatorConfigs)
        {
            if (!spritesMap.ContainsKey(config.name))
            {
                spritesMap[config.name] = config.sprites;
            }
            else
            {
                Debug.LogError($"Sprites with name {config.name} already exists.");
            }
        }
    }

    private void OnDestroy()
    {
        spritesMap.Clear();
    }

    public Sprite[] GetSpriteps(string name)
    {
        if (spritesMap.TryGetValue(name, out var sprites))
        {
            return sprites;
        }
        else
        {
            Debug.LogError($"Sprites with name {name} not found.");
            return null;
        }
    }
}
