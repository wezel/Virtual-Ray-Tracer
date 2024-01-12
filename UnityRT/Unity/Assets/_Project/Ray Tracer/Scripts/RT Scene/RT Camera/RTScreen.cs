using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera
{
    /// <summary>
    /// Represents the screen of the camera used by the ray tracer. On <see cref="Awake"/> it will instantiate a visual
    /// representation of the screen.
    /// </summary>
    public class RTScreen : MonoBehaviour
    {
        /// <summary>
        /// The width of the screen in pixels.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the screen in pixels.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The size of a single pixel in units. Pixels are always square.
        /// </summary>
        public float Size { get; private set; }

        private Color color;
        /// <summary>
        /// The color of the lines used to draw the screen.
        /// </summary>
        public Color Color 
        { 
            get { return color; }
            set
            {
                color = value;
                foreach (var line in lines)
                {
                    line.startColor = color;
                    line.endColor = color;
                }
            }
        }

        [SerializeField] 
        private LineRenderer linePrefab;
        private List<LineRenderer> lines = new List<LineRenderer>();

        private RTImage rayTracerImage;

        [SerializeField]
        private SpriteRenderer projectedImage;

        [SerializeField, Range(0.0f, 1.0f)]
        private float imageAlpha;
        /// <summary>
        /// The alpha component of the image projected on the screen.
        /// </summary>
        public float ImageAlpha
        {
            get { return imageAlpha; }
            set
            {
                imageAlpha = value;
                UpdatePreview();
            }
        }


        private void Awake()
        {
            InstatiateLines();
        }

        private void Start()
        {
            rayTracerImage = RTSceneManager.Get().Image;
            rayTracerImage.OnImageChanged += UpdatePreview;
        }

        private void OnDestroy()
        {
            DestroyLines();
        }

        /// <summary>
        /// Set the dimensions of the screen. The lines making up the screen will be destroyed and reinstantiated. This is
        /// not very performant, use <see cref="RecalculateLines"/> if the screen's dimensions have not changed.
        /// </summary>
        /// <param name="width"> The new width of the screen in pixels. </param>
        /// <param name="height"> The new height of the screen in pixels. </param>
        /// <param name="size"> The size of the pixels. </param>
        public void SetDimensions(int width, int height, float size)
        {
            this.Width = width;
            this.Height = height;
            this.Size = size;

            InstatiateLines();
        }

        /// <summary>
        /// Recalculate the lines making up the screen. Moves already instantiated lines making it quite fast. Does not
        /// handle changes in the screen's width, height or size.
        /// </summary>
        public void RecalculateLines()
        {
            // Make sure the lines are properly instantiated.
            if (lines.Count != Width + Height + 2)
            {
                Debug.LogError("Cannot recalculate screen when lines are not properly instantiated!");
                return;
            }

            float halfWidth = (Width * Size) / 2.0f;
            float halfHeight = (Height * Size) / 2.0f;

            // Draw vertical grid lines.
            for (int x = 0; x <= Width; ++x)
            {
                Vector3 start = new Vector3(x * Size, 0.0f, 0.0f);
                Vector3 end = new Vector3(x * Size, Height * Size, 0.0f);

                // Transform the lines so that the center is at the virtual screen's origin.
                start -= new Vector3(halfWidth, halfHeight);
                end -= new Vector3(halfWidth, halfHeight);

                // Transform the lines to match the virtual screen's position and rotation.
                start = transform.rotation * start;
                start = transform.position + start;
                end = transform.rotation * end;
                end = transform.position + end;

                lines[x].SetPosition(0, start);
                lines[x].SetPosition(1, end);
            }

            // Draw horizontal grid lines.
            for (int y = 0; y <= Height; ++y)
            {
                Vector3 start = new Vector3(0.0f, y * Size, 0.0f);
                Vector3 end = new Vector3(Width * Size, y * Size, 0.0f);

                // Transform the lines so that the center of is at the virtual screen's origin.
                start -= new Vector3(halfWidth, halfHeight);
                end -= new Vector3(halfWidth, halfHeight);

                // Transform the lines to match the virtual screen's position and rotation.
                start = transform.rotation * start;
                start = transform.position + start;
                end = transform.rotation * end;
                end = transform.position + end;

                lines[y + Width + 1].SetPosition(0, start);
                lines[y + Width + 1].SetPosition(1, end);
            }
        }

        private void InstatiateLines()
        {
            // Destroy any existing lines.
            if (lines.Count > 0)
                DestroyLines();

            // Instantiate the new lines. There are width + 1 vertical and height + 1 horizontal lines.
            for (int i = 0; i != Width + Height + 2; ++i)
            {
                LineRenderer line = Instantiate(linePrefab, transform);
                line.startColor = Color;
                line.endColor = Color;
                lines.Add(line);
            }

            // Draw the lines.
            RecalculateLines();
        }

        private void DestroyLines()
        {
            foreach (var line in lines)
                Destroy(line.gameObject);

            lines.Clear();
        }

        private void UpdatePreview()
        {
            // Destroy the old sprite to prevent a memory leak.
            if (projectedImage.sprite != null)
                Destroy(projectedImage.sprite);

            projectedImage.sprite = rayTracerImage.GetSprite(1.0f / Size);
            projectedImage.color = new Color(1.0f, 1.0f, 1.0f, imageAlpha);
        }
    }
}
