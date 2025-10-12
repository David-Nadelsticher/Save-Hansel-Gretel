using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    [CreateAssetMenu(fileName = "PhaseConfig", menuName = "Scriptable Objects/Resource Phase Config")]
    public class PhaseConfigSo : ScriptableObject
    {
        public PhaseEntity[] phases;

        private Dictionary<string, PhaseConfig> _phasesDataBase = new Dictionary<string, PhaseConfig>();

        private void OnEnable()
        {
            _phasesDataBase.Clear();
            foreach (var phase in phases)
            {
                if (phase.entityName == null || phase.phaseConfig == null) continue;

                if (!_phasesDataBase.ContainsKey(phase.entityName))
                {
                    _phasesDataBase[phase.entityName] = phase.phaseConfig;
                }
                else
                {
                    Debug.LogWarning($"Duplicate resource name detected: {phase.entityName}");
                }
            }
        }


        public PhaseConfig GetPhaseData(string phaseName)
        {
            if (_phasesDataBase.TryGetValue(phaseName, out var phaseConfig))
            {
                return phaseConfig;
            }

            Debug.LogWarning($"Resource '{phaseName}' not found in config.");
            return null;
        }


        [Serializable]
        public class PhaseConfig
        {
            public float resourceSpawnInterval = 5f;
            public ResourceData[] availableResourceData;
            public int maxConcurrentResources = 3;
        }

        [Serializable]
        public class ResourceData
        {
            public GameObject resourcePrefab;
            public string resourceName;
            [Range(0f, 1f)] public float spawnChance;

            public ResourceData(GameObject prefab, string name, float chance)
            {
                resourcePrefab = prefab;
                resourceName = name;
                spawnChance = chance;
            }
        }

        [Serializable]
        public class PhaseEntity
        {
            public string entityName;
            public PhaseConfig phaseConfig;


            public PhaseEntity(string name, PhaseConfig phaseConfig)
            {
                entityName = name;
                this.phaseConfig = phaseConfig;
            }
        }
    }
}