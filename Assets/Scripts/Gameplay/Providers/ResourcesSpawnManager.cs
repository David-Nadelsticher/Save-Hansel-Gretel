using BossProject.Core;
using Core.Data;
using Core.Managers;
using Gameplay.General.Utils;
using UnityEngine;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace Gameplay.Providers
{
    /// <summary>
    /// Manages the spawning of enemies and collectables based on phase configurations.
    /// Handles phase transitions, spawn intervals, and resource pool activation.
    /// </summary>
    public class ResourcesSpawnManager : BaseMono
    {
        #region Collectable Phase State
        [SerializeField] private string currentCollectablePhaseName; // Name of the current collectable phase
        [SerializeField] private float minSpawnCollectablePlayerDistance = 3f; // Minimum distance from player for collectable spawn
        [SerializeField] private float maxSpawnCollectablePlayerDistance = 5f; // Maximum distance from player for collectable spawn
        [SerializeField] private PhaseConfigSo collectablePhaseConfigSo; // Collectable phase configuration
        private PhaseConfigSo.PhaseConfig _currentCollectablePhase; // Current collectable phase data
        private int _activeCollectableNumber; // Number of currently active collectables
        private float _nextCollectableSpawnTime; // Time for next collectable spawn

        #endregion

        #region Enemy Phase State
        [SerializeField] private string currentEnemyPhaseName; // Name of the current enemy phase
        
        [SerializeField] private float minSpawnEnemyPlayerDistance = 3f; // Minimum distance from player for enemy spawn
        [SerializeField] private float maxSpawnEnemyPlayerDistance = 5f; // Maximum distance from player for enemy spawn
        [SerializeField] private PhaseConfigSo enemyPhaseConfigSo; // Enemy phase configuration
        [SerializeField]  private KeepDistanceX spawnKeepDistanceX; // Reference to keep distance logic
        private float _nextEnemySpawnTime; // Time for next enemy spawn
        private int _activeEnemyNumber; // Number of currently active enemies
        private PhaseConfigSo.PhaseConfig _currentEnemyPhase; // Current enemy phase data
        #endregion

        #region Debug & Pool State
        [SerializeField] private Transform player; // Reference to player transform (for debug)
        [SerializeField] private bool debugMode = true; // Enable debug logs
        [SerializeField] private float spawnRangeRadius = 0.5f; // Radius for spawn range
        [SerializeField] private bool enemyPoolActive; // Is enemy pool active
        [SerializeField] private bool collectablePoolActive; // Is collectable pool active
        #endregion

        #region Unity Methods
        /// <summary>
        /// Unity Start method. Initializes enemy and collectable phase configurations.
        /// </summary>
        private void Start()
        {
            InitializeEnemyPhaseConfigurations();
            InitializeCollectablePhaseConfigurations();
        }

        /// <summary>
        /// Unity Update method. Handles enemy and collectable spawning each frame.
        /// </summary>
        private void Update()
        {
            HandleEnemySpawning();
            HandleCollectableSpawning();
        }
        #endregion

        #region Phase Initialization
        /// <summary>
        /// Initializes collectable phase configuration and starts the phase.
        /// </summary>
        private void InitializeCollectablePhaseConfigurations()
        {
            if (collectablePhaseConfigSo == null || collectablePhaseConfigSo.phases == null ||
                collectablePhaseConfigSo.phases.Length == 0)
            {
                Debug.LogError("[ResourcesSpawnManager] Collectable Phase configuration is not set or empty.");
                return;
            }

            if (string.IsNullOrEmpty(currentCollectablePhaseName))
            {
                Debug.LogError("[ResourcesSpawnManager] Current collectable phase name is not set.");
                return;
            }

            _currentCollectablePhase = collectablePhaseConfigSo.GetPhaseData(currentCollectablePhaseName);
            StartCollectablePhase(currentCollectablePhaseName);
        }

        /// <summary>
        /// Initializes enemy phase configuration and starts the phase.
        /// </summary>
        private void InitializeEnemyPhaseConfigurations()
        {
            if (enemyPhaseConfigSo == null || enemyPhaseConfigSo.phases == null || enemyPhaseConfigSo.phases.Length == 0)
            {
                Debug.LogError("[ResourcesSpawnManager] Enemy Phase configuration is not set or empty.");
                return;
            }

            if (string.IsNullOrEmpty(currentEnemyPhaseName))
            {
                Debug.LogError("[ResourcesSpawnManager] Current enemy phase name is not set.");
                return;
            }

            _currentEnemyPhase = enemyPhaseConfigSo.GetPhaseData(currentEnemyPhaseName);
            StartEnemyPhase(currentEnemyPhaseName);
        }
        #endregion

        #region Phase Management
        /// <summary>
        /// Handles phase transition event for enemies.
        /// </summary>
        /// <param name="phaseName">The new phase name as object (should be string).</param>
        private void HandlePhaseTransition(object phaseName)
        {
            string phaseStringName = (string)phaseName;
            if (string.IsNullOrEmpty(phaseStringName))
            {
                Debug.LogError("[ResourcesSpawnManager] Phase name is null or empty.");
                return;
            }

            if (currentEnemyPhaseName == phaseStringName)
            {
                Debug.LogWarning($"[ResourcesSpawnManager] Already in enemy phase: {phaseName}");
                return;
            }

            ActivateEnemyPhaseTransition(phaseStringName);
        }

        /// <summary>
        /// Activates transition to a new enemy phase.
        /// </summary>
        /// <param name="phaseName">The new phase name.</param>
        private void ActivateEnemyPhaseTransition(string phaseName)
        {
            StartEnemyPhase(phaseName);
            if (debugMode)
                Debug.Log($"[ResourcesSpawnManager] Transitioning to enemy phase: {phaseName}");
        }

        /// <summary>
        /// Starts a new enemy phase and updates current phase data.
        /// </summary>
        /// <param name="phaseName">The phase name to start.</param>
        private void StartEnemyPhase(string phaseName)
        {
            var phaseData = enemyPhaseConfigSo.GetPhaseData(phaseName);
            if (phaseData == null)
            {
                Debug.LogError($"[ResourcesSpawnManager] Enemy Phase '{phaseName}' not found.");
                return;
            }

            currentEnemyPhaseName = phaseName;
            _currentEnemyPhase = phaseData;
        }

        /// <summary>
        /// Starts a new collectable phase and updates current phase data.
        /// </summary>
        /// <param name="phaseName">The phase name to start.</param>
        private void StartCollectablePhase(string phaseName)
        {
            var phaseData = collectablePhaseConfigSo.GetPhaseData(phaseName);
            if (phaseData == null)
            {
                Debug.LogError($"[ResourcesSpawnManager] Collectable Phase '{phaseName}' not found.");
                return;
            }

            currentCollectablePhaseName = phaseName;
            _currentCollectablePhase = phaseData;
        }
        #endregion

        #region Enemy Spawning
        /// <summary>
        /// Handles the logic for spawning enemies based on phase and pool state.
        /// </summary>
        private void HandleEnemySpawning()
        {
            if (enemyPoolActive && Time.time >= _nextEnemySpawnTime)
            {
                UpdateEnemyCount(null);
                if (_activeEnemyNumber < _currentEnemyPhase.maxConcurrentResources)
                {
                    SpawnEnemy();
                    _nextEnemySpawnTime = Time.time + _currentEnemyPhase.resourceSpawnInterval;
                }
            }
        }

        /// <summary>
        /// Updates the count of currently active enemies.
        /// </summary>
        /// <param name="obj">Unused parameter (for event compatibility).</param>
        private void UpdateEnemyCount(object obj)
        {
            _activeEnemyNumber = EnemiesProvider.Instance.GetActiveEnemiesCount();
        }

        /// <summary>
        /// Spawns a random enemy based on phase configuration and spawn chances.
        /// </summary>
        private void SpawnEnemy()
        {
            if (_currentEnemyPhase.availableResourceData == null ||
                _currentEnemyPhase.availableResourceData.Length == 0) return;

            float random = Random.value;
            int prefabIndex = -1;

            // Determine which enemy prefab to spawn based on random chance
            for (int i = 0; i < _currentEnemyPhase.availableResourceData.Length; i++)
            {
                if (random < _currentEnemyPhase.availableResourceData[i].spawnChance)
                {
                    prefabIndex = i;
                }
            }

            if (prefabIndex != -1)
            {
                var enemyName = _currentEnemyPhase.availableResourceData[prefabIndex].resourceName;
                if (!string.IsNullOrEmpty(enemyName))
                {
                    Transform enemy = EnemiesProvider.Instance.SpawnRandomEnemy(enemyName, minSpawnEnemyPlayerDistance,
                        maxSpawnEnemyPlayerDistance, spawnRangeRadius);
                    if (enemy != null && debugMode)
                        Debug.Log($"[ResourcesSpawnManager] Spawned Enemy: {enemyName} at {enemy.position}");
                }
            }
        }
        #endregion

        #region Collectable Spawning
        /// <summary>
        /// Handles the logic for spawning collectables based on phase and pool state.
        /// </summary>
        private void HandleCollectableSpawning()
        {
            if (Time.time >= _nextCollectableSpawnTime)
            {
                UpdateCollectableCount(null);
                if (_activeCollectableNumber < _currentCollectablePhase.maxConcurrentResources)
                {
                    SpawnCollectable();
                    _nextCollectableSpawnTime = Time.time + _currentCollectablePhase.resourceSpawnInterval;
                }
            }
        }

        /// <summary>
        /// Updates the count of currently active collectables.
        /// </summary>
        /// <param name="obj">Unused parameter (for event compatibility).</param>
        private void UpdateCollectableCount(object obj)
        {
            _activeCollectableNumber = CollectableProvider.Instance.GetActiveCollectableCount();
        }

        /// <summary>
        /// Spawns a random collectable based on phase configuration and spawn chances.
        /// </summary>
        private void SpawnCollectable()
        {
            if (_currentCollectablePhase.availableResourceData == null ||
                _currentCollectablePhase.availableResourceData.Length == 0) return;

            float random = Random.value;
            int prefabIndex = -1;

            // Determine which collectable prefab to spawn based on random chance
            for (int i = 0; i < _currentCollectablePhase.availableResourceData.Length; i++)
            {
                if (random < _currentCollectablePhase.availableResourceData[i].spawnChance)
                {
                    prefabIndex = i;
                }
            }

            if (prefabIndex != -1)
            {
                var collectableName = _currentCollectablePhase.availableResourceData[prefabIndex].resourceName;
                if (!string.IsNullOrEmpty(collectableName))
                {
                    Transform collectable = CollectableProvider.Instance.SpawnRandomCollectable(collectableName,
                        minSpawnCollectablePlayerDistance, maxSpawnCollectablePlayerDistance, spawnRangeRadius);
                    if (collectable != null && debugMode)
                        Debug.Log(
                            $"[ResourcesSpawnManager] Spawned Collectable: {collectableName} at {collectable.position}");
                }
            }
        }
        #endregion

        #region Checkpoint & Event Handling
        /// <summary>
        /// Handles player reaching a checkpoint and manages phase transitions.
        /// </summary>
        /// <param name="obj">Checkpoint data object.</param>
        public void HandlePlayerReachedCheckpoint(Object obj)
        {
            Checkpoint.CheckpointData data = obj as Checkpoint.CheckpointData;
            if (data == null) return;

            // If phase name is provided, check if a transition is needed
            if (data.PhaseName != null)
            {
                // Start enemy phase if it is different from the current one
                if (data.PhaseName != currentCollectablePhaseName)
                {
                    ActivateEnemyPhaseTransition(data.PhaseName);
                    if (data.LastCheckpoint)
                    {
                        spawnKeepDistanceX.enabled = false;
                    }
                }
                else if (debugMode)
                    Debug.Log("[ResourcesSpawnManager] Player reached checkpoint but no enemy phase change needed.");
            }
            else if (debugMode)
                Debug.Log("[ResourcesSpawnManager] Player reached checkpoint but no phase change needed.");
        }
        #endregion

        #region Unity Event Registration
        /// <summary>
        /// Registers event listeners when enabled.
        /// </summary>
        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNames.OnBossPhaseChange, HandlePhaseTransition);
            EventManager.Instance.AddListener(EventNames.OnEnemyDie, UpdateEnemyCount);
            EventManager.Instance.AddListener(EventNames.OnPlayerCollect, UpdateCollectableCount);
            EventManager.Instance.AddListener(EventNames.OnCheckpointReached, HandlePlayerReachedCheckpoint);
            EventManager.Instance.AddListener(EventNames.OnBossDefeated, _=>StopPoolEnemies());
            EventManager.Instance.AddListener(EventNames.OnStartGame, _ => StartGame());
        }

        /// <summary>
        /// Unregisters event listeners when disabled.
        /// </summary>
        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNames.OnBossPhaseChange, HandlePhaseTransition);
            EventManager.Instance.RemoveListener(EventNames.OnEnemyDie, UpdateEnemyCount);
            EventManager.Instance.RemoveListener(EventNames.OnPlayerCollect, UpdateCollectableCount);
            EventManager.Instance.RemoveListener(EventNames.OnCheckpointReached,HandlePlayerReachedCheckpoint);
            EventManager.Instance.RemoveListener(EventNames.OnStartGame, _ => StartGame());
        }
        #endregion

        #region Pool Control
        /// <summary>
        /// Stops the enemy pool (used when boss is defeated).
        /// </summary>
        private void StopPoolEnemies()
        {
            enemyPoolActive = false;
        }

        /// <summary>
        /// Starts the enemy and collectable pools (used when game starts).
        /// </summary>
        private void StartGame()
        {
            enemyPoolActive = true;
            collectablePoolActive = true;
        }
        #endregion

        #region Editor Debug
#if UNITY_EDITOR
        /// <summary>
        /// Draws gizmos in the editor to visualize spawn ranges around the player.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (player == null) return;

            Vector3 playerPos = player.transform.position;

            // Enemy spawn range
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(playerPos, minSpawnEnemyPlayerDistance);
            Gizmos.DrawWireSphere(playerPos, maxSpawnEnemyPlayerDistance);

            // Collectable spawn range
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(playerPos, minSpawnCollectablePlayerDistance);
            Gizmos.DrawWireSphere(playerPos, maxSpawnCollectablePlayerDistance);
        }
#endif
        #endregion
    }
}