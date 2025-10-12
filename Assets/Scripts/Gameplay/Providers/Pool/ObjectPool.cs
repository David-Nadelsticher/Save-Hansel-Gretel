using System.Collections.Generic;
using BossProject.Core;
using UnityEngine;

namespace Gameplay.Providers.Pool
{
    /// <summary>
    /// Generic object pool implementation for managing reusable game objects.
    /// Provides efficient object creation, recycling, and lifecycle management.
    /// Implements the Singleton pattern for global access.
    /// </summary>
    /// <typeparam name="T">The type of component that implements IPoolable interface</typeparam>
    public class ObjectPool<T> : BaseMono where T : BaseMono, IPoolable
    {
        #region Singleton Implementation
        
        /// <summary>
        /// Singleton instance of the object pool
        /// </summary>
        private static ObjectPool<T> instance;
        
        /// <summary>
        /// Public accessor for the singleton instance
        /// </summary>
        public static ObjectPool<T> Instance => instance;
        
        #endregion

        #region Pool Configuration
        
        /// <summary>
        /// Represents a single pool configuration with its associated objects and containers
        /// </summary>
        [System.Serializable]
        private class Pool
        {
            [Header("Pool Configuration")]
            public string poolKey;                    // Unique identifier for the pool
            public GameObject prefab;                 // Prefab to instantiate for this pool
            public int initialSize = 10;              // Initial number of objects to create
            
            [Header("Pool Statistics")]
            public int ActiveCount { get; set; }      // Number of currently active objects
            public int TotalPooledCount { get; set; } // Total number of objects ever created for this pool
            
            [Header("Pool Containers")]
            public Transform container;               // Root container for this pool
            public Transform activeContainer;         // Container for active objects
            public Transform inactiveContainer;       // Container for inactive objects
            
            [Header("Object Collections")]
            public Queue<T> pooledObjects = new Queue<T>();           // Queue of available inactive objects
            public HashSet<T> activeObjects = new HashSet<T>();       // Set of currently active objects
            public HashSet<T> inactiveObjects = new HashSet<T>();     // Set of currently inactive objects
            
            /// <summary>
            /// Returns a string representation of the pool's current state
            /// </summary>
            /// <returns>Formatted string with pool statistics</returns>
            public override string ToString()
            {
                return $"Pool Key: {poolKey}, Active Count: {ActiveCount}, Total Pooled Count: {TotalPooledCount}";
            }
        }

        #endregion

        #region Serialized Fields
        
        [Header("Pool Configuration")]
        [SerializeField] private List<Pool> pools;           // List of pool configurations
        [SerializeField] private Transform poolRoot;         // Root transform for all pools
        
        #endregion

        #region Private Fields
        
        /// <summary>
        /// Dictionary mapping pool keys to their corresponding Pool instances
        /// </summary>
        private Dictionary<string, Pool> poolDictionary = new Dictionary<string, Pool>();
        
        #endregion

        #region Unity Lifecycle Methods
        
        /// <summary>
        /// Initializes the singleton instance and sets up all configured pools
        /// </summary>
        private void Awake()
        {
            // Singleton pattern implementation
            if (instance == null)
            {
                instance = this;
                InitializePools();
            }
            else
            {
                // Destroy duplicate instances to maintain singleton
                Destroy(gameObject);
            }
        }
        
        #endregion

        #region Pool Initialization
        
        /// <summary>
        /// Initializes all configured pools by creating containers and pre-instantiating objects
        /// </summary>
        private void InitializePools()
        {
            foreach (var pool in pools)
            {
                // Create hierarchical structure for pool organization
                CreatePoolHierarchy(pool);
                
                // Pre-instantiate the initial number of objects
                PreInstantiatePoolObjects(pool);
                
                // Register pool in dictionary for quick access
                poolDictionary[pool.poolKey] = pool;
            }
        }

        /// <summary>
        /// Creates the hierarchical structure for organizing pool objects
        /// </summary>
        /// <param name="pool">The pool to create hierarchy for</param>
        private void CreatePoolHierarchy(Pool pool)
        {
            // Create main pool container
            pool.container = new GameObject($"Pool_{pool.poolKey}").transform;
            pool.container.SetParent(poolRoot);

            // Create active objects container
            pool.activeContainer = new GameObject("Active").transform;
            pool.activeContainer.SetParent(pool.container);

            // Create inactive objects container
            pool.inactiveContainer = new GameObject("Inactive").transform;
            pool.inactiveContainer.SetParent(pool.container);
        }

        /// <summary>
        /// Pre-instantiates the specified number of objects for the pool
        /// </summary>
        /// <param name="pool">The pool to create objects for</param>
        private void PreInstantiatePoolObjects(Pool pool)
        {
            for (int i = 0; i < pool.initialSize; i++)
            {
                CreateNewPoolObject(pool);
            }
        }

        /// <summary>
        /// Creates a new object instance and adds it to the pool's inactive collection
        /// </summary>
        /// <param name="pool">The pool to add the object to</param>
        private void CreateNewPoolObject(Pool pool)
        {
            // Instantiate the prefab as child of inactive container
            var obj = Instantiate(pool.prefab, pool.inactiveContainer);
            var pooledComponent = obj.GetComponent<T>();
            
            // Validate that the prefab has the required component
            if (pooledComponent == null)
            {
                Debug.LogError($"Prefab for pool {pool.poolKey} does not have component of type {typeof(T).Name}!");
                return;
            }

            // Set object as inactive and add to pool collections
            obj.SetActive(false);
            pool.pooledObjects.Enqueue(pooledComponent);
            pool.inactiveObjects.Add(pooledComponent);
        }
        
        #endregion

        #region Public Pool Operations
        
        /// <summary>
        /// Retrieves an object from the specified pool, creating a new one if necessary
        /// </summary>
        /// <param name="key">The pool key to get object from</param>
        /// <returns>The retrieved object, or null if pool not found</returns>
        public T GetFromPool(string key)
        {
            // Validate pool exists
            if (!poolDictionary.TryGetValue(key, out Pool pool))
            {
                Debug.LogError($"Pool with key {key} not found!");
                return null;
            }

            // Create new object if pool is empty
            if (pool.pooledObjects.Count == 0)
            {
                CreateNewPoolObject(pool);
            }
            
            // Update pool statistics
            pool.ActiveCount++;
            pool.TotalPooledCount++;
            
            // Retrieve and activate object
            var obj = pool.pooledObjects.Dequeue();
            obj.transform.SetParent(pool.activeContainer);
            obj.gameObject.SetActive(true);
            obj.Reset(); // Reset object state for reuse

            // Update collections
            pool.inactiveObjects.Remove(obj);
            pool.activeObjects.Add(obj);

            return obj;
        }

        /// <summary>
        /// Returns an object to the specified pool for reuse
        /// </summary>
        /// <param name="key">The pool key to return object to</param>
        /// <param name="obj">The object to return to the pool</param>
        public void ReturnToPool(string key, T obj)
        {
            // Validate pool exists
            if (!poolDictionary.TryGetValue(key, out Pool pool))
            {
                Debug.LogError($"Pool with key {key} not found!");
                return;
            }
            
            // Decrement active count and validate
            pool.ActiveCount--;
            if (pool.ActiveCount < 0)
            {
                Debug.LogWarning($"Active count for pool {key} is already 0. Cannot return object {obj.name}.");
                pool.ActiveCount = 0;
                return;
            }
            
            // Deactivate and reorganize object
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(pool.inactiveContainer);

            // Update collections
            pool.activeObjects.Remove(obj);
            pool.inactiveObjects.Add(obj);
            pool.pooledObjects.Enqueue(obj);
        }
        
        #endregion

        #region Pool Information and Statistics
        
        /// <summary>
        /// Gets a read-only collection of all active objects in the specified pool
        /// </summary>
        /// <param name="key">The pool key</param>
        /// <returns>Collection of active objects, or null if pool not found</returns>
        public IReadOnlyCollection<T> GetActiveObjects(string key)
        {
            return poolDictionary.TryGetValue(key, out Pool pool) ? pool.activeObjects : null;
        }

        /// <summary>
        /// Gets a read-only collection of all inactive objects in the specified pool
        /// </summary>
        /// <param name="key">The pool key</param>
        /// <returns>Collection of inactive objects, or null if pool not found</returns>
        public IReadOnlyCollection<T> GetInactiveObjects(string key)
        {
            return poolDictionary.TryGetValue(key, out Pool pool) ? pool.inactiveObjects : null;
        }

        /// <summary>
        /// Gets the number of active objects in the specified pool
        /// </summary>
        /// <param name="key">The pool key</param>
        /// <returns>Number of active objects, or 0 if pool not found</returns>
        public int GetActiveCount(string key)
        {
            if (poolDictionary.TryGetValue(key, out Pool pool))
            {
                return pool.ActiveCount;
            }
            Debug.LogWarning($"Pool with key {key} not found!");
            return 0;
        }

        /// <summary>
        /// Gets the total number of active objects across all pools
        /// </summary>
        /// <returns>Total count of active objects</returns>
        public int GetAllActiveObjectsCount()
        {
            int totalCount = 0;
            foreach (var pool in poolDictionary.Values)
            {
                totalCount += pool.ActiveCount;
            }
            return totalCount;
        }

        /// <summary>
        /// Gets the total number of objects ever created across all pools
        /// </summary>
        /// <returns>Total count of all pooled objects</returns>
        public int GetTotalObjectPooledCount()
        {
            int totalCount = 0;
            foreach (var pool in poolDictionary.Values)
            {
                totalCount += pool.TotalPooledCount;
            }
            return totalCount;
        }

        /// <summary>
        /// Gets the total number of objects created for a specific pool
        /// </summary>
        /// <param name="str">The pool key</param>
        /// <returns>Total count for the specified pool, or 0 if not found</returns>
        private int GetTotalCount(string str)
        {
            if (poolDictionary.TryGetValue(str, out Pool pool))
            {
                return pool.TotalPooledCount;
            }
            Debug.LogError($"Pool with key {str} not found!");
            return 0;
        }
        
        #endregion

        #region Pool Management
        
        /// <summary>
        /// Clears all objects from the specified pool and destroys them
        /// </summary>
        /// <param name="key">The pool key to clear</param>
        public void ClearPool(string key)
        {
            // Validate pool exists
            if (!poolDictionary.TryGetValue(key, out Pool pool))
            {
                Debug.LogError($"Pool with key {key} not found!");
                return;
            }

            // Destroy all pooled objects
            foreach (var obj in pool.pooledObjects)
            {
                Destroy(obj.gameObject);
            }
            
            // Clear all collections
            pool.pooledObjects.Clear();
            pool.activeObjects.Clear();
            pool.inactiveObjects.Clear();
            pool.ActiveCount = 0;
            pool.TotalPooledCount = 0;

            // Destroy all objects in containers
            ClearContainer(pool.activeContainer);
            ClearContainer(pool.inactiveContainer);
        }

        /// <summary>
        /// Destroys all child objects in the specified container
        /// </summary>
        /// <param name="container">The container to clear</param>
        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
        
        #endregion
    }
}
