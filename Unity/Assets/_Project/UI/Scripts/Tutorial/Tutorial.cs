using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Tutorial
{
    /// <summary>
    /// Tutorial UI class.
    /// </summary>
    public class Tutorial : MonoBehaviour
    {
        [SerializeField]
        private GameObject content;

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

        private RectTransform rectTransform;
        private Vector2 originalSize;
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
                rectTransform.sizeDelta = new Vector2(originalSize.x, 38);
            }
            else
            {
                expandCollapseImage.sprite = expandedIcon;
                rectTransform.sizeDelta = new Vector2(originalSize.x, originalSize.y);
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

            rectTransform = content.GetComponent<RectTransform>();
            originalSize = rectTransform.sizeDelta;
            expandCollapseButton.onClick.AddListener(ExpandCollapse);
            UpdateButtons();
        }
    }
}
