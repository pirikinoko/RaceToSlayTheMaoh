using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnemyController : MonoBehaviour
{
    public List<Enemy> _enemyList = new();

    [SerializeField]
    private Transform _staticEnemiesParent;

    [SerializeField]
    private Transform _enemiesToRandomizeParent;

    async void Start()
    {
        var allEnemiesToRandomize = _enemiesToRandomizeParent.GetComponentsInChildren<Enemy>();
        foreach (var enemy in allEnemiesToRandomize)
        {
            await InitializeEnemiesAsync(enemy, GetRandomEntityIdentifier());
            _enemyList.Add(enemy);
        }

        var allStaticEnemies = _staticEnemiesParent.GetComponentsInChildren<Enemy>();
        foreach (var enemy in allStaticEnemies)
        {
            await InitializeEnemiesAsync(enemy, enemy.Identifier);
            _enemyList.Add(enemy);
        }
    }

    private async UniTask InitializeEnemiesAsync(Enemy target, EntityIdentifier identifier)
    {
        var parameterAsset = await Addressables.LoadAssetAsync<ParameterAsset>(Constants.AssetReferenceParameter).Task;
        var parameter = parameterAsset.ParameterList.FirstOrDefault(p => p.Id == identifier);

        target.SetParameter(parameter);
    }

    private EntityIdentifier GetRandomEntityIdentifier()
    {
        Array values = Enum.GetValues(typeof(EntityIdentifier));
        int randomIndex = UnityEngine.Random.Range(1, values.Length);
        return (EntityIdentifier)values.GetValue(randomIndex);
    }
}
