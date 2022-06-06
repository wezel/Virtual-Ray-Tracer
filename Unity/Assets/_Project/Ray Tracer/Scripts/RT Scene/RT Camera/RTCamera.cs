using System;
using UnityEngine;
using UnityEngine.Events;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera
{
    /// <summary>
    /// Represents the camera used by the ray tracer. On <see cref="Start"/> it will instantiate a visual
    /// representation of the camera and the projected screen.
    /// </summary>
    [RequireComponent(typeof(CameraCollisionMesh))]
    public class RTCamera : MonoBehaviour
    {
        public delegate void CameraChanged();
        /// <summary>
        /// An event invoked whenever a property of this camera is changed.
        /// </summary>
        public event CameraChanged OnCameraChanged;

        /// <summary>
        /// An event invoked whenever a mesh is selected.
        /// </summary>
        [Serializable]
        public class CameraSelected : UnityEvent { }
        public CameraSelected OnCameraSelected;

        [SerializeField]
        private Color defaultColor;
        private Color color;
        /// <summary>
        /// The color of the lines making up the camera's frustum and screen visuals.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                frustumLine1.startColor = color;
                frustumLine1.endColor = color;
                frustumLine2.startColor = color;
                frustumLine2.endColor = color;
                frustumLine3.startColor = color;
                frustumLine3.endColor = color;
                frustumLine4.startColor = color;
                frustumLine4.endColor = color;
                Screen.Color = color;
            }
        }

        /// <summary>
        /// The position of the camera.
        /// </summary>
        public Vector3 Position
        {
            get { return transform.position; }
            set 
            {
                if (value == transform.position) return;
                transform.position = value; 
            }
        }

        /// <summary>
        /// The rotation of the camera.
        /// </summary>
        public Vector3 Rotation
        {
            get { return transform.eulerAngles; }
            set
            {
                if (value == transform.eulerAngles) return;
                transform.eulerAngles = value;
            }
        }

        [SerializeField, Range(0.0f, 180.0f)]
        private float fieldOfView = 45.0f;
        /// <summary>
        /// The vertical field of view of the camera. When set, the camera's frustum and screen visuals will be
        /// recalculated.
        /// </summary>
        public float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                if (value == fieldOfView) return;
                fieldOfView = value;
                Recalculate();
                OnCameraChanged?.Invoke();
            }
        }

        [SerializeField, Range(1, 16)]
        private int screenWidth = 8;
        /// <summary>
        /// The width of the camera's screen in pixels. When set, the camera's frustum and screen visuals will be
        /// recalculated.
        /// </summary>
        public int ScreenWidth
        {
            get { return screenWidth; }
            set 
            {
                if (value == screenWidth) return;
                screenWidth = value;
                Recalculate();
                OnCameraChanged?.Invoke();
            }
        }

        [SerializeField, Range(1, 16)]
        private int screenHeight = 4;
        /// <summary>
        /// The height of the camera's screen in pixels. When set, the camera's frustum and screen visuals will be
        /// recalculated.
        /// </summary>
        public int ScreenHeight
        {
            get { return screenHeight; }
            set
            {
                if (value == screenHeight) return;
                screenHeight = value;
                Recalculate();
                OnCameraChanged?.Invoke();
            }
        }

        [SerializeField, Range(0.0f, 10.0f)]
        private float screenDistance = 1.0f;
        /// <summary>
        /// The distance between the camera's origin and screen in units. This property is used by the ray tracer to
        /// clip objects close to the camera. When set, the camera's frustum and screen visuals will be recalculated.
        /// </summary>
        public float ScreenDistance
        {
            get { return screenDistance; }
            set 
            {
                if (value == screenDistance) return;
                screenDistance = value;
                Recalculate();
                OnCameraChanged?.Invoke();
            }
        }

        /// <summary>
        /// The screen attached to this camera.
        /// </summary>
        public RTScreen Screen { get; private set; }

        private float aspectRatio;
        private float halfScreenHeight;
    
        private LineRenderer frustumLine1;
        private LineRenderer frustumLine2;
        private LineRenderer frustumLine3;
        private LineRenderer frustumLine4;
        private CameraCollisionMesh collisionMesh;

        /// <summary>
        /// Reset the color of the lines making up the camera's frustum and screen visuals to its default value. Used
        /// to reset the camera after highlighting it as selected.
        /// </summary>
        public void ResetColor()
        {
            Color = defaultColor;
        }

        private void Start()
        {
            frustumLine1 = transform.Find("Frustum Line 1").GetComponent<LineRenderer>();
            frustumLine2 = transform.Find("Frustum Line 2").GetComponent<LineRenderer>();
            frustumLine3 = transform.Find("Frustum Line 3").GetComponent<LineRenderer>();
            frustumLine4 = transform.Find("Frustum Line 4").GetComponent<LineRenderer>();
            Screen = transform.Find("Virtual Screen").GetComponent<RTScreen>();
            collisionMesh = GetComponent<CameraCollisionMesh>();

            Recalculate();
            ResetColor();
        }

        private void FixedUpdate()
        {
            if (transform.hasChanged) OnCameraChanged?.Invoke();
        }

        private void Update()
        {
            // In update we only move around existing lines to match the camera's transform.
            RecalculateFrustum();
            Screen.RecalculateLines();
            transform.hasChanged = false;   // Do this in Update to let other scripts also check
        }

        /// <summary>
        /// Recalculate the camera's frustum and screen. This function is called whenever the camera's field of view or
        /// screen dimensions change.
        /// </summary>
        private void Recalculate()
        {
            // Recalculate variables used to calculate the frustum and screen.
            aspectRatio = (float)ScreenWidth / (float)ScreenHeight;
            halfScreenHeight = Mathf.Tan(Mathf.Deg2Rad * FieldOfView / 2.0f) * ScreenDistance;

            // Recalculate frustum and screen.
            RecalculateFrustum();
            RecalculateScreen();
        }

        /// <summary>
        /// Recalculate the begin and end points of the frustum lines based on the camera's properties. Should be called
        /// whenever the camera's properties change, e.g. when its field of view or screen dimensions are changed.
        /// </summary>
        private void RecalculateFrustum()
        {
            // The frustum lines start at the camera's origin.
            frustumLine1.SetPosition(0, transform.position);
            frustumLine2.SetPosition(0, transform.position);
            frustumLine3.SetPosition(0, transform.position);
            frustumLine4.SetPosition(0, transform.position);

            // The frustum lines end at the corners of the screen.
            Vector3 frustumLine1End = new Vector3(-aspectRatio * halfScreenHeight, -halfScreenHeight, ScreenDistance);
            Vector3 frustumLine2End = new Vector3(-aspectRatio * halfScreenHeight,  halfScreenHeight, ScreenDistance);
            Vector3 frustumLine3End = new Vector3( aspectRatio * halfScreenHeight,  halfScreenHeight, ScreenDistance);
            Vector3 frustumLine4End = new Vector3( aspectRatio * halfScreenHeight, -halfScreenHeight, ScreenDistance);

            // Transform the lines to match the camera's position and rotation.
            frustumLine1End = transform.rotation * frustumLine1End;
            frustumLine1End = transform.position + frustumLine1End;
            frustumLine2End = transform.rotation * frustumLine2End;
            frustumLine2End = transform.position + frustumLine2End;
            frustumLine3End = transform.rotation * frustumLine3End;
            frustumLine3End = transform.position + frustumLine3End;
            frustumLine4End = transform.rotation * frustumLine4End;
            frustumLine4End = transform.position + frustumLine4End;

            // Update the collision mesh with the new frustum dimensions.
            collisionMesh.RecalculateMesh(transform.position, frustumLine1End, frustumLine2End, frustumLine3End,
                                          frustumLine4End);

            frustumLine1.SetPosition(1, frustumLine1End);
            frustumLine2.SetPosition(1, frustumLine2End);
            frustumLine3.SetPosition(1, frustumLine3End);
            frustumLine4.SetPosition(1, frustumLine4End);
        }

        /// <summary>
        /// Recalculate the grid of lines that represents the screen. Should be called whenever the camera's properties
        /// change, e.g. when its field of view or screen dimensions are changed.
        /// </summary>
        private void RecalculateScreen()
        {
            Screen.transform.localPosition = new Vector3(0.0f, 0.0f, ScreenDistance);
            Screen.SetDimensions(ScreenWidth, ScreenHeight, 2.0f * halfScreenHeight / ScreenHeight);
        }
    }
}
