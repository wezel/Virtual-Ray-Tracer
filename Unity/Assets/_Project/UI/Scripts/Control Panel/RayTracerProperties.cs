using System.Collections;
using _Project.Ray_Tracer.Scripts;
using _Project.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that provides access to the properties of the current <see cref="UnityRayTracer"/> and
    /// <see cref="RayManager"/>. Any changes made to the shown properties will be applied to the ray tracer and ray
    /// manager.
    /// </summary>
    public class RayTracerProperties : MonoBehaviour
    {
        private UnityRayTracer rayTracer;
        private RayManager rayManager;
        private UIManager uiManager;
        private RTSceneManager rtSceneManager;

        [SerializeField]
        private BoolEdit renderShadowsEdit;
        [SerializeField]
        private FloatEdit recursionDepthEdit;
        [SerializeField]
        private ColorEdit backgroundColorEdit;

        [SerializeField]
        private BoolEdit showRaysEdit;
        [SerializeField]
        private BoolEdit hideNoHitRaysEdit;
        [SerializeField]
        private BoolEdit hideNegligibleRaysEdit;
        [SerializeField]
        private FloatEdit rayHideThresholdEdit;
        [SerializeField]
        private BoolEdit rayTransparencyEnabled;
        [SerializeField]
        private BoolEdit rayDynamicRadiusEnabled;
        [SerializeField]
        private BoolEdit rayColorContributionEnabled;
        [SerializeField]
        private FloatEdit rayTransThresholdEdit;
        [SerializeField]
        private FloatEdit rayTransExponentEdit;
        [SerializeField]
        private FloatEdit rayRadiusEdit;
        [SerializeField]
        private FloatEdit rayMinRadiusEdit;
        [SerializeField]
        private FloatEdit rayMaxRadiusEdit;

        [SerializeField]
        private BoolEdit animateEdit;
        [SerializeField]
        private BoolEdit animateSequentiallyEdit;
        [SerializeField]
        private BoolEdit loopEdit;
        [SerializeField]
        private FloatEdit speedEdit;

        [SerializeField]
        private FloatEdit superSamplingFactorEdit;
        [SerializeField]
        private BoolEdit superSamplingVisualEdit;
        [SerializeField]
        private BoolEdit enablePointLightsEdit;
        [SerializeField]
        private BoolEdit enableSpotLightsEdit;
        [SerializeField]
        private BoolEdit enableAreaLightsEdit;
        [SerializeField]
        private Button renderImageButton;
        [SerializeField]
        private Button openImageButton;
        [SerializeField]
        private Button flyRoRTCameraButton;


        /// <summary>
        /// Show the ray tracer properties for the current <see cref="UnityRayTracer"/> and <see cref="RayManager"/>.
        /// These properties can be changed via the shown UI.
        /// </summary>
        public void Show()
        {            
            gameObject.SetActive(true);
            rayTracer = UnityRayTracer.Get();
            rayManager = RayManager.Get();
            uiManager = UIManager.Get();
            rtSceneManager = RTSceneManager.Get();

            renderShadowsEdit.IsOn = rayTracer.RenderShadows;
            enablePointLightsEdit.IsOn = rtSceneManager.Scene.EnablePointLights;
            enableSpotLightsEdit.IsOn = rtSceneManager.Scene.EnableSpotLights;
            enableAreaLightsEdit.IsOn = rtSceneManager.Scene.EnableAreaLights;
            recursionDepthEdit.Value = rayTracer.MaxDepth;
            backgroundColorEdit.Color = rayTracer.BackgroundColor;

            showRaysEdit.IsOn = rayManager.ShowRays;
            hideNoHitRaysEdit.IsOn = rayManager.HideNoHitRays;
            hideNegligibleRaysEdit.IsOn = rayManager.HideNegligibleRays;
            rayHideThresholdEdit.Value = rayManager.RayHideThreshold;
            rayTransparencyEnabled.IsOn = rayManager.RayTransparencyEnabled;
            rayDynamicRadiusEnabled.IsOn = rayManager.RayDynamicRadiusEnabled;
            rayColorContributionEnabled.IsOn = rayManager.RayColorContributionEnabled;
            rayTransExponentEdit.Value = rayManager.RayTransExponent;
            rayRadiusEdit.Value = rayManager.RayRadius;
            rayMinRadiusEdit.Value = rayManager.RayMinRadius;
            rayMaxRadiusEdit.Value = rayManager.RayMaxRadius;

            animateEdit.IsOn = rayManager.Animate;
            animateSequentiallyEdit.IsOn = rayManager.AnimateSequentially;
            loopEdit.IsOn = rayManager.Loop;
            speedEdit.Value = rayManager.Speed;

            superSamplingFactorEdit.Value = rayTracer.SuperSamplingFactor;
            superSamplingVisualEdit.IsOn = rayTracer.SuperSamplingVisual;
        }

        /// <summary>
        /// Hide the shown ray tracer properties.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private IEnumerator RunRenderImage()
        {
            yield return new WaitForFixedUpdate();
            yield return rayTracer.RenderImage();
            uiManager.RenderedImageWindow.SetImageTexture(rayTracer.Image);
            yield return null;
        }

        private void RenderImage()
        {
            uiManager.RenderedImageWindow.Show();
            uiManager.RenderedImageWindow.SetLoading();
            StartCoroutine(RunRenderImage());
        }

        private void ToggleImage()
        {
            uiManager.RenderedImageWindow.Toggle();
        }

        // TODO overhaul object order in levels and dependencies. It's becoming a bit difficult to get the right order 
        // TODO code wise. Objects should ideally set there own values on awake and do everything else on start.
        private void Start()
        {
            renderShadowsEdit.OnValueChanged.AddListener((value) => { RTSceneManager.Get().SetShadows(value); });
        }
        
        private void Awake()
        {
            renderShadowsEdit.OnValueChanged.AddListener((value) => { rayTracer.RenderShadows = value; });
            enablePointLightsEdit.OnValueChanged.AddListener((value) => { rtSceneManager.Scene.EnablePointLights = value; });
            enableSpotLightsEdit.OnValueChanged.AddListener((value) => { rtSceneManager.Scene.EnableSpotLights = value; });
            enableAreaLightsEdit.OnValueChanged.AddListener((value) => { rtSceneManager.Scene.EnableAreaLights = value; });
            recursionDepthEdit.OnValueChanged.AddListener((value) => { rayTracer.MaxDepth = (int)value; });
            backgroundColorEdit.OnValueChanged.AddListener((value) => { rayTracer.BackgroundColor = value; });

            hideNoHitRaysEdit.OnValueChanged.AddListener((value) => { rayManager.HideNoHitRays = value; });
            showRaysEdit.OnValueChanged.AddListener((value) => { rayManager.ShowRays = value; });
            hideNegligibleRaysEdit.OnValueChanged.AddListener((value) => { rayManager.HideNegligibleRays = value; });
            rayHideThresholdEdit.OnValueChanged.AddListener((value) => { rayManager.RayHideThreshold = value; });
            rayTransparencyEnabled.OnValueChanged.AddListener((value) => { rayManager.RayTransparencyEnabled = value; });
            rayDynamicRadiusEnabled.OnValueChanged.AddListener((value) => { rayManager.RayDynamicRadiusEnabled = value; });
            rayColorContributionEnabled.OnValueChanged.AddListener((value) => { rayManager.RayColorContributionEnabled = value; });
            rayTransExponentEdit.OnValueChanged.AddListener((value) => { rayManager.RayTransExponent = value; });
            rayRadiusEdit.OnValueChanged.AddListener((value) => { rayManager.RayRadius = value; });
            rayMinRadiusEdit.OnValueChanged.AddListener((value) => { rayManager.RayMinRadius = value; });
            rayMaxRadiusEdit.OnValueChanged.AddListener((value) => { rayManager.RayMaxRadius = value; });

            animateEdit.OnValueChanged.AddListener((value) => { rayManager.Animate = value; });
            animateSequentiallyEdit.OnValueChanged.AddListener((value) => { rayManager.AnimateSequentially = value; });
            loopEdit.OnValueChanged.AddListener((value) => { rayManager.Loop = value; });
            speedEdit.OnValueChanged.AddListener((value) => { rayManager.Speed = value; });

            superSamplingFactorEdit.OnValueChanged.AddListener((value) => { rayTracer.SuperSamplingFactor = (int)value; });
            superSamplingVisualEdit.OnValueChanged.AddListener((value) => { rayTracer.SuperSamplingVisual = value; });
            renderImageButton.onClick.AddListener(RenderImage);
            openImageButton.onClick.AddListener(ToggleImage);
            flyRoRTCameraButton.onClick.AddListener(() =>
            { 
                showRaysEdit.IsOn = false; // This invokes the OnValueChanged event as well.
                FindObjectOfType<CameraController>().FlyToRTCamera(); // There should only be 1 CamerController.
            });
        }
    }
}