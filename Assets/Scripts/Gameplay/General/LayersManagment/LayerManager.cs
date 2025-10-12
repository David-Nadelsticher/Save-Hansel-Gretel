// LayerManager.cs

using System;
using Core.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.General.LayersManagment
{
    public class LayerManager : MonoBehaviour
    {
        /*public static event Action<LayerMask> OnStaticLayerSortRequest;
        public static event Action<LayerMask> OnCollectableLayerSortRequest;
        
        [SerializeField] private LayerMask collectableLayerMask;
        [SerializeField] private LayerMask environmentLayerMask;*/
        public static int SortingMultiplier { get; private set; } = 5;

        public static void SetMultiplier(int value)
        {
            SortingMultiplier = value;
        }

        public static int GetOrderFromY(float y)
        {
            return Mathf.RoundToInt(-y * SortingMultiplier);
        }

        /*private void Start()
        {
            InvokeStaticLayerSortRequest();
        }

        private void OnEnable()
        {
            InvokeStaticLayerSortRequest
        }
        private void OnDisable()
        {
             //InvokeLayerSortRequest();
        }

        public void InvokeStaticLayerSortRequest()
        {
            EventManager.Instance.InvokeEvent(EventNames.OnLayerSortRequest,OnStaticLayerSortRequest);
        }*/
    }


}