using System.Collections.Generic;
using System.Linq;
using BossProject.Core;
using Gameplay.Controls.Collectable;
using Gameplay.Controls.Enemy;
using Gameplay.Providers.Pool;
using UnityEngine;

namespace Gameplay.Providers
{
    /// <summary>
    /// Responsible for managing active enemies, spawning them based on distance filters, and handling enemy pool.
    /// </summary>
    public class CollectableProvider : BaseMono
    {
        public static CollectableProvider Instance;


        #region Inspector Fields

        [Header("Dependencies")] [SerializeField]
        private Spawner spawner;

        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private Transform playerTransform;

        [Header("Debug")] [SerializeField] private bool debugMode = true;

        #endregion

        #region Private Fields

        // private List<Transform> _activeEnemies = new List<Transform>();
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
        public List<Transform> SpawnRandomCollectables(int count, string collectableKey, float minDistance = 5f,
            float maxDistance = 100f,
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

            List<Transform> spawnedCollectable = new List<Transform>();
            for (int i = 0; i < count; i++)
            {
                if (i > spawnPointList.Count - 1)
                {
                    if (debugMode)
                        Debug.LogWarning(
                            $"[EnemiesProvider] Not enough spawn points available. Requested {count}, but only {spawnPointList.Count} available.");
                    break;
                }

                CollectableObject collectable = CollectableObjectPool.Instance.GetFromPool(collectableKey);
                if (collectable == null)
                {
                    if (debugMode)
                        Debug.LogError($"[Collectable Provider] Failed to spawn collectableObject with key: {collectableKey}");
                    continue;
                }

                spawnedCollectable.Add(collectable.transform);
                spawner.SpawnObjectInRange(collectable.transform.gameObject, spawnPointList[i], spawnRangeRadius,
                    false);
                collectable.Reset();
                /*AddEnemy(enemy.transform);*/
            }

            if (debugMode)
            {
                Debug.Log(
                    $"[CollectableProvider] Spawned {spawnedCollectable.Count} enemies with key: {collectableKey}");
            }

            return spawnedCollectable;
        }

        public Transform SpawnRandomCollectable(string collectableKey, float minDistance = 5f, float maxDistance = 100f,
            float spawnRangeRadius = 1f)
        {
            List<Transform> spawnRandomCollectables =
                SpawnRandomCollectables(1, collectableKey, minDistance, maxDistance, spawnRangeRadius);
            if (spawnRandomCollectables == null || spawnRandomCollectables.Count == 0)
            {
                if (debugMode)
                    Debug.LogError("[CollectableProvider] No valid spawn point found for single Collectable.");
                return null;
            }

            return spawnRandomCollectables[0];
        }

        /// <summary>
        /// Adds a single enemy to the active enemies list by enemy key from pool.
        /// </summary>
        public void AddEnemy(string collectableKey)
        {
            CollectableObject collectable = CollectableObjectPool.Instance.GetFromPool(collectableKey);

            if (collectable == null)
            {
                if (debugMode)
                    Debug.LogError($"[CollectableProvider] Failed to spawn enemy with key: {collectableKey}");
                return;
            }

            /*AddEnemy(enemy.transform);*/
        }

        /// <summary>
        /// Removes an enemy from the active enemies list and returns it to pool.
        /// </summary>
        public void RemoveEnemy(CollectableObject collectableObject, string collectableObjectKey)
        {
            if (collectableObject == null)
            {
                if (debugMode) Debug.LogError("[CollectableProvider] collectable is null.");
                return;
            }

            CollectableObjectPool.Instance.ReturnToPool(collectableObjectKey, collectableObject);
            // var enemyTransform = enemy.transform;

            /*if (_activeEnemies.Count == 0)
            {
                return;
            }

            if (!_activeEnemies.Contains(enemyTransform))
            {
                if (debugMode) Debug.LogWarning("[EnemiesProvider] Enemy not found in active enemies list.");
                return;
            }

            _activeEnemies.Remove(enemyTransform);*/
        }

        /*
        /// <summary>
        /// Gets the nearest enemy to a given position.
        /// </summary>
        public Transform GetNearestEnemy(Vector3 position)
        {
            if (_activeEnemies == null || _activeEnemies.Count == 0)
            {
                return null;
            }

            // Clean null references (e.g., dead enemies)
            _activeEnemies = _activeEnemies
                .Where(enemy => enemy != null)
                .ToList();

            return _activeEnemies
                .OrderBy(enemy => Vector3.Distance(position, enemy.position))
                .FirstOrDefault();
        }
        */

        #endregion

        #region Private Methods

        /*
        /// <summary>
        /// Adds an enemy's transform to the active enemies list.
        /// </summary>
        private void AddEnemy(Transform enemy)
        {
            if (enemy == null)
            {
                if (debugMode) Debug.LogError("[EnemiesProvider] Enemy transform is null.");
                return;
            }

            if (!_activeEnemies.Contains(enemy))
                _activeEnemies.Add(enemy);
        }*/

        #endregion

        public int GetActiveCollectableCount()
        {
            //TODO Change by using pool Hashset count - this is just a temporary solution
            return CollectableObjectPool.Instance.GetAllActiveObjectsCount();

            /*return _activeEnemies?.Count ?? 0;*/
        }
    }
}