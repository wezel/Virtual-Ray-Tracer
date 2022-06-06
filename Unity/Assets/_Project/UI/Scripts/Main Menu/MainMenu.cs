using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.UI.Scripts.Main_Menu
{
    /// <summary>
    /// A UI class that provides a main menu.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private LevelsPanel levelsPanel;
        [SerializeField]
        private SettingsPanel settingsPanel;
        
        private int lastScene;
        private int currentScene;

        public void Show()
        {
            UIManager uiManager = UIManager.Get();
            uiManager.AddEscapable(Hide);
            gameObject.SetActive(true);
            uiManager.EnableBlocker();
        }

        public void Hide()
        {
            levelsPanel.Hide();
            settingsPanel.Hide();
            
            UIManager uiManager = UIManager.Get();
            uiManager.RemoveEscapable(Hide);
            uiManager.DisableBlocker();
            gameObject.SetActive(false);
        }
        /// <summary>
        /// Toggle the main menu. If the main menu is hidden it will now be shown and vice versa.
        /// </summary>
        public void Toggle()
        {
            if (gameObject.activeSelf) Hide();
            else Show();
        }

        public void ToggleLevelSelector()
        {
            settingsPanel.Hide();
            levelsPanel.Toggle();
        }
        
        public void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else //UNITY_STANDALONE
            Application.Quit();
#endif
        }

        public void GoHome()
        {
            SceneManager.LoadSceneAsync(0);
        }

        public void ToggleSettings()
        {
            levelsPanel.Hide();
            settingsPanel.Toggle();
        }

        public void LoadNextLevel()
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
