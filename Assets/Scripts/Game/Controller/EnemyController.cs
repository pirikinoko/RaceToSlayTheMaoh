using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BossSlayingTourney.Core;
using BossSlayingTourney.ScriptableObjects;

namespace BossSlayingTourney.Game.Controllers
{
    public class EnemyController : MonoBehaviour
    {
        public List<Entity> EnemyList = new();

        [SerializeField]
        private Transform _staticEnemiesParent;
        [SerializeField]
        private Transform _enemiesToRandomizeParent;
        private ParameterAsset _parameterAsset;

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
            target.Initialize(clonedParameter, -1,
            Constants.GetAssetReferenceEnemyFieldImage(entityType), Constants.GetAssetReferenceEnemyBattleImage(entityType), isNpc: true);
        }

        private EntityType GetRandomEntityIdentifier()
        {
            var values = Enum.GetValues(typeof(EntityType))
                .Cast<EntityType>()
                .Where(entityType => entityType != EntityType.Satan)
                .ToArray();
            int randomIndex = UnityEngine.Random.Range(1, values.Length);
            return (EntityType)values.GetValue(randomIndex);
        }
    }
}