using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Providers
{
    public class ResourceFetcher
    {
        private Transform _targetTransform;
        private List<Transform> _resources;
        
        public void SetTargetTransform(Transform referenceTransform)
        {
            _targetTransform = referenceTransform;
        }

        public void SetResources(List<Transform> resources)
        {
            _resources = resources;
        }

        public ResourceFetcher(List<Transform> resources, Transform targetTransform = null)
        {
            _targetTransform = targetTransform;
            _resources = resources;
        }

        public Transform GetRandomResource(List<Transform> resources)
        {
            if (resources == null || resources.Count == 0)
            {
                Debug.LogWarning("No resources available.");
                return null;
            }

            return resources[Random.Range(0, resources.Count)];
        }

        public List<Transform> FilterResourcesByDistance(float minDistance = 5f, float maxDistance = 20f)
        {
            if (_targetTransform == null || _resources == null || _resources.Count == 0)
            {
                Debug.LogWarning("No reference transform or resources available.");
                return new List<Transform>();
            }

            List<Transform> validResources = new List<Transform>();

            foreach (var resource in _resources)
            {
                var distance = Vector3.Distance(_targetTransform.position, resource.position);
                if (distance >= minDistance && distance <= maxDistance)
                {
                    validResources.Add(resource);
                }
            }
            return validResources;
        }

        public List<Transform> GetRandomResourcesFilteredByDistance(float minDistance = 5f, float maxDistance = 20f, int numberOfResources = 1)
        {
            var filteredResources = FilterResourcesByDistance(minDistance, maxDistance);
            if (filteredResources.Count == 0)
            {
                Debug.LogWarning("No valid resources found within the specified distance.");
                return null;
            }
            if (numberOfResources <= 0 || numberOfResources > filteredResources.Count)
            {
                Debug.LogWarning("Invalid number of resources requested.");
                return null;
            }
            var selectedResources = new List<Transform>();
            for (int i = 0; i < numberOfResources; i++)
            {
                var randomResource = GetRandomResource(filteredResources);
                if (randomResource != null)
                {
                    filteredResources.Remove(randomResource);
                    selectedResources.Add(randomResource);
                }
            }
            return selectedResources;
        }

        public Transform GetClosestResource(List<Transform> resources)
        {
            if (_targetTransform == null || resources == null || resources.Count == 0)
            {
                Debug.LogWarning("No reference transform or resources available.");
                return null;
            }

            return resources
                .OrderBy(resource => Vector3.Distance(_targetTransform.position, resource.position))
                .FirstOrDefault();
        }

        public List<Transform> GetClosestResourcesFilteredByDistance(float minDistance = 5f, float maxDistance = 20f, int numberOfResources = 1)
        {
            var filteredResources = FilterResourcesByDistance(minDistance, maxDistance);
            if (filteredResources.Count == 0)
            {
                Debug.LogWarning("No valid resources found within the specified distance.");
                return null;
            }
            if (numberOfResources <= 0 || numberOfResources > filteredResources.Count)
            {
                Debug.LogWarning("Invalid number of resources requested.");
                return null;
            }
            var selectedResources = new List<Transform>();
            for (int i = 0; i < numberOfResources; i++)
            {
                var closestResource = GetClosestResource(filteredResources);
                if (closestResource != null)
                {
                    filteredResources.Remove(closestResource);
                    selectedResources.Add(closestResource);
                }
            }
            return selectedResources;
        }
    }
}
