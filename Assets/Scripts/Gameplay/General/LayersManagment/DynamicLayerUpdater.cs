
// DynamicLayerUpdater.cs

using Gameplay.General.LayersManagment;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[RequireComponent(typeof(SortingGroup))]
public class DynamicLayerUpdater : MonoBehaviour
{
    [FormerlySerializedAs("characterLowerPlace")] [SerializeField]
    private Transform characterLowestPlace;
    private SortingGroup sortingGroup;

    void Awake()
    {
        sortingGroup = GetComponent<SortingGroup>();
    }

    void Update()
    {
        float y =characterLowestPlace.transform.position.y;
        sortingGroup.sortingOrder = LayerManager.GetOrderFromY(y);
    }
    
    
    /// <summary>
    /// Returns the lowest Y value among all SpriteRenderers under the given transform.
    /// </summary>
    /// <param name="root">The root transform to search under.</param>
    /// <returns>The lowest Y position found.</returns>
    private float GetLowestSpriteY(Transform root)
    {
        SpriteRenderer[] sprites = root.GetComponentsInChildren<SpriteRenderer>();
        if (sprites.Length == 0) return root.position.y;

        float minY = float.MaxValue;
        foreach (var sr in sprites)
        {
            minY = Mathf.Min(minY, sr.bounds.min.y);
        }
        return minY;
    }
}

/*
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Controls.Player
{
    /// <summary>
    /// Dynamically updates the sorting order of the player and nearby objects based on their Y position,
    /// to ensure correct rendering order in a 2D environment.
    /// </summary>
    public class DynamicLayerUpdater : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Radius around the player to check for objects that need sorting order adjustment.")]
        public float checkRadius = 5f;
        [Tooltip("Layer mask to filter which objects are detected for sorting.")]
        public LayerMask layerMask;
        [Tooltip("Multiplier for fine-tuning sorting order calculation.")]
        public int sortingMultiplier = 100;

        private SortingGroup playerSortingGroup;
        //private SpriteRenderer playerSpriteRenderer;

        private void Start()
        {
            playerSortingGroup = GetComponent<SortingGroup>();
           // playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            UpdateNearbyObjectsSorting();
            UpdatePlayerSorting();
        }
        

        /// <summary>
        /// Updates the sorting order of all nearby objects within the detection radius.
        /// </summary>
        private void UpdateNearbyObjectsSorting()
        {
            Vector2 position = transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, checkRadius, layerMask);

            foreach (Collider2D hit in hits)
            {
                UpdateObjectSorting(hit);
            }
        }

        /// <summary>
        /// Updates the sorting order for a single object, preferring SortingGroup if available.
        /// </summary>
        /// <param name="collider">The collider of the object to update.</param>
        private void UpdateObjectSorting(Collider2D collider)
        {
            SortingGroup group = collider.GetComponent<SortingGroup>();
            if (group != null)
            {
                float groupY = GetLowestSpriteY(group.transform);
                group.sortingOrder = Mathf.RoundToInt(-groupY * sortingMultiplier);
                return;
            }

            SpriteRenderer sr = collider.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float objY = sr.bounds.min.y;
                sr.sortingOrder = Mathf.RoundToInt(-objY * sortingMultiplier);
            }
        }

        /// <summary>
        /// Updates the player's sorting order based on its Y position.
        /// </summary>
        private void UpdatePlayerSorting()
        {
            if (playerSortingGroup == null) return;
            float playerY = transform.position.y;
            playerSortingGroup.sortingOrder = Mathf.RoundToInt(-playerY * sortingMultiplier);
        }

        /// <summary>
        /// Returns the lowest Y value among all SpriteRenderers under the given transform.
        /// </summary>
        /// <param name="root">The root transform to search under.</param>
        /// <returns>The lowest Y position found.</returns>
        private float GetLowestSpriteY(Transform root)
        {
            SpriteRenderer[] sprites = root.GetComponentsInChildren<SpriteRenderer>();
            if (sprites.Length == 0) return root.position.y;

            float minY = float.MaxValue;
            foreach (var sr in sprites)
            {
                minY = Mathf.Min(minY, sr.bounds.min.y);
            }
            return minY;
        }

        /// <summary>
        /// Draws the detection radius in the Scene view for debugging.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position, checkRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, checkRadius);

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(transform.position + Vector3.up * (checkRadius + 0.5f), "Sorting Detection Radius");
#endif
        }
    }
}


/*using BossProject.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Controls.Player
{
    public class DynamicLayerUpdater : BaseMono
    {
        [Header("Detection Settings")]
        public float checkRadius = 5f; // Radius around the player to check objects
        public LayerMask layerMask;    // Layers to detect (e.g., Trees, NPCs)
        public int sortingMultiplier = 100; // Multiplier for fine-grained sorting

        private SortingGroup playerSortingGroup;

        void Start()
        {
            playerSortingGroup = GetComponent<SortingGroup>();
        }

        void Update()
        {
            Vector2 position = transform.position;

            // Detect all nearby objects in a circular area around the player
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, checkRadius, layerMask);

            foreach (Collider2D hit in hits)
            {
                SortingGroup sr = hit.GetComponent<SortingGroup>();
                if (sr == null) continue;

                float objY = hit.transform.position.y;

                // Set sorting order based on Y position
                sr.sortingOrder = Mathf.RoundToInt(-objY * sortingMultiplier);
            }

            // Set the player's sorting order based on their Y position
            if (playerSortingGroup != null)
            {
                playerSortingGroup.sortingOrder = Mathf.RoundToInt(-position.y * sortingMultiplier);
            }
        }

        // Draw a visualization of the detection radius in the Scene view
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Transparent green fill
            Gizmos.DrawSphere(transform.position, checkRadius);

            Gizmos.color = Color.green; // Bright green outline
            Gizmos.DrawWireSphere(transform.position, checkRadius);

            // Optional: label for clarity in Scene view
#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(transform.position + Vector3.up * (checkRadius + 0.5f), "Sorting Detection Radius");
#endif
        }
        // Helper: get the lowest Y from all SpriteRenderers under a group
        float GetLowestSpriteY(Transform root)
        {
            SpriteRenderer[] sprites = root.GetComponentsInChildren<SpriteRenderer>();
            if (sprites.Length == 0) return root.position.y;

            float minY = float.MaxValue;
            foreach (var sr in sprites)
            {
                minY = Mathf.Min(minY, sr.bounds.min.y);
            }
            return minY;
        }
    }
}#1#*/