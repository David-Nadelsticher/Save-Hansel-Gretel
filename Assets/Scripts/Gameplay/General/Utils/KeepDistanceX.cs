using UnityEngine;
namespace Gameplay.General.Utils
{


    public class KeepDistanceX : MonoBehaviour
    {
        [SerializeField]private Transform target;
        private float _initialXDistance;

        void Start()
        {
            if (target != null)
            {
                _initialXDistance = transform.position.x - target.position.x;
            }
        }

        void Update()
        {
            if (target != null)
            {
                Vector3 newPosition = transform.position;
                newPosition.x = target.position.x + _initialXDistance;
                transform.position = newPosition;
            }
        }
    }

}