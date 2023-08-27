using System.Collections;
using System.Collections.Generic;
using _Project.UI.Scripts.Tutorial;
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

        private List<Button> levelButtons = new();

        /// <summary>
        /// Show the levels panel.
        /// </summary>
        public void Show()
        {
            // make levels enabled/disabled
            for (int i = 0; i < levelButtons.Count; i++)
                levelButtons[i].interactable = TutorialManager.Get().CanLevelBeLoaded(i + 1);

            gameObject.SetActive(true);
            Overlay.Get().AddEscapable(Hide);
        }

        /// <summary>
        /// Hide the levels panel.
        /// </summary>
        public void Hide()
        {
            //TODO there exists 2 versions of this 1 is in main base that one gets destroyed
            Overlay.Get().RemoveEscapable(Hide);
            gameObject.SetActive(false);
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

        private void Awake()
        {
            exitButton.onClick.AddListener(Hide);

            // Set up a button for each scene. We start the index at 1 because we skip the start and initialize scene.
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 1; i < sceneCount; i++)
            {
                Button levelButton = Instantiate(levelsPrefab, content.transform);
                levelButton.name = i.ToString();

                levelButton.interactable = TutorialManager.Get().CanLevelBeLoaded(i);
                levelButton.GetComponentInChildren<TextMeshProUGUI>().text = i + ". " + System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                levelButton.onClick.AddListener(() => LevelManager.Get().LoadLevel(int.Parse(levelButton.name)));
                levelButtons.Add(levelButton);
            }
        }
    }
}