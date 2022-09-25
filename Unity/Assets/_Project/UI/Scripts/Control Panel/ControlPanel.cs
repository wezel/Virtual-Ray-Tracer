using System;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using TMPro;
using UnityEngine;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that provides a basic panel for ray tracer, camera, light and mesh properties to be shown on.
    /// </summary>
    public class ControlPanel : MonoBehaviour
    {
        
        public enum SignalType
        {
            RayTracer,
            Camera,
            Object,
            Visual
        }

        public void Subscribe(Action<SignalType> function)
        {
            rayTracerButton.onClick.AddListener(() => function(SignalType.RayTracer));
            cameraButton.onClick.AddListener(() => function(SignalType.Camera));
            objectButton.onClick.AddListener(() => function(SignalType.Object));
            visualButton.onClick.AddListener(() => function(SignalType.Visual));
        }
        
        [SerializeField]
        private RayTracerProperties rayTracerProperties;
        [SerializeField]
        private CameraProperties cameraProperties;
        [SerializeField]
        private LightProperties lightProperties;
        [SerializeField]
        private MeshProperties meshProperties;
        [SerializeField]
        private VisualizationProperties visualizationProperties;
        [SerializeField]
        private TextMeshProUGUI emptyProperties;
        [SerializeField]
        private FolderButton rayTracerButton;
        [SerializeField]
        private FolderButton cameraButton;
        [SerializeField]
        private FolderButton objectButton;
        [SerializeField]
        private FolderButton visualButton;

        /// <summary>
        /// Show the control panel. By default this will not show any properties, just the panel background.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Show the ray tracer properties for the current <see cref="UnityRayTracer"/> and
        /// <see cref="RayManager"/>. These properties can be changed via the shown UI.
        /// </summary>
        public void ShowRayTracerProperties()
        {
            cameraProperties.Hide();
            lightProperties.Hide();
            meshProperties.Hide();
            visualizationProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            cameraButton.Conceal();
            objectButton.Conceal();
            visualButton.Conceal();

            Show();
            rayTracerButton.Highlight();
            rayTracerProperties.Show();
        }

        /// <summary>
        /// Show the camera properties for <paramref name="camera"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="camera"> The <see cref="RTCamera"/> whose properties will be shown. </param>
        public void ShowCameraProperties(RTCamera camera)
        {
            rayTracerProperties.Hide();
            lightProperties.Hide();
            meshProperties.Hide();
            visualizationProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            objectButton.Conceal();
            rayTracerButton.Conceal();
            visualButton.Conceal();

            Show();
            cameraButton.Highlight();
            cameraProperties.Show(camera);
        }

        /// <summary>
        /// Show the light properties for <paramref name="light"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="light"> The <see cref="RTLight"/> whose properties will be shown. </param>
        public void ShowLightProperties(RTLight light)
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            meshProperties.Hide();
            visualizationProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            rayTracerButton.Conceal();
            cameraButton.Conceal();
            visualButton.Conceal();

            Show();
            objectButton.Highlight();
            lightProperties.Show(light);
        }

        /// <summary>
        /// Show the mesh properties for <paramref name="mesh"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="mesh"> The <see cref="RTMesh"/> whose properties will be shown. </param>
        public void ShowMeshProperties(RTMesh mesh)
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            lightProperties.Hide();
            visualizationProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            cameraButton.Conceal();
            rayTracerButton.Conceal();
            visualButton.Conceal();

            Show();
            objectButton.Highlight();
            meshProperties.Show(mesh);
        }

        public void ShowVisualizationProperties()
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            lightProperties.Hide();
            meshProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            cameraButton.Conceal();
            rayTracerButton.Conceal();
            objectButton.Conceal();

            Show();
            visualButton.Highlight();
            visualizationProperties.Show();
        }

        public void ShowEmptyProperties()
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            lightProperties.Hide();
            visualizationProperties.Hide();
            cameraButton.Conceal();
            rayTracerButton.Conceal();
            visualButton.Conceal();

            Show();
            objectButton.Highlight();
            emptyProperties.gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the control panel and any properties shown on it.
        /// </summary>
        public void Hide()
        {
            rayTracerProperties.Hide();
            cameraProperties.Hide();
            lightProperties.Hide();
            meshProperties.Hide();
            visualizationProperties.Hide();
            emptyProperties.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void Start()
        {
            ShowRayTracerProperties();
            visualizationProperties = VisualizationProperties.Instance;
        }
    }
}
