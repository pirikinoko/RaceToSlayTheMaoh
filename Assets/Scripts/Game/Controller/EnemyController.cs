using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnemyController : MonoBehaviour
{
    public List<Entity> EnemyList = new();

    [SerializeField]
    private Transform _staticEnemiesParent;

    [SerializeField]
    private Transform _enemiesToRandomizeParent;

    private ParameterAsset _parameterAsset;

    /// <summary>
    /// ���ׂĂ̓G�����������܂��B
    /// </summary>
    /// <returns></returns>
    public async UniTask InitializeAllEnemiesAsync()
    {
        _parameterAsset = await Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).Task;

        var allEnemiesToRandomize = _enemiesToRandomizeParent.GetComponentsInChildren<Entity>();
        foreach (var enemy in allEnemiesToRandomize)
        {
            InitializeEnemies(enemy, GetRandomEntityIdentifier());
            EnemyList.Add(enemy);
        }

        var allStaticEnemies = _staticEnemiesParent.GetComponentsInChildren<Entity>();
        foreach (var enemy in allStaticEnemies)
        {
            InitializeEnemies(enemy, enemy.EntityType);
            EnemyList.Add(enemy);
        }
    }

    private void InitializeEnemies(Entity target, EntityType entityType)
    {
        var parameter = _parameterAsset.ParameterList.FirstOrDefault(p => p.EntityType == entityType);
        var clonedParameter = parameter.Clone();
        target.Initialize(clonedParameter);
    }

    private EntityType GetRandomEntityIdentifier()
    {
        Array values = Enum.GetValues(typeof(EntityType));
        int randomIndex = UnityEngine.Random.Range(1, values.Length);
        return (EntityType)values.GetValue(randomIndex);
    }
}
