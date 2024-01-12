using UnityEngine;

namespace _Project.UI.Scripts.Tooltips
{
    /// <summary>
    /// Manages tooltips.
    /// </summary>
    public class TooltipManager : MonoBehaviour
    {
        private static TooltipManager instance = null;
        
        [SerializeField]
        private Tooltip tooltip;

        /// <summary>
        /// Get the current <see cref="TooltipManager"/> instance.
        /// </summary>
        /// <returns> The current <see cref="TooltipManager"/> instance. </returns>
        public static TooltipManager Get()
        {
            return instance;
        }

        /// <summary>
        /// Show the tooltip window.
        /// </summary>
        /// <param name="content"> The content of the tooltip window. </param>
        public void Show(string content)
        {
            // Do not show the tooltip if it is empty.
            if (string.IsNullOrEmpty(content))
                return;

            tooltip.SetText(content);
            tooltip.gameObject.SetActive(true);
            StartCoroutine(tooltip.FadeIn());
        }

        /// <summary>
        /// Hide the tooltip window.
        /// </summary>
        public void Hide()
        {
            tooltip.gameObject.SetActive(false);
        }

        private void Awake()
        {
            instance = this;
        }
    }
}

