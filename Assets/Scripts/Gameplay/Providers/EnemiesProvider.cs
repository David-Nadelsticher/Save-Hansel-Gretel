using System.Collections.Generic;
using BossProject.Core;
using Gameplay.Controls.Enemy;
using Gameplay.Providers.Pool;
using UnityEngine;

namespace Gameplay.Providers
{
    /// <summary>
    /// Responsible for managing active enemies, spawning them based on distance filters, and handling enemy pool.
    /// </summary>
    public class EnemiesProvider : BaseMono
    {
        public static EnemiesProvider Instance;
        #region Inspector Fields

        [Header("Dependencies")] [SerializeField]
        private Spawner spawner;

        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private Transform playerTransform;

        [Header("Debug")] [SerializeField] private bool debugMode = true;

        #endregion

        #region Private Fields

        private ResourceFetcher _spawnPointProvider;

        #endregion

        #region Unity Methods

        private void Start()
        {
            if (playerTransform == null)
            {
                if (debugMode) Debug.LogError("[EnemiesProvider] playerTransform is null.");
            }

            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                if (debugMode) Debug.LogError("[EnemiesProvider] Spawn points are not assigned or empty.");
            }

            _spawnPointProvider = new ResourceFetcher(spawnPoints, playerTransform);
        }

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
        #endregion

        #region Public Methods

        /// <summary>
        /// Spawns multiple enemies at random spawn points filtered by distance from the player.
        /// </summary>
        public List<Transform> SpawnRandomEnemies(int count, string enemyKey, float minDistance = 5f, float maxDistance = 100f,
            float spawnRangeRadius = 1f)
        {
            if (count <= 0)
            {
                if (debugMode) Debug.LogError("[EnemiesProvider] Count must be greater than zero.");
                return null;
            }

            List<Transform> spawnPointList =
                _spawnPointProvider.GetRandomResourcesFilteredByDistance(minDistance, maxDistance, count);
            if (spawnPointList == null)
            {
                if (debugMode) Debug.LogError("[EnemiesProvider] No valid spawn point found.");
                return null;
            }
            List<Transform> spawnedEnemies = new List<Transform>();
            for (int i = 0; i < count; i++)
            {
                if (i > spawnPointList.Count - 1)
                {
                    if (debugMode)
                        Debug.LogWarning(
                            $"[EnemiesProvider] Not enough spawn points available. Requested {count}, but only {spawnPointList.Count} available.");
                    break;
                }

                BaseEnemy enemy = EnemyPool.Instance.GetFromPool(enemyKey);
                if (enemy == null)
                {
                    if (debugMode) Debug.LogError($"[EnemiesProvider] Failed to spawn enemy with key: {enemyKey}");
                    continue;
                }
                spawnedEnemies.Add(enemy.transform);
                spawner.SpawnObjectInRange(enemy.transform.gameObject, spawnPointList[i], spawnRangeRadius, false);
                /*AddEnemy(enemy.transform);*/
            }
            if (debugMode)
            {
                Debug.Log($"[EnemiesProvider] Spawned {spawnedEnemies.Count} enemies with key: {enemyKey}");
            }
            return spawnedEnemies;
        }

        public Transform SpawnRandomEnemy(string enemyKey, float minDistance = 5f, float maxDistance = 100f,
            float spawnRangeRadius = 1f)
        {
            List<Transform> spawnRandomEnemy  = SpawnRandomEnemies(1, enemyKey, minDistance, maxDistance, spawnRangeRadius);
            if (spawnRandomEnemy == null|| spawnRandomEnemy.Count == 0)
            {
                if (debugMode) Debug.LogError("[EnemiesProvider] No valid spawn point found for single enemy.");
                return null;
            }
            return spawnRandomEnemy[0];
        }
        /// <summary>
        /// Removes an enemy from the active enemies list and returns it to pool.
        /// </summary>
        public void RemoveEnemy(BaseEnemy enemy, string enemyKey)
        {
            if (enemy == null)
            {
                if (debugMode) Debug.LogError("[EnemiesProvider] Enemy is null.");
                return;
            }

            EnemyPool.Instance.ReturnToPool(enemyKey, enemy);
        }
        #endregion
        public int GetActiveEnemiesCount()
        {
            return EnemyPool.Instance.GetAllActiveObjectsCount();
            
        }
    }
}