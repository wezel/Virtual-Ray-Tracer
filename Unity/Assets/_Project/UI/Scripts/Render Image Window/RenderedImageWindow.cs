using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Render_Image_Window
{
    /// <summary>
    /// A simple UI window that displays a full resolution image produced by the ray tracer.
    /// </summary>
    public class RenderedImageWindow : MonoBehaviour
    {
        [SerializeField]
        private RawImage renderedImage;
        [SerializeField]
        private RectTransform imageBounds; // The rendered image is constrained to these bounds.
        [SerializeField]
        private TextMeshProUGUI loading;
        [SerializeField]
        private TextMeshProUGUI empty;
        [SerializeField]
        private UIZoomImage zoomImage;

        private RenderTexture texture;
        private Sprite sprite;

        /// <summary>
        /// Set the texture used by the image displayed in the window. Does not show the window, so in most cases
        /// <see cref="Show"/> should be called afterwards.
        /// </summary>
        /// <param name="texture"> The new texture for the displayed image. </param>
        public void SetImageTexture(RenderTexture texture)
        {
            // Textures and sprites are not garbage collected. Provided we are the only users of these textures and
            // sprites destroying them here is fine.
            if (sprite != null)
                Destroy(sprite);
            
            // hide loading message, show image and reset zoom. This order is important the other way around it breaks
            loading.gameObject.SetActive(false);
            renderedImage.gameObject.SetActive(true);
            zoomImage.ResetZoom();
            
            this.texture = texture;

            Rect rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            // We expand the image until we hit the bounds in either the width or height.
            float pixelsPerUnitInWidth = imageBounds.rect.width / texture.width;
            float pixelsPerUnitInHeight = imageBounds.rect.height / texture.height;
            float pixelsPerUnit = Mathf.Min(pixelsPerUnitInWidth, pixelsPerUnitInHeight);

            renderedImage.texture = texture;
            
            // Make sure the image UI element is scaled correctly.
            renderedImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                texture.width * pixelsPerUnit);
            renderedImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                texture.height * pixelsPerUnit);
        }

        public void SetLoading()
        {
            // Textures and sprites are not garbage collected. Provided we are the only users of these textures and
            // sprites destroying them here is fine.
            if (sprite != null)
                Destroy(sprite);
            
            // hide image, empty and show loading message
            empty.gameObject.SetActive(false);
            renderedImage.gameObject.SetActive(false);
            loading.gameObject.SetActive(true);
        }

        /// <summary>
        /// Show the rendered image window.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            UIManager.Get().EnableBlocker();
            UIManager.Get().AddEscapable(Hide);
        }

        /// <summary>
        /// Hide the rendered image window.
        /// </summary>
        public void Hide()
        {
            UIManager.Get().DisableBlocker();
            UIManager.Get().RemoveEscapable(Hide);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Toggle the visibility of the rendered image window. If the window is hidden it will now be shown and vice
        /// versa.
        /// </summary>
        public void Toggle()
        {
            if (gameObject.activeSelf) Hide();
            else Show();
        }

        private void Awake()
        {
            loading.gameObject.SetActive(false);
            renderedImage.gameObject.SetActive(false);
        }
    }
}
