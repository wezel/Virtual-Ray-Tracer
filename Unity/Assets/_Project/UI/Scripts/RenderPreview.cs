using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.Utility;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.UI.Scripts
{
    /// <summary>
    /// A UI class that displays an <see cref="RTImage"/>.
    /// </summary>
    public class RenderPreview : MonoBehaviour
    {
        private RTImage rayTracerImage;
        private RayManager rayManager;

        private int imageWidth;
        private int imageHeight;

        [SerializeField]
        private Image uiImage;

        [SerializeField]
        private RectTransform imageBounds; // The UI image is constrained to these bounds.
        [SerializeField]
        private Image hoverImage;
        [SerializeField]
        private Image selectImage;

        [SerializeField]
        private Button expandCollapseButton;
        [SerializeField]
        private Image expandCollapseImage;

        [SerializeField]
        private Sprite expandedIcon;
        [SerializeField]
        private Sprite collapsedIcon;

        [Serializable]
        public class Event : UnityEvent { };
        public Event OnPixelSelected, OnPixelDeselected;

        private RectTransform windowSize;
        private bool expanded = false;
        
        private float pixelsPerUnit;
        private Vector2Int hoveredPixel;
        private Vector2Int selectedPixel;

        public bool PreviewWindowHovered { get; set; }
        public bool ImageHovered { get; set; }

        private void UpdatePreview()
        {
            // We expand the image until we hit the bounds in either the width or height.
            float pixelsPerUnitInWidth = imageBounds.rect.width / rayTracerImage.Width;
            float pixelsPerUnitInHeight = imageBounds.rect.height / rayTracerImage.Height;
            pixelsPerUnit = Mathf.Min(pixelsPerUnitInWidth, pixelsPerUnitInHeight);

            // Destroy the old sprite to prevent a memory leak.
            if (uiImage.sprite != null)
                Destroy(uiImage.sprite);

            uiImage.sprite = rayTracerImage.GetSprite(pixelsPerUnit);

            // Make sure the image UI element is scaled correctly.
            uiImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 
                rayTracerImage.Width * pixelsPerUnit);
            uiImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 
                rayTracerImage.Height * pixelsPerUnit);

            // The hover and select images are the size of one pixel.
            hoverImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelsPerUnit);
            hoverImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelsPerUnit);
            selectImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixelsPerUnit);
            selectImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixelsPerUnit);
        }

        private void CheckDimensionsChanged()
        {
            // If the image dimensions have changed we need to invalidate our selected pixel as it may no longer exist.
            if (rayTracerImage.Width != imageWidth || rayTracerImage.Height != imageHeight)
            {
                selectImage.enabled = false;
                rayManager.DeselectRay();
            }

            imageWidth = rayTracerImage.Width;
            imageHeight = rayTracerImage.Height;
        }

        private void ExpandCollapse()
        {
            if (expanded)
            {
                expandCollapseImage.sprite = collapsedIcon;
                windowSize.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, windowSize.rect.width / 3);
                windowSize.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, windowSize.rect.height / 3);
                UIManager.Get().RemoveEscapable(ExpandCollapse);
            }
            else
            {
                expandCollapseImage.sprite = expandedIcon;
                windowSize.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, windowSize.rect.width * 3);
                windowSize.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, windowSize.rect.height * 3);
                UIManager.Get().AddEscapable(ExpandCollapse);
            }

            UpdatePreview();
            expanded = !expanded;

            // Recalculate the position of the selection indicator after expanding/collapsing the preview.
            if (selectImage.enabled)
            {
                float pixelCenterX = pixelsPerUnit * selectedPixel.x + pixelsPerUnit / 2.0f;
                float pixelCenterY = pixelsPerUnit * selectedPixel.y + pixelsPerUnit / 2.0f;
                float transformX = pixelCenterX - uiImage.rectTransform.rect.width / 2.0f;
                float transformY = pixelCenterY - uiImage.rectTransform.rect.height / 2.0f;
                selectImage.rectTransform.anchoredPosition = new Vector2(transformX, transformY);
            }
        }

        private void OnDisable()
        {
            if(expanded)
                ExpandCollapse();
        }

        private void Awake()
        {
            expandCollapseButton.onClick.AddListener(ExpandCollapse);
        }

        private void Start()
        {
            rayTracerImage = RTSceneManager.Get().Image;
            rayTracerImage.OnImageChanged += UpdatePreview;
            rayTracerImage.OnImageChanged += CheckDimensionsChanged;
            
            rayManager = RayManager.Get();

            windowSize = GetComponent<RectTransform>();

            hoverImage.enabled = false;
            selectImage.enabled = false;
        }

        private void Update()
        {
            if (!PreviewWindowHovered && Input.GetMouseButtonDown(0) && expanded) ExpandCollapse();

            if (ImageHovered)
            {
                // Get the mouse position with respect to the UI image's transform.
                Vector2 mouseScreen = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                Vector2 mouseLocal;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(uiImage.rectTransform, mouseScreen, null,
                    out mouseLocal);

                // The UI image is anchored in the center, but we want the coordinates anchored bottom left.
                mouseLocal.x += uiImage.rectTransform.rect.width / 2.0f;
                mouseLocal.y += uiImage.rectTransform.rect.height / 2.0f;

                // Determine the pixel that we are hovering over.
                int xCoordinate = Mathf.FloorToInt(mouseLocal.x / pixelsPerUnit);
                int yCoordinate = Mathf.FloorToInt(mouseLocal.y / pixelsPerUnit);
                hoveredPixel = new Vector2Int(xCoordinate, yCoordinate);

                // Snap the mouse position to the pixel center.
                float pixelCenterX = pixelsPerUnit * xCoordinate + pixelsPerUnit / 2.0f;
                float pixelCenterY = pixelsPerUnit * yCoordinate + pixelsPerUnit / 2.0f;
                Vector2 mouseSnapped = new Vector2(pixelCenterX, pixelCenterY);

                // The hover image is anchored in the center, so we convert back.
                mouseSnapped.x -= uiImage.rectTransform.rect.width / 2.0f;
                mouseSnapped.y -= uiImage.rectTransform.rect.height / 2.0f;
                hoverImage.rectTransform.anchoredPosition = mouseSnapped;

                hoverImage.enabled = true;

                // We can click to (de)select the hovered pixel.
                if (Input.GetMouseButtonDown(0))
                {
                    // If the hovered pixel is already selected we deselect it.
                    if (hoveredPixel == selectedPixel && selectImage.enabled)
                    {
                        selectImage.enabled = false;
                        rayManager.DeselectRay();
                        OnPixelDeselected?.Invoke();
                    }
                    // Select the hovered pixel.
                    else
                    {
                        selectedPixel = hoveredPixel;
                        selectImage.rectTransform.anchoredPosition = mouseSnapped;
                        selectImage.enabled = true;
                        rayManager.SelectRay(selectedPixel);
                        OnPixelSelected?.Invoke();
                    }
                }
            }
            else
            {
                hoverImage.enabled = false;
            }
        }
    }
}
