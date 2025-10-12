using System;
using BossProject.Core;
using Core.Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace Gameplay.General.Utils
{
    class Checkpoint : BaseMono
    {
        [SerializeField] string phaseName;
        [SerializeField] private GameObject newBorder;
        [SerializeField] private bool finalCheckpoint;
        [SerializeField] private bool checkpointActive = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (checkpointActive && other.CompareTag("Player"))
            {
                if (newBorder != null)
                {
                    newBorder.SetActive(true);
                }

                EventManager.Instance.InvokeEvent(EventNames.OnCheckpointReached,
                    new CheckpointData
                    {
                        PhaseName = phaseName,
                        Position = newBorder.transform.position, LastCheckpoint = finalCheckpoint
                    });
                checkpointActive = false;
            }
        }

        public class CheckpointData
        {
            public string PhaseName { get; set; }
            public Vector2 Position { get; set; }
            public bool LastCheckpoint { get; set; }
        }
    }
}