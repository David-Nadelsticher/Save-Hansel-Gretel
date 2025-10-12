using UnityEngine;

namespace Core.Managers
{
    public class CheatSheetManager : MonoBehaviour
    {
        [Header("Cheat Keys")] [SerializeField]
        private KeyCode resetTimerKey = KeyCode.R;

        [SerializeField] private KeyCode restartSceneKey = KeyCode.T;
        [SerializeField] private KeyCode pauseKey = KeyCode.P;
        [SerializeField] private KeyCode resumeKey = KeyCode.O;
        [SerializeField] private KeyCode quitKey = KeyCode.Q;
        [SerializeField] private KeyCode restartGameKey = KeyCode.G;

        [Header("Debug Mode")] public bool debugMode = true;

        void Update()
        {
            if (Input.GetKeyDown(resetTimerKey))
            {
                if (debugMode) Debug.Log("[CheatSheetManager] Reset Timer Cheat Activated");
                if (EventManager.Instance != null)
                    EventManager.Instance.InvokeEvent(EventNames.OnResetTimer, null);
            }

            if (Input.GetKeyDown(restartSceneKey))
            {
                if (debugMode) Debug.Log("[CheatSheetManager] Restart Scene Cheat Activated");
                GameManager.Instance.ReloadScene();
            }

            if (Input.GetKeyDown(pauseKey))
            {
                if (debugMode) Debug.Log("[CheatSheetManager] Pause Cheat Activated");
                GameManager.Instance.PauseGame();
            }

            if (Input.GetKeyDown(resumeKey))
            {
                if (debugMode) Debug.Log("[CheatSheetManager] Resume Cheat Activated");
                GameManager.Instance.ResumeGame();
            }

            if (Input.GetKeyDown(quitKey))
            {
                if (debugMode) Debug.Log("[CheatSheetManager] Quit Cheat Activated");
                GameManager.Instance.GameOver();
            }

            if (Input.GetKeyDown(restartGameKey))
            {
                if (debugMode) Debug.Log("[CheatSheetManager] Restart Game Cheat Activated");
                GameManager.Instance.RestartGame();
            }
        }
    }
}