using System;
using Core.Data;
using Core.Managers;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Gameplay.General.UI
{

    public class ButtonFeedback : MonoBehaviour
    {
        [Header("References")] public Button button;

        [Header("Settings")] public float scaleAmount = 1.1f;
        public float duration = 0.15f;
        public Color flashColor = Color.yellow;
        public event Action OnCompleteAnimation;
        private Vector3 _originalScale;
        private Color _originalColor;
        [SerializeField] private Image buttonImage;

        void Start()
        {
            InitializeButton();

            // Example: trigger feedback on click
            //button.onClick.AddListener(PlayFeedback);
        }

        private void InitializeButton()
        {
            if (button == null) button = GetComponent<Button>();
            _originalScale = button.transform.localScale;
            if (buttonImage == null)
            {
                Debug.LogWarning(
                    "ButtonFeedback: No Image component found on the button. Color feedback will not work.");
                return;
            }

            _originalColor = buttonImage.color;
        }

        private void OnEnable()
        {
            InitializeButton();
        }

        private void OnDisable()
        {
            ResetButton();
        }

        private void ResetButton()
        {
            // Reset button scale and color when disabled
            button.transform.localScale = _originalScale;
            if (buttonImage != null)
            {
                buttonImage.color = _originalColor;
            }

            // Ensure no animations are running when the button is disabled
            button.transform.DOKill(); // Cancel any existing animations
        }

        public void PlayFeedback()
        {
            AudioManager.Instance.PlaySoundByAudioType(GameSoundsSo.AudioType.ButtonClick);
            ScaleFeedback();
            ColorFeedback();
        }

        private void ScaleFeedback()
        {
            // Scale feedback
            button.transform.DOKill(); // Cancel any existing animations
            button.transform.localScale = _originalScale;
            button.transform
                .DOScale(_originalScale * scaleAmount, duration / 2)
                .SetEase(Ease.OutBack)
                .OnComplete(() => { button.transform.DOScale(_originalScale, duration / 2).SetEase(Ease.InOutQuad); });
        }

        private void ColorFeedback()
        {
            // Color feedback
            buttonImage.DOKill();
            buttonImage.color = _originalColor;
            buttonImage
                .DOColor(flashColor, duration / 2)
                .OnComplete(() =>
                {
                    buttonImage.DOColor(_originalColor, duration / 2).OnComplete(() =>
                    {
                        OnCompleteAnimation?.Invoke();
                    });
                });
        }

    }
}