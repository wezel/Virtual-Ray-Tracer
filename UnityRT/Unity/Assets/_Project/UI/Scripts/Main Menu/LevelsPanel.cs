using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Main_Menu
{
    /// <summary>
    /// A UI class that provides a menu for loading all built scenes except the opening scene.
    /// </summary>
    public class LevelsPanel : MonoBehaviour
    {
        [SerializeField]
        private Button levelsPrefab;

        [SerializeField]
        private GameObject content;

        [SerializeField]
        private Button exitButton;

        [SerializeField]
        private Button loadLevelButton;

        private List<Button> levelsList = new List<Button>();
        private Button selectedButton;
        private MainMenu mainMenu;

        /// <summary>
        /// Show the levels panel.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            OnButtonClicked(levelsList.First()); // Select the first level by default.
            UIManager.Get().AddEscapable(Hide);
        }

        /// <summary>
        /// Hide the levels panel.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            UIManager.Get().RemoveEscapable(Hide);
        }

        /// <summary>
        /// Toggle the levels panel. If the levels panel is hidden it will now be shown and vice versa.
        /// </summary>
        public void Toggle()
        {
            if (gameObject.activeSelf) 
                Hide();
            else
                Show();
        }

        private void OnButtonClicked(Button clickedButton)
        {
            // Set all level buttons to be interactable.
            foreach (Button levelButton in levelsList)
                levelButton.interactable = true;

            // Select the clicked button and prevent any further interaction.
            clickedButton.interactable = false;
            selectedButton = clickedButton;
        }

        private void LoadLevel()
        {
            mainMenu.Toggle();
            SceneManager.LoadSceneAsync(selectedButton.GetComponentInChildren<TextMeshProUGUI>().text);
        }

        private void Awake()
        {
            exitButton.onClick.AddListener(Hide);
            loadLevelButton.onClick.AddListener(LoadLevel);
            mainMenu = gameObject.GetComponentInParent<MainMenu>();

            // Set up a button for each scene. We start the index at 2 because we skip the start and initialize scene.
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 1; i < sceneCount; i++)
            {
                Button levelButton = Instantiate(levelsPrefab, content.transform);
                string levelName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));

                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = levelName;
                levelButton.onClick.AddListener(() => OnButtonClicked(levelButton));
                levelsList.Add(levelButton);
            }
            
        }
    }
}