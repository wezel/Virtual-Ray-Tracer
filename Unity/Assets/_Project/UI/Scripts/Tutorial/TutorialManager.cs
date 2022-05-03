using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

namespace _Project.UI.Scripts.Tutorial
{
    /// <summary>
    /// Tutorial task UI class
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        private static TutorialManager instance;

        [SerializeField]
        private GameObject fill;
        private RectTransform fillRect;

        [SerializeField]
        private Color taskColor;

        [SerializeField]
        private TextMeshProUGUI taskName;
        [SerializeField]
        private TextMeshProUGUI taskDescription;

        /// <summary>
        /// Get the current <see cref="TutorialManager"/> instance.
        /// </summary>
        /// <returns> The current <see cref="TutorialManager"/> instance. </returns>
        public static TutorialManager Get()
        {
            return instance;
        }

        /// <summary>
        /// Set background color, update UI
        /// </summary>
        private void Awake()
        {
            instance = this;
        }
    }
}

