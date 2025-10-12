using System;
using Core.Managers;
using UnityEngine;


namespace Gameplay.General
{
    public class FinishPoint : MonoBehaviour
    {
        [Header("Debug Settings")] [SerializeField]
        private bool showDebugLogs = false;

        [Header("Settings")] [SerializeField] private string endSceneName = "EndScene";
        [SerializeField] private GameObject playerBarrier;
        [SerializeField] private GameObject portalEffect;

        private void Start()
        {
            playerBarrier.SetActive(true);
            portalEffect.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (showDebugLogs)
                Debug.Log($"[FinishPoint] Collision detected with object: {other.name}");

            // Check if the colliding object is a player (using tag)
            if (other.CompareTag("Player"))
            {
                if (showDebugLogs)
                    Debug.Log("[FinishPoint] Player reached the finish point!");
                if (showDebugLogs)
                    Debug.Log($"[FinishPoint] Loading end scene: {endSceneName}");
                EventManager.Instance.InvokeEvent(EventNames.OnPlayerSaveThem, true);
            }

        }


        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNames.OnBossDefeated, HandleBossDefeated);
        } private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNames.OnBossDefeated, HandleBossDefeated);
        }
        private void HandleBossDefeated(object obj)
        {
            if (showDebugLogs)
                Debug.Log("[FinishPoint] Boss defeated, enabling border.");

            playerBarrier.SetActive(false);
            portalEffect.SetActive(true);

        }
    }
} 