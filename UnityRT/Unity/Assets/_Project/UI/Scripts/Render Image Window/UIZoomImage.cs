using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.UI.Scripts.Render_Image_Window
{
    /// <summary>
    /// A UI class that handles zooming and panning for an image. Adapted from
    /// https://gist.github.com/unity3dcollege/f971cee86b6eb09ad4dafc49050f693c.
    /// </summary>
    public class UIZoomImage : MonoBehaviour, IScrollHandler
    {
        private Vector3 initialScale;

        [SerializeField]
        private float zoomSpeed = 0.1f;
        [SerializeField]
        private float maxZoom = 10f;

        private void Awake()
        {
            initialScale = transform.localScale;
        }

        public void OnScroll(PointerEventData eventData)
        {
            var delta = Vector3.one * (eventData.scrollDelta.y * zoomSpeed);
            var desiredScale = transform.localScale + delta;

            desiredScale = ClampDesiredScale(desiredScale);

            // Zoom where the cursor is.
            var move = ((Vector3) eventData.position - transform.position) *
                       ((desiredScale - transform.localScale).magnitude / transform.localScale.magnitude);
            transform.position = eventData.scrollDelta.y > 0 ? transform.position - move : transform.position + move;
            
            transform.localScale = desiredScale;
            
        }

        public void ResetZoom()
        {
            transform.localScale = initialScale;
        }

        private Vector3 ClampDesiredScale(Vector3 desiredScale)
        {
            desiredScale = Vector3.Max(initialScale, desiredScale);
            desiredScale = Vector3.Min(initialScale * maxZoom, desiredScale);
            return desiredScale;
        }
    }
}
