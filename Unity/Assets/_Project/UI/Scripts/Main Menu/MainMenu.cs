using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace _Project.UI.Scripts.Main_Menu
{
    /// <summary>
    /// A UI class that provides a main menu.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Serializable]
        public class Event : UnityEvent { }
        public Event OnMainMenuShown;

        [SerializeField]
        private LevelsPanel levelsPanel;
        [SerializeField]
        private SettingsPanel settingsPanel;
        [SerializeField]
        private BadgesPanel badgesPanel;

        public void Show()
        {
            Overlay.Get().ShowBlocker(Hide);
            gameObject.SetActive(true);
            OnMainMenuShown?.Invoke();
        }

        public void Hide()
        {
            levelsPanel.Hide();
            settingsPanel.Hide();
            badgesPanel.Hide();
            
            Overlay.Get().HideBlocker(Hide);
            gameObject.SetActive(false);
        }
        /// <summary>
        /// Toggle the main menu. If the main menu is hidden it will now be shown and vice versa.
        /// </summary>
        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }

        public void ToggleLevelSelector()
        {
            settingsPanel.Hide();
            badgesPanel.Hide();
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
            LevelManager.Get().LoadLevel(2);
        }

        public void ToggleSettings()
        {
            levelsPanel.Hide();
            badgesPanel.Hide();
            settingsPanel.Toggle();
        }

        public void ToggleBadges()
        {
            settingsPanel.Hide();
            levelsPanel.Hide();
            badgesPanel.Toggle();
        }

        public void LoadNextLevel()
        {
            LevelManager.Get().LoadNextLevel();
        }
        
    }
}
