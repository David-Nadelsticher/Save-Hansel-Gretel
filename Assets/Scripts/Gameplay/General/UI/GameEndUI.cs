using System.Collections;
using Core.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.General.UI
{
    /// <summary>
    /// Handles the end-of-game UI, displaying victory or defeat images and enabling Exit/Restart buttons.
    /// Listens for game end events (player death, time out, or finish line reached) and responds accordingly.
    /// </summary>
    public class GameEndUI : MonoBehaviour
    {
        [Header("UI Elements")] [Tooltip("Button to exit the game.")]
        [SerializeField] private  Button exitButton;
        
        [Tooltip("Button to restart the game.")]
        [SerializeField] private  Button restartButton;
        [SerializeField] private ButtonFeedback exitButtonFeedback;
        [SerializeField] private ButtonFeedback restartButtonFeedback;
        [Tooltip("Image shown on victory.")] [SerializeField] private  Image victoryImage;
        [Tooltip("Image shown on defeat.")] [SerializeField] private  Image defeatImage;
        [Tooltip("Image shown on defeat.")] [SerializeField] private  GameObject endScreenContainer;
    

        private bool _gameEnded;
        private bool _buttonPressed;
        [SerializeField] private float delayBeforeDefeat = 1f;
        [SerializeField] private float delayBeforeVictory = 1f;

        void Awake()
        {
            // Hide all end game UI elements at start
            // Assign button listeners

            victoryImage.gameObject.SetActive(false);
            defeatImage.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(false);
            restartButton.gameObject.SetActive(false);
            endScreenContainer.SetActive(false);

        }

        void OnEnable()
        {
            // Subscribe to relevant game events
            EventManager.Instance.AddListener(EventNames.OnPlayerDeath, TriggerDefeat);
            EventManager.Instance.AddListener(EventNames.OnPlayerSaveThem, TriggerVictory);
            /*exitButton.onClick.AddListener(OnExitClicked);
            restartButton.onClick.AddListener(OnRestartClicked); */
        }

        void OnDisable()
        {
            // Unsubscribe from events to prevent memory leaks
            EventManager.Instance.RemoveListener(EventNames.OnPlayerDeath, TriggerDefeat);
            EventManager.Instance.RemoveListener(EventNames.OnPlayerSaveThem, TriggerVictory);
            /*exitButton.onClick.RemoveListener(OnExitClicked);
            restartButton.onClick.RemoveListener(OnRestartClicked); */
        }

        /// <summary>
        /// Triggers the defeat state: shows defeat image and activates buttons.
        /// </summary>
        public void ShowDefeatScreen()
        {
           
            endScreenContainer.SetActive(true);
            defeatImage.gameObject.SetActive(true);
            victoryImage.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
        }
        void TriggerDefeat(object obj)
        {
            if (_gameEnded) return;
            _gameEnded = true;
            StartCoroutine(TriggerDefeatWithDelay(delayBeforeDefeat));
        }

        private IEnumerator TriggerDefeatWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowDefeatScreen();
        }
        private IEnumerator TriggerVictoryWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowVictoryScreen();
        }
        public void TriggerVictory(object obj)
        {    if (_gameEnded) return;
            _gameEnded = true;
            StartCoroutine(TriggerVictoryWithDelay(delayBeforeVictory));
        }
 

        /// <summary>
        /// Triggers the victory state: shows victory image and activates buttons.
        /// </summary>
        public void ShowVictoryScreen()
        {
    
            endScreenContainer.SetActive(true);
            victoryImage.gameObject.SetActive(true);
            defeatImage.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when the Exit button is clicked. Exits the application.
        /// </summary>
        public void OnExitClicked()
        {
            // In the editor, stop play mode. In build, quit application.
            _buttonPressed = true;
            exitButtonFeedback.PlayFeedback();
            StartCoroutine(ExitGameAfterDelay(0.5f));
        }

        /// <summary>
        /// Called when the Restart button is clicked. Reloads the current scene.
        /// </summary>
        public void OnRestartClicked()
        {
            _buttonPressed = true;
            restartButtonFeedback.PlayFeedback();
            StartCoroutine(RestartGameAfterDelay(0.5f));
        }

        private IEnumerator RestartGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            GameManager.Instance.ReloadScene();
        }
        private IEnumerator ExitGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            GameManager.Instance.GameOver();
        }
 

        void Update()
        {
            if (!_gameEnded&&!_buttonPressed) return;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnRestartClicked();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnExitClicked();
            }
        }

    }
}



