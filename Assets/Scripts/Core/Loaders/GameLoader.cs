using _BossProject.Scripts.Core.Managers;
using BossProject.Core;
using UnityEngine.SceneManagement;

namespace Core.Loaders { 
    public class GameLoader : BaseMono
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            new CoreManager();
            LoadNextScene();
        }

        private void LoadNextScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
