using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

namespace _Project.UI.Scripts.Tutorial
{
    /// <summary>
    /// Manager for the tutorial task UI
    /// </summary>
    public class TutorialTaskManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject fill;
        private RectTransform fillRect;

        [SerializeField]
        private Color taskColor;

        [SerializeField]
        private TextMeshProUGUI taskName;
        [SerializeField]
        private TextMeshProUGUI taskDescription;

        [SerializeField]
        private GameObject optionalElement;

        [SerializeField]
        private List<TutorialTasks> tasks;

        [SerializeField]
        private bool isOptional;

        private TutorialTasks currentTasks;

        /// <summary>
        /// Update the UI
        /// </summary>
        private void UpdateUI()
        {
            Vector2 size = GetComponent<RectTransform>().sizeDelta;
            fillRect.sizeDelta = new Vector2(size.x * currentTasks.GetPercentage(), fillRect.sizeDelta.y);

            if (currentTasks.IsFinished())
                return;

            taskName.text = currentTasks.GetName();
            taskDescription.text = currentTasks.GetDescription();
        }

        /// <summary>
        /// Try to complete a task, then update the UI
        /// </summary>
        /// <param name="identifier"></param>
        public void CompleteTask(string identifier)
        {
            if (currentTasks != null && currentTasks.CompleteTask(identifier))
                UpdateUI();
        }

        /// <summary>
        /// Set background color, update UI
        /// </summary>
        private void Awake()
        {
            fill.GetComponent<Image>().color = taskColor;
            fillRect = fill.GetComponent<RectTransform>();
            optionalElement.SetActive(isOptional);

            int level = SceneManager.GetActiveScene().buildIndex - 1;
            currentTasks = level < 0 || level >= tasks.Count ? null : tasks[level];
            if (currentTasks != null)
                UpdateUI();
        }
    }
}

