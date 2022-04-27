using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

namespace _Project.UI.Scripts.Tutorial
{
    /// <summary>
    /// This class manages the tutorial UI.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject panel;

        [SerializeField]
        private Button nextLevelButton;
        [SerializeField]
        private Button previousLevelButton;

        [SerializeField]
        private Button expandCollapseButton;
        [SerializeField]
        private Image expandCollapseImage;

        [SerializeField]
        private Sprite expandedIcon;
        [SerializeField]
        private Sprite collapsedIcon;

        [SerializeField]
        private List<TutorialTasks> tutorialTasks;

        private RectTransform panelTransform;
        private bool expanded = true;

        private int lastScene;
        private int currentScene;

        /// <summary>
        /// Load next level (if possible) and update the buttons.
        /// </summary>
        public void NextLevel()
        {
            if (nextLevelButton.interactable && currentScene < lastScene)
                SceneManager.LoadSceneAsync(++currentScene);

            UpdateButtons();
        }

        /// <summary>
        /// Load previous level (if possible) and update the buttons.
        /// </summary>
        public void PreviousLevel()
        {
            if (previousLevelButton.interactable && currentScene > 1)
                SceneManager.LoadSceneAsync(--currentScene);

            UpdateButtons();
        }

        /// <summary>
        /// Update the enabled status of the two buttons.
        /// </summary>
        private void UpdateButtons()
        {
             previousLevelButton.interactable = currentScene > 1;
             nextLevelButton.interactable = currentScene < lastScene;
        }

        /// <summary>
        /// Expand or collapse the tutorial tasks
        /// </summary>
        private void ExpandCollapse()
        {
            if (expanded)
            {
                expandCollapseImage.sprite = collapsedIcon;
                panelTransform.sizeDelta = new Vector2(500, 38);
            }
            else
            {
                expandCollapseImage.sprite = expandedIcon;
                panelTransform.sizeDelta = new Vector2(500, 250);
            }

            expanded = !expanded;
        }

        /// <summary>
        /// Initialize variables, add listeners, and update the buttons.
        /// </summary>
        private void Awake()
        {
            lastScene = SceneManager.sceneCountInBuildSettings - 1;
            currentScene = SceneManager.GetActiveScene().buildIndex;
            panelTransform = panel.GetComponent<RectTransform>();
            expandCollapseButton.onClick.AddListener(ExpandCollapse);
            UpdateButtons();
        }
    }
}
