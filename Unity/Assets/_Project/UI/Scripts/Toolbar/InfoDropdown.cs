using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Toolbar
{
    /// <summary>
    /// A UI class that provides a dropdown button that when clicked expands to show a wall of text.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class InfoDropdown : MonoBehaviour
    {
        [SerializeField]
        private Image icon;
        [SerializeField]
        private TextMeshProUGUI content;

        [SerializeField]
        private Sprite openedIcon;
        [SerializeField]
        private Sprite closedIcon;

        private bool opened = false;
        private RectTransform rectTransform;

        /// <summary>
        /// Open the dropdown.
        /// </summary>
        public void Open()
        {
            icon.sprite = openedIcon;
            content.gameObject.SetActive(true);
            RecalculateHeight();

            opened = true;
        }

        /// <summary>
        /// Close the dropdown.
        /// </summary>
        public void Close()
        {
            icon.sprite = closedIcon;
            content.gameObject.SetActive(false);
            RecalculateHeight();

            opened = false;
        }

        /// <summary>
        /// Toggle the dropdown. If the dropdown is closed it will now be opened and vice versa.
        /// </summary>
        public void Toggle()
        {
            if (opened)
                Close();
            else
                Open();
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void RecalculateHeight()
        {
            // Determine the combined height of all children.
            float totalHeight = 0.0f;
            foreach (RectTransform child in transform)
            {
                if (child != null && child.gameObject.activeSelf)
                    totalHeight += child.rect.height;
            }

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

            // A change in height will likely cause format problems. Toggle the parent to force a recalculation.
            transform.parent.gameObject.SetActive(false);
            transform.parent.gameObject.SetActive(true);
        }
    }
}
