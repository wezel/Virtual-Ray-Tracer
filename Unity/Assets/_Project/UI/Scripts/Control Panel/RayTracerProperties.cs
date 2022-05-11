using System.Collections;
using _Project.Ray_Tracer.Scripts;
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
        private Button renderImageButton;
        [SerializeField]
        private Button openImageButton;

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

            renderShadowsEdit.IsOn = rayTracer.RenderShadows;
            recursionDepthEdit.Value = rayTracer.MaxDepth;
            backgroundColorEdit.Color = rayTracer.BackgroundColor;

            showRaysEdit.IsOn = rayManager.ShowRays;
            hideNoHitRaysEdit.IsOn = rayManager.HideNoHitRays;
            hideNegligibleRaysEdit.IsOn = rayManager.HideNegligibleRays;
            rayHideThresholdEdit.Value = rayManager.RayHideThreshold;
            rayTransparencyEnabled.IsOn = rayManager.RayTransparencyEnabled;
            rayDynamicRadiusEnabled.IsOn = rayManager.RayDynamicRadiusEnabled;
            rayTransThresholdEdit.Value = rayManager.RayTransThreshold;
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
            Texture2D render = rayTracer.RenderImage();
            uiManager.RenderedImageWindow.SetImageTexture(render);
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
            renderShadowsEdit.OnValueChanged += (value) => { RTSceneManager.Get().SetShadows(value); };
        }
        
        private void Awake()
        {
            renderShadowsEdit.OnValueChanged += (value) => { rayTracer.RenderShadows = value; };
            recursionDepthEdit.OnValueChanged += (value) => { rayTracer.MaxDepth = (int)value; };
            backgroundColorEdit.OnValueChanged += (value) => { rayTracer.BackgroundColor = value; };

            hideNoHitRaysEdit.OnValueChanged += (value) => { rayManager.HideNoHitRays = value; };
            showRaysEdit.OnValueChanged += (value) => { rayManager.ShowRays = value; };
            hideNegligibleRaysEdit.OnValueChanged += (value) => { rayManager.HideNegligibleRays = value; };
            rayHideThresholdEdit.OnValueChanged += (value) => { rayManager.RayHideThreshold = value; };
            rayTransparencyEnabled.OnValueChanged += (value) => { rayManager.RayTransparencyEnabled = value; };
            rayDynamicRadiusEnabled.OnValueChanged += (value) => { rayManager.RayDynamicRadiusEnabled = value; };
            rayTransThresholdEdit.OnValueChanged += (value) => { rayManager.RayTransThreshold = value; };
            rayTransExponentEdit.OnValueChanged += (value) => { rayManager.RayTransExponent = value; };
            rayRadiusEdit.OnValueChanged += (value) => { rayManager.RayRadius = value; };
            rayMinRadiusEdit.OnValueChanged += (value) => { rayManager.RayMinRadius = value; };
            rayMaxRadiusEdit.OnValueChanged += (value) => { rayManager.RayMaxRadius = value; };

            animateEdit.OnValueChanged += (value) => { rayManager.Animate = value; };
            animateSequentiallyEdit.OnValueChanged += (value) => { rayManager.AnimateSequentially = value; };
            loopEdit.OnValueChanged += (value) => { rayManager.Loop = value; };
            speedEdit.OnValueChanged += (value) => { rayManager.Speed = value; };

            superSamplingFactorEdit.OnValueChanged += (value) => { rayTracer.SuperSamplingFactor = (int)value; };
            superSamplingVisualEdit.OnValueChanged += (value) => { rayTracer.SuperSamplingVisual = value; };
            renderImageButton.onClick.AddListener(RenderImage);
            openImageButton.onClick.AddListener(ToggleImage);
        }
    }
}
