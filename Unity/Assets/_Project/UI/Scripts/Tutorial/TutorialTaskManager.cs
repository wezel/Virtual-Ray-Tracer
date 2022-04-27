using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Tutorial
{
    /// <summary>
    /// Manager for the tutorial task UI
    /// </summary>
    public class TutorialTaskManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject fill;

        [SerializeField]
        private Color taskColor;

        /// <summary>
        /// Set background color
        /// </summary>
        private void Awake()
        {
            fill.GetComponent<Image>().color = taskColor;
        }
    }
}

