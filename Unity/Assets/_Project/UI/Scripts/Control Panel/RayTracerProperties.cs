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
        private BoolEdit hideNoHitRaysEdit;
        [SerializeField]
        private BoolEdit showRaysEdit;
        [SerializeField]
        private FloatEdit rayRadiusEdit;

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

            hideNoHitRaysEdit.IsOn = rayManager.HideNoHitRays;
            showRaysEdit.IsOn = rayManager.ShowRays;
            rayRadiusEdit.Value = rayManager.RayRadius;

            animateEdit.IsOn = rayManager.Animate;
            animateSequentiallyEdit.IsOn = rayManager.AnimateSequentially;
            loopEdit.IsOn = rayManager.Loop;
            speedEdit.Value = rayManager.Speed;

            superSamplingFactorEdit.Value = rayTracer.SuperSamplingFactor;
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
            renderShadowsEdit.OnValueChanged.AddListener((value) => { RTSceneManager.Get().SetShadows(value); });
        }
        
        private void Awake()
        {
            renderShadowsEdit.OnValueChanged.AddListener((value) => { rayTracer.RenderShadows = value; });
            recursionDepthEdit.OnValueChanged.AddListener((value) => { rayTracer.MaxDepth = (int)value; });
            backgroundColorEdit.OnValueChanged.AddListener((value) => { rayTracer.BackgroundColor = value; });

            hideNoHitRaysEdit.OnValueChanged.AddListener( (value) => { rayManager.HideNoHitRays = value; });
            showRaysEdit.OnValueChanged.AddListener((value) => { rayManager.ShowRays = value; });
            rayRadiusEdit.OnValueChanged.AddListener((value) => { rayManager.RayRadius = value; });

            animateEdit.OnValueChanged.AddListener((value) => { rayManager.Animate = value; });
            animateSequentiallyEdit.OnValueChanged.AddListener((value) => { rayManager.AnimateSequentially = value; });
            loopEdit.OnValueChanged.AddListener((value) => { rayManager.Loop = value; });
            speedEdit.OnValueChanged.AddListener((value) => { rayManager.Speed = value; });

            superSamplingFactorEdit.OnValueChanged.AddListener((value) => { rayTracer.SuperSamplingFactor = (int)value; });
            renderImageButton.onClick.AddListener(RenderImage);
            openImageButton.onClick.AddListener(ToggleImage);
        }
    }
}
