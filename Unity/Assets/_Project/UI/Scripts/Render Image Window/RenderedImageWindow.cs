using System.IO;
using TMPro;
using UnityEditor;
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
        private Image renderedImage;
        [SerializeField]
        private RectTransform imageBounds; // The rendered image is constrained to these bounds.
        [SerializeField]
        private TextMeshProUGUI loading;
        [SerializeField]
        private TextMeshProUGUI empty;
        [SerializeField]
        private UIZoomImage zoomImage;
        [SerializeField]
        private Button closeButton;
        [SerializeField]
        private Button saveImageButton;
        [SerializeField]
        private TextMeshProUGUI imageSavedText;
        [SerializeField]
        private GameObject progressBar;
        [SerializeField]
        private GameObject progressFill;
        [SerializeField]
        private TextMeshProUGUI taskProgress;

        private Texture2D texture;
        private Sprite sprite;

        /// <summary>
        /// Set the texture used by the image displayed in the window. Does not show the window, so in most cases
        /// <see cref="Show"/> should be called afterwards.
        /// </summary>
        /// <param name="texture"> The new texture for the displayed image. </param>
        public void SetImageTexture(Texture2D texture)
        {
            // Textures and sprites are not garbage collected. Provided we are the only users of these textures and
            // sprites destroying them here is fine.
            if (this.texture != null)
                Destroy(this.texture);
            if (sprite != null)
                Destroy(sprite);
            
            // hide loading message, show image and reset zoom. This order is important the other way around it breaks
            loading.gameObject.SetActive(false);
            progressBar.gameObject.SetActive(false);
            saveImageButton.gameObject.SetActive(true);
            renderedImage.gameObject.SetActive(true);
            zoomImage.ResetZoom();
            
            this.texture = texture;

            Rect rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            // We expand the image until we hit the bounds in either the width or height.
            float pixelsPerUnitInWidth = imageBounds.rect.width / texture.width;
            float pixelsPerUnitInHeight = imageBounds.rect.height / texture.height;
            float pixelsPerUnit = Mathf.Min(pixelsPerUnitInWidth, pixelsPerUnitInHeight);

            sprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);
            renderedImage.sprite = sprite;
            
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
            if (texture != null)
                Destroy(texture);
            if (sprite != null)
                Destroy(sprite);
            
            // hide image, empty and show loading message
            empty.gameObject.SetActive(false);
            renderedImage.gameObject.SetActive(false);
            loading.gameObject.SetActive(true);
            progressFill.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
            taskProgress.text = "0%";
            progressBar.SetActive(true);
            saveImageButton.gameObject.SetActive(false);
            imageSavedText.gameObject.SetActive(false);
        }

        public void UpdateProgressBar(int percentage)
        {
            progressFill.GetComponent<RectTransform>().sizeDelta = 
                new Vector2(progressBar.GetComponent<RectTransform>().rect.width / 100 * percentage, 0);
            taskProgress.text = percentage.ToString() + "%";
        }

        /// <summary>
        /// Show the rendered image window.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            UIManager.Get().EnableBlocker();
            UIManager.Get().AddEscapable(closeButton.onClick.Invoke);
        }

        /// <summary>
        /// Hide the rendered image window.
        /// </summary>
        public void Hide()
        {
            if (loading.gameObject.activeSelf)
            {
                loading.gameObject.SetActive(false);
                progressBar.gameObject.SetActive(false);
                empty.gameObject.SetActive(true);
            }

            UIManager.Get().DisableBlocker();
            UIManager.Get().RemoveEscapable(closeButton.onClick.Invoke);
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

        public void SaveImage()
        {
#if UNITY_WEBGL
            saveImageButton.gameObject.SetActive(false);
            imageSavedText.text = "This functionality is not (yet) available for the web version.";
            imageSavedText.gameObject.SetActive(true);
#else
            string path = Application.dataPath + "/SavedImages/";
            string fileName = "Render " + System.DateTime.Now.ToString().Replace(":", "-") + ".png";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllBytes(path + fileName, renderedImage.sprite.texture.EncodeToPNG());

            saveImageButton.gameObject.SetActive(false);
            imageSavedText.text = "Image has been saved to " + path +  fileName;
            imageSavedText.gameObject.SetActive(true);
#endif
        }

        private void Awake()
        {
            loading.gameObject.SetActive(false);
            renderedImage.gameObject.SetActive(false);
            progressBar.gameObject.SetActive(false);
            saveImageButton.gameObject.SetActive(false);
            imageSavedText.gameObject.SetActive(false);
        }
    }
}
