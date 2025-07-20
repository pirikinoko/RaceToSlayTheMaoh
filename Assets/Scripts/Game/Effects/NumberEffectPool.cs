using System.Collections.Generic;
using UnityEngine;

namespace BossSlayingTourney.Game.Effects
{

public class NumberEffectPool : MonoBehaviour
{
    public static NumberEffectPool Instance;

    [System.Serializable]
    public class PoolConfig
    {
        public GameObject prefab;
        public int initialSize;
    }

    [SerializeField] private List<PoolConfig> poolConfigs;
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (var config in poolConfigs)
        {
            InitializePool(config);
        }
    }

    private void InitializePool(PoolConfig config)
    {
        var queue = new Queue<GameObject>();
        for (int i = 0; i < config.initialSize; i++)
        {
            var obj = Instantiate(config.prefab, transform);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }
        pools[config.prefab.name] = queue;
    }

    public T GetFromPool<T>(string prefabName) where T : NumberEffect
    {
        if (!pools.ContainsKey(prefabName)) return null;

        var pool = pools[prefabName];
        if (pool.Count == 0)
        {
            var config = poolConfigs.Find(c => c.prefab.name == prefabName);
            if (config != null)
            {
                var obj = Instantiate(config.prefab, transform);
                pool.Enqueue(obj);
            }
        }

        var effect = pool.Dequeue();
        effect.SetActive(true);
        return effect.GetComponent<T>();
    }

    public void ReturnToPool(string prefabName, GameObject obj)
    {
        if (!pools.ContainsKey(prefabName)) return;

        obj.SetActive(false);
        pools[prefabName].Enqueue(obj);
    }
}
}