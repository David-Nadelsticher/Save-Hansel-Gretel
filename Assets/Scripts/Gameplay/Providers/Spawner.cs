using UnityEngine;

namespace Gameplay.Providers
{
    /// <summary>
    /// Spawner is responsible for spawning prefabs at specific points or within a range around a point.
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        [Header("Debug")]
        public bool debugMode = true;

        /// <summary>
        /// Spawns a prefab at the exact position of the given point.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="point">The transform whose position will be used for spawning.</param>
        /// <returns>The spawned GameObject instance, or null if blocked by a tree.</returns>
        public GameObject SpawnObject(GameObject prefab, Transform point, bool instantiate = true)
        {
            return SpawnObject(prefab, point.position, instantiate);
        }
        public GameObject SpawnObject(GameObject prefab, Vector3 pointPosition, bool instantiate = true)
        {
            if (prefab == null || pointPosition == null) return null;
            if (IsTreeAtPosition(pointPosition))
            {
                if (debugMode)
                    Debug.Log($"[Spawner] Spawn blocked by tree at {pointPosition}");
                return null;
            }

            if (instantiate)
            {
                return Instantiate(prefab, pointPosition, Quaternion.identity);

            }
            else
            { 
                prefab.transform.position = pointPosition;
                prefab.transform.rotation = Quaternion.identity;
                return prefab;
            }

        }
        
        
        /// <summary>
        /// Spawns a prefab at a random position within a radius around the center point.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="center">The center transform for the spawn range.</param>
        /// <param name="radius">The radius around the center to spawn within.</param>
        /// <returns>The spawned GameObject instance, or null if blocked by a tree.</returns>
        public GameObject SpawnObjectInRange(GameObject prefab, Transform center, float radius, bool instantiate = true)
        {
            if (prefab == null || center == null) return null;
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            Vector3 spawnPos = center.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
            return SpawnObject(prefab,spawnPos, instantiate);
        }

    

        /// <summary>
        /// Checks if there is a tree (object with tag "Tree" and collider) at the given position.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="checkRadius">The radius to check for trees (default 0.5f).</param>
        /// <returns>True if a tree is found, false otherwise.</returns>
        private bool IsTreeAtPosition(Vector2 position, float checkRadius = 0.5f)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, checkRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Tree"))
                    return true;
            }
            return false;
        }
    }
} 