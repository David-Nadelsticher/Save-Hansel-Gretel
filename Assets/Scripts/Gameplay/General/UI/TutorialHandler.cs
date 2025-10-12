using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Core.Managers;
using Gameplay.Controls.Player;

namespace Gameplay.General.UI
{
    /// <summary>
    /// Handles the display and navigation of tutorial steps in the UI.
    /// Manages step transitions, button feedback, and player control state during the tutorial.
    /// </summary>
    public class TutorialHandler : MonoBehaviour
    {
        #region Nested Classes
        /// <summary>
        /// Represents a single step in the tutorial, including its title, description, and optional image.
        /// </summary>
        [Serializable]
        public class TutorialStep
        {
            /// <summary>
            /// The title of the tutorial step.
            /// </summary>
            public string title;
            /// <summary>
            /// The description of the tutorial step.
            /// </summary>
            [TextArea] public string description;
            /// <summary>
            /// The image associated with the tutorial step (optional).
            /// </summary>
            public GameObject stepImage;
        }
        #endregion

        #region Inspector Fields
        [Header("Visual Feedback")]
        [SerializeField] private ButtonFeedback nextButtonFeedback; // Feedback for the next button
        [SerializeField] private ButtonFeedback backButtonFeedback; // Feedback for the back button
        [SerializeField] private PlayerController playerController; // Reference to the player controller
        public TutorialStep[] steps; // Array of tutorial steps
        public TextMeshProUGUI titleText; // UI text for the step title
        public TextMeshProUGUI descriptionText; // UI text for the step description
        public Button nextButton; // Button to go to the next step
        public Button backButton; // Button to go to the previous step
        #endregion

        #region Private Fields
        private GameObject _currentActive; // Currently active step image
        private int _currentIndex; // Current step index
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes the tutorial handler, sets up button listeners, and shows the tutorial.
        /// </summary>
        void Start()
        {
            nextButton.onClick.AddListener(NextStep);
            backButton.onClick.AddListener(PreviousStep);
            ShowTutorial();
            playerController.enabled = false; // Disable player control during tutorial
        }

        /// <summary>
        /// Handles keyboard input for navigating the tutorial.
        /// </summary>
        void Update()
        {
            // Advance to next step or complete tutorial on Enter
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (_currentIndex == steps.Length - 1)
                    CompleteTutorial();
                else
                    NextStep();
            }

            // Go to previous step on Left Shift
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                PreviousStep();
            }
        }
        #endregion

        #region Tutorial Navigation
        /// <summary>
        /// Updates the UI to reflect the current tutorial step.
        /// </summary>
        public void UpdateTutorial()
        {
            // Deactivate the previous step image if it exists
            if (_currentActive != null)
                _currentActive.SetActive(false);

            // Update title and description text
            titleText.text = steps[_currentIndex].title;
            descriptionText.text = steps[_currentIndex].description;

            // Activate the current step image if it exists
            if (steps[_currentIndex].stepImage != null)
            {
                _currentActive = steps[_currentIndex].stepImage;
                _currentActive.SetActive(true);
            }

            // Enable/disable navigation buttons based on current step
            backButton.interactable = _currentIndex > 0;
            nextButton.interactable = _currentIndex < steps.Length;
        }

        /// <summary>
        /// Advances to the next tutorial step if possible.
        /// </summary>
        public void NextStep()
        {
            if (_currentIndex < steps.Length - 1)
            {
                nextButtonFeedback?.PlayFeedback(); // Play feedback if assigned
                _currentIndex++;
                UpdateTutorial();
            }
        }

        /// <summary>
        /// Returns to the previous tutorial step if possible.
        /// </summary>
        public void PreviousStep()
        {
            if (_currentIndex > 0)
            {
                backButtonFeedback?.PlayFeedback(); // Play feedback if assigned
                _currentIndex--;
                UpdateTutorial();
            }
        }
        #endregion

        #region Tutorial Completion
        /// <summary>
        /// Completes the tutorial, enables player control, and triggers the start game event.
        /// </summary>
        void CompleteTutorial()
        {
            playerController.enabled = true; // Re-enable player control
            EventManager.Instance.InvokeEvent(EventNames.OnStartGame, null); // Trigger start game event
            gameObject.SetActive(false); // Hide the tutorial UI
        }
        #endregion

        #region Public API
        /// <summary>
        /// Shows the tutorial UI and resets to the first step.
        /// </summary>
        public void ShowTutorial()
        {
            gameObject.SetActive(true);
            _currentIndex = 0;
            UpdateTutorial();
        }
        #endregion
    }
}
