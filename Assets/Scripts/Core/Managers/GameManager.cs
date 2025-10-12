
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Managers
{
    public class GameManager :MonoBehaviour
    {
           
        private static GameManager _instance;

        void Awake()
        {
            
            // Singleton pattern implementation
            if (_instance == null)
                _instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }




        
        public void RestartGame()
        {
            // Restart the game by load the "MainMenu" scene
            SceneManager.LoadScene("MainMenu");
        }
        public void ReloadScene()
        {
            // Reload the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        public void PauseGame()
        {
            // Pause the game
            Time.timeScale = 0f;
        }
        public void ResumeGame()
        {
            //Resume the game
            Time.timeScale = 1f;
        }
        public void GameOver()
        {
            QuitGame();
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
Application.Quit();
#endif
        }
        
    }
}



