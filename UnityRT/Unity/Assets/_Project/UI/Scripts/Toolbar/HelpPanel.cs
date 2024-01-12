using UnityEngine;

namespace _Project.UI.Scripts.Toolbar
{
    /// <summary>
    /// A UI class that provides a basic panel for help information to be shown on.
    /// </summary>
    public class HelpPanel : MonoBehaviour
    {
        /// <summary>
        /// Show the help panel.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            UIManager.Get().AddEscapable(Hide);
        }

        /// <summary>
        /// Hide the help panel.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            UIManager.Get().RemoveEscapable(Hide);
        }

        /// <summary>
        /// Toggle the visibility of the help panel. If the help panel is hidden it will now be shown and vice versa.
        /// </summary>
        public void Toggle()
        {
            if (gameObject.activeSelf)
            {
                Hide();
                return;
            }
            Show();
        }
    }
}
