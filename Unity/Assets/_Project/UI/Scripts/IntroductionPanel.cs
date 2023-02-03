using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI.Scripts
{
    /// <summary>
    /// A UI class that manages a panel with information shown at the start of scenes. When active, all other UI and
    /// interactions will be blocked.
    /// </summary>
    public class IntroductionPanel : MonoBehaviour
    {
        [SerializeField]
        private Button closeButton;
        private UIManager uiManager;

        // TODO instead of removing functionality remove the function itself
        /// <summary>
        /// Show the introduction panel.
        /// </summary>
        public void Show()
        {
            /*
            if (uiManager == null)
                uiManager = UIManager.Get();

            // Only enable the blocker if the panel is inactive now.
            if (!gameObject.activeInHierarchy)
                uiManager.EnableBlocker();

            gameObject.SetActive(true);
            UIManager.Get().AddEscapable(Hide);
            */
        }

        /// <summary>
        /// Hide the introduction panel.
        /// </summary>
        public void Hide()
        {
            // Only disable the blocker if the panel is active now.
            if (gameObject.activeInHierarchy)
                uiManager.DisableBlocker();

            gameObject.SetActive(false);
            UIManager.Get().RemoveEscapable(Hide);
        }
        
        public void Toggle()
        {
            Hide();
        }

        private void Awake()
        {
            closeButton.onClick.AddListener(Hide);
        }

        private void Start()
        {
            uiManager = UIManager.Get();
        }
    }
}
