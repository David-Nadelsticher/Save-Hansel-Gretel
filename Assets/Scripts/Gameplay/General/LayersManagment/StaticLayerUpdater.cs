// StaticLayerUpdater.cs

using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.General.LayersManagment
{
    [RequireComponent(typeof(SortingGroup))]
    public class StaticLayerUpdater : MonoBehaviour   
    {
        private SortingGroup sortingGroup;

        private void Awake()
        {
            sortingGroup = GetComponent<SortingGroup>();
            if (sortingGroup == null)
            {
                //print the name of the GameObject if the SortingGroup component is missing
                Debug.LogError("SortingGroup component is missing on the GameObject.");
                return;
            }
            // Initialize sorting order based on the current position
            UpdateSorting(null);
        }

        void OnEnable()
        {
            //EventManager.Instance.AddListener(EventNames.OnLayerSortRequest, UpdateSorting);
            UpdateSorting(null);

        
        }

        void OnDisable()
        {
            //LayerSortEvent.OnLayerSortRequest -= UpdateSorting;
        }

        void UpdateSorting(object obj)
        {
            if (sortingGroup == null)
            {
                Debug.LogError("SortingGroup component is missing on the GameObject.");
                return;
            }
            float y = transform.position.y;
            sortingGroup.sortingOrder = LayerManager.GetOrderFromY(y);
        }
    }
}