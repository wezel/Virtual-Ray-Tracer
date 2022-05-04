using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using _Project.Scripts;
using System.Collections;

namespace _Project.UI.Scripts.Tutorial
{
    /// <summary>
    /// Tutorial task UI class
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        private static TutorialManager instance;
        private const string DEFAULT_REQUIRED_NAME = "All required tasks completed!";
        private const string DEFAULT_OPTIONAL_NAME = "All optional tasks completed!";
        private const string DEFAULT_REQUIRED_DESC = "You may press \"Next Level\" now to move onto the next level or click the button below to continue with additional tasks.";
        private const string DEFAULT_OPTIONAL_DESC = "You may press \"Next Level\" now to move onto the next level or do some more exploring.";

        [SerializeField]
        private GameObject content;

        [SerializeField]
        private Button nextLevelButton;
        [SerializeField]
        private Button previousLevelButton;

        [SerializeField]
        private Button skipButton;

        [SerializeField]
        private Button expandCollapseButton;
        [SerializeField]
        private Image expandCollapseImage;

        [SerializeField]
        private Sprite expandedIcon;
        [SerializeField]
        private Sprite collapsedIcon;

        [SerializeField]
        private GameObject fill;
        private RectTransform fillRect;

        [SerializeField]
        private Color requiredTaskColor;

        [SerializeField]
        private Color optionalTaskColor;

        [SerializeField]
        private int maxWidth;

        [SerializeField]
        private TextMeshProUGUI taskName;
        [SerializeField]
        private TextMeshProUGUI taskDescription;

        private Tasks currentTasks;

        private RectTransform rectTransform;
        private Vector2 originalSize;

        private int lastScene;
        private int currentScene;

        /// <summary>
        /// Get the current <see cref="TutorialManager"/> instance.
        /// </summary>
        /// <returns> The current <see cref="TutorialManager"/> instance. </returns>
        public static TutorialManager Get()
        {
            return instance;
        }

        /// <summary>
        /// Load next level (if possible) and update the buttons.
        /// </summary>
        public void NextLevel()
        {
            if (nextLevelButton.interactable && currentScene < lastScene)
                SceneManager.LoadSceneAsync(++currentScene);
        }

        /// <summary>
        /// Load previous level (if possible) and update the buttons.
        /// </summary>
        public void PreviousLevel()
        {
            if (previousLevelButton.interactable && currentScene > 1)
                SceneManager.LoadSceneAsync(--currentScene);
        }

        /// <summary>
        /// Skip a tutorial task
        /// </summary>
        public static void SkiptTask()
        {
            TutorialManager manager = Get();
            manager.SkipTaskInternal();
        }

        /// <summary>
        /// Complete a tutorial task
        /// </summary>
        /// <param name="identifier"></param>
        public static void CompleteTask(string identifier)
        {
            TutorialManager manager = Get();
            manager.StartCoroutine(manager.CompleteTaskInternal(identifier));
        }

        /// <summary>
        /// Complete a tutorial task and update the UI if necessary
        /// </summary>
        /// <param name="identifier"></param>
        private IEnumerator CompleteTaskInternal(string identifier)
        {
            yield return new WaitForSeconds(.2f);
            if (currentTasks.CompleteTask(identifier)) UpdateTutorial();
        }

        /// <summary>
        /// Skip a tutorial task and update the UI if necessary
        /// </summary>
        private void SkipTaskInternal()
        {
            if (currentTasks.SkipTask()) UpdateTutorial();
        }

        /// <summary>
        /// Expand or collapse the tutorial tasks
        /// </summary>
        private void ExpandCollapse()
        {
            GlobalSettings.TutorialExpanded = !GlobalSettings.TutorialExpanded;
            UpdateExpandCollapse();
        }

        /// <summary>
        /// Update the collapse/expand state
        /// </summary>
        private void UpdateExpandCollapse()
        {
            if (GlobalSettings.TutorialExpanded)
            {
                expandCollapseImage.sprite = expandedIcon;
                rectTransform.sizeDelta = new Vector2(originalSize.x, originalSize.y);
            }
            else
            {
                expandCollapseImage.sprite = collapsedIcon;
                rectTransform.sizeDelta = new Vector2(originalSize.x, 38);
            }
        }

        /// <summary>
        /// Updates all UI elements
        /// </summary>
        private void UpdateTutorial()
        {
            // Set the fill color
            fill.GetComponent<Image>().color = currentTasks.IsRequired() ? requiredTaskColor : optionalTaskColor;

            // Set the fill width/percentage
            fillRect.sizeDelta = new Vector2(maxWidth * currentTasks.GetPercentage(), fillRect.sizeDelta.y);

            // Set the name and description
            taskName.text = currentTasks.GetName();
            if (taskName.text == "") taskName.text = currentTasks.IsRequired() ? DEFAULT_REQUIRED_NAME : DEFAULT_OPTIONAL_NAME;
            taskDescription.text = currentTasks.GetDescription();
            if (taskDescription.text == "") taskDescription.text = currentTasks.IsRequired() ? DEFAULT_REQUIRED_DESC : DEFAULT_OPTIONAL_DESC;

            previousLevelButton.interactable = currentScene > 1;
            nextLevelButton.interactable = currentScene < lastScene && currentTasks.RequiredTasksFinished();

            skipButton.gameObject.SetActive(currentTasks.IsLastRequiredTask());
        }

        /// <summary>
        /// Initialize variables, update UI
        /// </summary>
        private void Start()
        {
            lastScene = SceneManager.sceneCountInBuildSettings - 1;
            currentScene = SceneManager.GetActiveScene().buildIndex;
            rectTransform = content.GetComponent<RectTransform>();
            originalSize = rectTransform.sizeDelta;
            expandCollapseButton.onClick.AddListener(ExpandCollapse);
            fillRect = fill.GetComponent<RectTransform>();
            int level = SceneManager.GetActiveScene().buildIndex - 1;
            if (level < 0 || level >= GlobalSettings.Get().TutorialTasks.Count) currentTasks = new Tasks();
            else currentTasks = GlobalSettings.Get().TutorialTasks[level];

            UpdateTutorial();
            UpdateExpandCollapse();
        }

        private void Awake()
        {
            instance = this;
        }
    }
}

