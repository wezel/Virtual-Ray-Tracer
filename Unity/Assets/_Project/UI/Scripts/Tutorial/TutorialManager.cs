using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.UI.Scripts.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        private int lastScene;
        private int currentScene;

        public void NextLevel()
        {
            if (currentScene < lastScene) SceneManager.LoadSceneAsync(++currentScene);
        }

        private void Awake()
        {
            lastScene = SceneManager.sceneCountInBuildSettings - 1;
            currentScene = SceneManager.GetActiveScene().buildIndex;
        }
    }
}
