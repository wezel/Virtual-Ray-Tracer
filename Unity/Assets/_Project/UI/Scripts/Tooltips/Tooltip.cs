using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Tooltips
{
    /// <summary>
    /// A tooltip UI class.
    /// </summary>
    [ExecuteInEditMode]
    public class Tooltip : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI contentField;
        [SerializeField]
        private Image backgroundImage;
        [SerializeField]
        private LayoutElement layoutElement;
        [SerializeField]
        private int characterWrapLimit;
        [SerializeField]
        private float fadeInTime;
        [SerializeField]
        private Vector2 mouseOffset;

        /// <summary>
        /// Set the text for this tooltip.
        /// </summary>
        /// <param name="content"> The text for this tooltip. </param>
        public void SetText(string content)
        {
            contentField.text = content;

            int contentLength = contentField.text.Length;
            layoutElement.enabled = contentLength > characterWrapLimit;
        }

        /// <summary>
        /// A coroutine for fading in this tool tip. The time it takes to fade in is determined by
        /// <see cref="fadeInTime"/>.
        /// </summary>
        public IEnumerator FadeIn()
        {
            for (float alpha = 0.0f; alpha <= 1.0f; alpha += Time.deltaTime / fadeInTime)
            {
                Color color = contentField.color;
                contentField.color = new Color(color.r, color.g, color.b, alpha);

                color = backgroundImage.color;
                backgroundImage.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }
        }

        private void Update()
        {
            if (Application.isEditor)
            {
                int contentLength = contentField.text.Length;
                layoutElement.enabled = contentLength > characterWrapLimit ? true : false;
            }

            Vector2 mousePosition = Input.mousePosition;
            transform.position = mousePosition + mouseOffset;
        }
    }
}
