using System;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.General.UI
{
    [System.Serializable]
    public class AnimatedMessageDisplayer : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Image backgroundImage;
        public event Action OnCompleteMessegeDisplay;
        [Header("Animation Settings")] [SerializeField]
        private float entryScale = 0.5f;

        [SerializeField] private float scaleSize = 2f;
        [SerializeField] private float entryDuration = 0.3f;
        [SerializeField] private float exitDuration = 0.4f;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private Ease entryEase = Ease.OutBack;
        [SerializeField] private Ease exitEase = Ease.InOutQuad;
        [SerializeField] private float fontSize = 36f;
        [SerializeField] private Color32 defaultTextColor = new Color32(255, 255, 255, 255);

        private Vector3 _originalScale;
        private Coroutine _currentRoutine;

        private void Awake()
        {
            if (messageText != null)
            {
                _originalScale = messageText.transform.localScale;
                SetupTextStyle();
            }

            if (backgroundImage != null)
                backgroundImage.gameObject.SetActive(false);
        }

        private void SetupTextStyle()
        {
            messageText.fontSize = fontSize;
            messageText.fontStyle = FontStyles.Bold;
            messageText.color = defaultTextColor;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.outlineWidth = 0.2f;
            messageText.outlineColor = new Color32(0, 0, 0, 255);
        }

        public void ShowMessage(string text, Color32 color)
        {
            StopCurrentRoutine();
            _currentRoutine = StartCoroutine(ShowMessageRoutine(text, color));
        }

        private IEnumerator ShowMessageRoutine(string text, Color32 color)
        {
            if (backgroundImage != null)
                backgroundImage.gameObject.SetActive(true);

            AnimateText(text, color);
            yield return new WaitForSeconds(displayDuration);

            if (backgroundImage != null)
                backgroundImage.gameObject.SetActive(false);

            _currentRoutine = null;
        }

        private void AnimateText(string text, Color32 color)
        {
            if (messageText == null) return;

            messageText.text = text;
            messageText.color = new Color32(color.r, color.g, color.b, 255);
            messageText.transform.localScale = Vector3.one * entryScale;
            messageText.alpha = 0f;

            Sequence msgSequence = DOTween.Sequence();
            msgSequence.Append(
                messageText.transform
                    .DOScale(_originalScale, entryDuration)
                    .SetEase(entryEase)
            );
            msgSequence.Join(
                messageText.DOFade(1, entryDuration)
                    .SetEase(entryEase)
            );
            msgSequence.AppendInterval(displayDuration - (entryDuration + exitDuration));
            msgSequence.Append(
                messageText.transform
                    .DOScale(_originalScale * scaleSize, exitDuration)
                    .SetEase(Ease.OutBack)
            );
            msgSequence.Join(
                messageText.DOFade(0, exitDuration)
                    .SetEase(exitEase)
            );
            //On complete invoke action OnCompleteAnimation
            
            
        }

        public void StopCurrentRoutine()
        {
            if (_currentRoutine != null)
            {
                StopCoroutine(_currentRoutine);
                _currentRoutine = null;
            }

            if (messageText != null)
            {
                messageText.transform.DOKill();
                messageText.DOKill();
            }
        }
    }
}