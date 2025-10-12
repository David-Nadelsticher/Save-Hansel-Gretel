using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class FollowPlayerUI : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float smoothSpeed = 5f;

        private RectTransform _uiElement;
        private Camera _camera;

        private Vector3 _currentScreenPos;

        void Start()
        {
            _camera = Camera.main;
            _uiElement = GetComponent<RectTransform>();
            _currentScreenPos = _uiElement.position;
        }

        void Update()
        {
            if (_camera == null || player == null) return;

            Vector3 worldPosition = player.position + offset;
            Vector3 targetScreenPos = _camera.WorldToScreenPoint(worldPosition);

            if (targetScreenPos.z > 0)
            {
                _currentScreenPos = Vector3.Lerp(_currentScreenPos, targetScreenPos, Time.deltaTime * smoothSpeed);
                _uiElement.position = _currentScreenPos;
            }
            else
            {
                _uiElement.position = new Vector3(-1000, -1000, 0);
            }
        }
    }
}

/*using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class FollowPlayerUI : MonoBehaviour
    {
        public Transform player;
        public Vector3 offset;
        private RectTransform _uiElement;
        private Camera _camera;


        void Start()
        {
            _camera = Camera.main;
            _uiElement = GetComponent<RectTransform>();
        }

        void Update()
        {
            Vector3 screenPos = _camera.WorldToScreenPoint(player.position + offset);
            _uiElement.position = screenPos;
        }
    }
}*/