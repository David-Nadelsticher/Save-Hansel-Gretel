using System.Collections;
using UnityEngine;

namespace Gameplay.General.UI
{
    public class UIShaker : MonoBehaviour
    {
        [SerializeField] private float duration;
        [SerializeField] private float magnitude;
        private Vector3 _originalPosition;
        private Coroutine _shakeCoroutine;
        private RectTransform _uiElement;

        private void Awake()
        {
            // Initialize the RectTransform if not set
            _uiElement = GetComponent<RectTransform>();
            if (_uiElement == null)
            {
                Debug.LogError("[UIShaker] RectTransform component is missing on the GameObject.");
                return;
            }
            _originalPosition =  _uiElement.anchoredPosition;;
        }

        public void SetDuration(float shakeDuration)
        {
            duration = shakeDuration;
        }

        public void SetMagnitude(float shakeMagnitude)
        {
            magnitude = shakeMagnitude;
        }


        public void Shake( /*PlayerType playerType*/)
        {
            // Check if the player ID matches the current player's ID
            /*if (playerType != playerID)
            {
                Debug.LogWarning($"[UIShaker] Player ID mismatch: {playerType} != {playerID}. Shake not triggered.");
                return;
            }*/

            // If a shake is already in progress, stop it
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
                Debug.Log("[UIShaker] Previous shake coroutine stopped.");
                _uiElement.anchoredPosition = _originalPosition; // Reset position when disabled
                
            }

            _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
            
        }


        private IEnumerator ShakeCoroutine(float shakeDuration, float shakeMagnitude)
        {

            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                float x = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
                float y = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
                //transform.localPosition = new Vector3(x, y, _originalPosition.z);
                _uiElement.anchoredPosition = _originalPosition + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            _uiElement.anchoredPosition = _originalPosition; // Reset position after shaking
            _shakeCoroutine = null; // Clear the coroutine reference

        }


        void OnDisable()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
                Debug.Log("Shake coroutine stopped.");
            }

            _uiElement.anchoredPosition = _originalPosition; // Reset position when disabled
        }

    }
}