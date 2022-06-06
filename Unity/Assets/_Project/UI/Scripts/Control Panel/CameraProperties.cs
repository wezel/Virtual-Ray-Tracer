using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that provides access to the properties of an <see cref="RTCamera"/>. Any changes made to the shown
    /// properties will be applied to the camera.
    /// </summary>
    public class CameraProperties : MonoBehaviour
    {
        private new RTCamera camera; // Hides Component.camera which is obsolete.

        [SerializeField]
        private Vector3Edit positionEdit;
        [SerializeField]
        private Vector3Edit rotationEdit;

        [SerializeField]
        private FloatEdit fieldOfViewEdit;
        [SerializeField]
        private FloatEdit screenWidthEdit;
        [SerializeField]
        private FloatEdit screenHeightEdit;
        [SerializeField]
        private FloatEdit screenDistanceEdit;
        [SerializeField]
        private FloatEdit screenOpacityEdit;

        [Serializable]
        public class ExternalChange : UnityEvent { };
        public ExternalChange OnExternalTranslationChange;

        /// <summary>
        /// Show the camera properties for <paramref name="camera"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="camera"> The <see cref="RTCamera"/> whose properties will be shown. </param>
        public void Show(RTCamera camera)
        {
            gameObject.SetActive(true);
            this.camera = camera;

            positionEdit.Value = camera.Position;
            rotationEdit.Value = camera.Rotation;

            fieldOfViewEdit.Value = camera.FieldOfView;
            screenWidthEdit.Value = camera.ScreenWidth;
            screenHeightEdit.Value = camera.ScreenHeight;
            screenDistanceEdit.Value = camera.ScreenDistance;
            screenOpacityEdit.Value = camera.Screen.ImageAlpha;
        }

        /// <summary>
        /// Hide the shown camera properties.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            camera = null;
        }

        private void Awake()
        {
            positionEdit.OnValueChanged.AddListener((value) => { camera.Position = value; });
            rotationEdit.OnValueChanged.AddListener((value) => { camera.Rotation = value; });

            fieldOfViewEdit.OnValueChanged.AddListener((value) => { camera.FieldOfView = value; });
            screenWidthEdit.OnValueChanged.AddListener((value) => { camera.ScreenWidth = (int)value; });
            screenHeightEdit.OnValueChanged.AddListener((value) => { camera.ScreenHeight = (int)value; });
            screenDistanceEdit.OnValueChanged.AddListener((value) => { camera.ScreenDistance = value; });
            screenOpacityEdit.OnValueChanged.AddListener((value) => { camera.Screen.ImageAlpha = value; });
        }

        private void FixedUpdate()
        {
            // Update the UI based on external changes to the camera transform (e.g. through the transformation gizmos).
            bool inUI = EventSystem.current.currentSelectedGameObject != null; // Only update if we are not in the UI.
            bool draggingEdit = positionEdit.IsDragging() || rotationEdit.IsDragging();
            if (camera != null && camera.transform.hasChanged && !inUI && !draggingEdit)
            {
                if (positionEdit.Value != camera.transform.position)
                {
                    positionEdit.Value = camera.transform.position;
                    OnExternalTranslationChange?.Invoke();
                }
                rotationEdit.Value = camera.transform.eulerAngles;
            }
        }

        private void Update()
        {
            camera.transform.hasChanged = false;   // Do this in Update to let other scripts also check
        }
    }
}
