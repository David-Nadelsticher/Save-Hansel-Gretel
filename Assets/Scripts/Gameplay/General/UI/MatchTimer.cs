using System;
using System.Collections;
using Core.Managers;
using TMPro;
using UnityEngine;

using UnityEngine.UI;



namespace Gameplay.General.UI
{
    public class MatchTimer : MonoBehaviour
    {
        [Header("Timer Settings")] [Tooltip("Total match time in minutes.")] [SerializeField]
        private float matchDurationMinutes = 5f; // Duration in minutes

        [Header("Time Scale")]
        [Tooltip("Multiplier for timer speed (0.1 = 10% speed, 2 = double speed)")]
        [Range(0.1f, 3f)]
        [SerializeField]
        private float timeScaleMultiplier = 1f;


        [Header("UI References")] [Tooltip("UI TextMeshPro element to display the countdown.")] [SerializeField]
        private TMP_Text timerText;

        [SerializeField] private Image timerPaddingImage;

        [Header("Timer Behavior")] [Tooltip("If true, the timer starts immediately on game start.")] [SerializeField]
        private bool startImmediately;

        [SerializeField] private UIShaker timerShaker;
        [SerializeField] private float endGameDelay;

        private float _remainingTimeSeconds;
        private bool _isRunning;
        private int _lastWarnedMinute; // To track when the last minute warning was given
        private Color _originalTimerPaddingColor; // Store the original color of the timer padding image
        private bool _isFlashing; // To control the flashing coroutine
        private bool _lastMinuteTriggered; // Track if last minute logic was triggered
        private bool _lastTenSecondsTriggered; // Track if last 10 seconds logic was triggered
        private float _originalTimeScaleMultiplier;

        void Start()
        {
            _originalTimeScaleMultiplier = timeScaleMultiplier;
            _isRunning = startImmediately;
            InitializeTimer();
        }

        void Update()
        {
            if (!_isRunning) return;

            _remainingTimeSeconds -= Time.deltaTime * timeScaleMultiplier;

            // Check for minute warnings
            int currentMinutes = Mathf.CeilToInt(_remainingTimeSeconds / 60f);
            if (currentMinutes < _lastWarnedMinute)
            {
                _lastWarnedMinute = currentMinutes;
                // Play clock ping sound for each minute warning
                //  AudioManager.instance.Play("ClockPing"); // Play clock ping sound for each minute warning
                EventManager.Instance.InvokeEvent(EventNames.OnMinuteWarning, obj: currentMinutes);

                timerShaker.Shake();
            }

            // --- Last minute logic ---
            if (_remainingTimeSeconds <= 60f && !_lastMinuteTriggered)
            {
                _lastMinuteTriggered = true;
                // AudioManager.instance.Play("TickingClock");


                // Trigger special last minute warning event
                EventManager.Instance.InvokeEvent(EventNames.OnLastMinuteWarning, obj: null);
                // Start flashing if not already
                if (!_isFlashing)
                    StartCoroutine(FlashTimerPadding());
            }

            // --- Last 10 seconds logic ---
            if (_remainingTimeSeconds <= 10f && !_lastTenSecondsTriggered)
            {
                timeScaleMultiplier = 1f; // Reset timer speed to normal

                // Mute main music and play ticking clock
                //AudioManager.instance.Stop("GameBGMusic");


                _lastTenSecondsTriggered = true;
                // Trigger the last Ten-second warning event
                EventManager.Instance.InvokeEvent(EventNames.OnLastTenSecondsWarning, obj: null);
            }

            if (_remainingTimeSeconds <= 0f)
            {
                _remainingTimeSeconds = 0f;
                _isRunning = false;
                StopAllCoroutines(); // Stop flashing when timer ends
                timerShaker.Shake();
                timerPaddingImage.color = _originalTimerPaddingColor; // Reset color
                UpdateTimerUI();

                //TODO - Add Audio 

                // AudioManager.instance.Play("Buzzer"); // Play buzzer sound when time is over
                // Stop ticking clock and restore main music if needed
                // AudioManager.instance.Stop("TickingClock");


                // Trigger game end event

                StartCoroutine(EndGame(
                    endGameDelay)); // Adjust the wait time as needed*/
            }
            else
            {
                UpdateTimerUI();
            }
        }

        private IEnumerator EndGame(float delay)
        {
            yield return new WaitForSeconds(delay);
            // Trigger game end event
            EventManager.Instance.InvokeEvent(EventNames.OnTimeOver, obj: false);
      
        }




        private void ContinueTheGame()
        {
            RunTimer();
        }

        private void StartGame()
        {
            _isRunning = true;
            InitializeTimer();
        }
        private void OnEnable()
        {
            EventManager.Instance.AddListener(EventNames.OnStartGame, _ => StartGame());
            EventManager.Instance.AddListener(EventNames.OnEndGame, _ => StopAndResetFlashing());
            EventManager.Instance.AddListener(EventNames.OnResetTimer, _ => ResetTimer());
            EventManager.Instance.AddListener(EventNames.OnBossDefeated, _ => StopTimer());
            EventManager.Instance.AddListener(EventNames.OnPlayerDeath, _ => StopTimer());

        }
        private void OnDisable()
        {
            EventManager.Instance.RemoveListener(EventNames.OnStartGame, _ => StartGame());
            EventManager.Instance.RemoveListener(EventNames.OnEndGame, _ => StopAndResetFlashing());
            EventManager.Instance.RemoveListener(EventNames.OnResetTimer, _ => ResetTimer());
            EventManager.Instance.RemoveListener(EventNames.OnBossDefeated, _ => StopTimer());
            EventManager.Instance.RemoveListener(EventNames.OnPlayerDeath, _ => StopTimer());

            StopAllCoroutines(); // Ensure coroutines are stopped when disabled
            if (timerPaddingImage != null)
            {
                timerPaddingImage.color = _originalTimerPaddingColor; // Reset color on disable
            }
        }

        /// <summary>
        /// Initializes and starts the countdown based on minutes.
        /// </summary>
        public void InitializeTimer()
        {
            if (timerText == null)
            {
                Debug.LogError("[MatchTimer] Timer Text is not assigned!");
                return;
            }

            if (timerPaddingImage != null)
            {
                _originalTimerPaddingColor = timerPaddingImage.color; // Store original color
            }

            timeScaleMultiplier = _originalTimeScaleMultiplier;
            _remainingTimeSeconds = matchDurationMinutes * 60f; // Convert minutes to seconds
            _lastWarnedMinute = Mathf.CeilToInt(_remainingTimeSeconds / 60f); // Initialize with total minutes
            // _isRunning = startImmediately;
            _isFlashing = false; // Reset flashing state
            _lastMinuteTriggered = false; // Reset last minute flag
            _lastTenSecondsTriggered = false; // Reset last 10 seconds flag
            StopAllCoroutines(); // Stop any lingering coroutines
            UpdateTimerUI();
        }

        /// <summary>
        /// Stops the timer prematurely.
        /// </summary>
        public void StopTimer()
        {
            _isRunning = false;
        }

        public void HandleFinishLineReached()
        {
            StopTimer();
        }


        public void RunTimer()
        {
            _isRunning = true;
        }


        private void UpdateTimerUI()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(_remainingTimeSeconds);
            // Format as MM:SS
            timerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }

        /// <summary>
        /// Coroutine to make the timer padding image flash red and black.
        /// </summary>
        private IEnumerator FlashTimerPadding()
        {
            _isFlashing = true;
            while (_remainingTimeSeconds > 0f)
            {
                // Flash red
                timerPaddingImage.color = Color.red;
                yield return new WaitForSeconds(0.25f);
                // Flash black
                timerPaddingImage.color = Color.black;
                yield return new WaitForSeconds(0.25f);
            }

            _isFlashing = false;
            timerPaddingImage.color = _originalTimerPaddingColor; // Ensure color is reset when loop ends
        }

        /// <summary>
        /// Stops the flashing coroutine and resets the color.
        /// Called when the game time ends.
        /// </summary>
        private void StopAndResetFlashing()
        {
            StopAllCoroutines();
            if (timerPaddingImage != null)
            {
                timerPaddingImage.color = _originalTimerPaddingColor; // Reset color
            }

            _isFlashing = false; // Ensure flashing state is reset
        }

        /// <summary>
        /// Returns the current remaining time in seconds.
        /// </summary>
        public float GetRemainingTimeSeconds()
        {
            return _remainingTimeSeconds;
        }

        /// <summary>
        /// Returns the current remaining time in minutes.
        /// </summary>
        public float GetRemainingTimeMinutes()
        {
            return _remainingTimeSeconds / 60f;
        }

        /// <summary>
        /// Stops the timer and resets the time to the initial value.
        /// </summary>
        public void StopAndResetTimer()
        {
            StopTimer();
            StopAndResetFlashing();
            InitializeTimer();
        }

        /// <summary>
        /// Reset the timer logic here.
        /// </summary>
        public void ResetTimer()
        {
            StopAndResetTimer();
            RunTimer();
            Debug.Log("Timer reset by event!");
        }
    }
}