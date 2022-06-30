using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light
{
    /// <summary>
    /// Base class of the RTLights
    /// </summary>
    [ExecuteAlways]
    public abstract class RTLight : MonoBehaviour
    {
        /// <summary>
        /// This function encodes the color data. By encoding the color data we have extra room to send other data to
        /// the graphic renderer.
        /// </summary>
        [SerializeField]
        protected Color color;
        public virtual Color Color
        {
            get => color;
            set
            {
                value.a = 1;
                if (value == color) return;
                color = value;
                label.color = value;
                UpdateLightData();
                OnLightChanged?.Invoke();
                OnLightColorChanged?.Invoke();
            }
        }

        // Important! The range is also hardcoded in the LightShader!
        protected const int intensityDivisor = 60;  // 2 * max intensity
        [SerializeField, Range(0.0f, 30.0f)]
        protected float intensity;
        public float Intensity
        {
            get => intensity;
            set
            {
                if (value == intensity) return;
                intensity = value;
                UpdateLightData();
                OnLightChanged?.Invoke();
                OnIntensityChanged?.Invoke();
            }
        }

        [SerializeField, Range(0,1)]
        protected float ambient;
        public float Ambient
        {
            get => ambient;
            set
            {
                if (value == ambient) return;
                ambient = value;
                UpdateLightData();
                OnLightChanged?.Invoke();
            }
        }

        [SerializeField, Range(0,1)]
        protected float diffuse;

        public float Diffuse
        {
            get => diffuse;
            set
            {
                if (value == diffuse) return;
                diffuse = value;
                UpdateLightData();
                OnLightChanged?.Invoke();
            }
        }

        [SerializeField, Range(0,1)]
        protected float specular;

        public float Specular
        {
            get => specular;
            set
            {
                if (value == specular) return;
                specular = value;
                UpdateLightData();
                OnLightChanged?.Invoke();
            }
        }

        public virtual float SpotAngle { get; set; } = 90;

        [SerializeField]
        protected bool lightDistanceAttenuation = true;
        public bool LightDistanceAttenuation
        {
            get => lightDistanceAttenuation;
            set
            {
                if (value == lightDistanceAttenuation) return;
                lightDistanceAttenuation = value;
                UpdateLightData();
                OnLightChanged?.Invoke();
                OnDistAttenuationChanged?.Invoke();
            }
        }

        [Serializable]
        public class LightChanged : UnityEvent { };
        /// <summary>
        /// An event invoked whenever a this light is changed.
        /// </summary>
        public LightChanged OnLightSelected, OnLightChanged, OnLightColorChanged, OnDistAttenuationChanged, OnIntensityChanged;

        protected void OnLightChangedInvoke() => OnLightChanged?.Invoke();

        /// <summary>
        /// The position of the light.
        /// </summary>
        public Vector3 Position
        {
            get { return transform.position; }
            set
            {
                if (value == transform.position) return;
                transform.position = value;
                OnLightChanged?.Invoke();
            }
        }


        /// <summary>
        /// The rotation of the light.
        /// </summary>
        public Vector3 Rotation
        {
            get => transform.eulerAngles;
            set
            {
                if (value == transform.eulerAngles) return;
                transform.eulerAngles = value;
                OnLightChanged?.Invoke();
            }
        }

        /// <summary>
        /// The scale of the light.
        /// </summary>
        public Vector3 Scale
        {
            get => transform.localScale;
            set
            {
                if (value == transform.localScale) return;
                transform.localScale = value;
                OnLightChanged?.Invoke();
            }
        }

        public enum RTLightType
        {
            Point,
            Spot,
            Area
        }

        /// <summary>
        /// Whether the light is a point or an area.
        /// </summary>
        [HideInInspector]
        public RTLightType Type;

        [SerializeField]
        protected Image label;
        
        [SerializeField]
        protected Image outline;

        [SerializeField]
        protected Canvas canvas;

        protected Color defaultOutline;

        public virtual void UpdateLightData() { }

        public void Higlight(Color value) => outline.color = value;

        public void ResetHighlight() => outline.color = defaultOutline;

        public virtual LightShadows Shadows { get; set; }

        public virtual int LightSamples { get; set; } = 4;

        public virtual float SpotAttenuationPower { get; set; } = 1f;

        protected void FixedUpdate()
        {
            if (transform.hasChanged) OnLightChanged?.Invoke();
        }

        protected void Update()
        {
            transform.hasChanged = false;   // Do this in Update to let other scripts also check
        }

        protected virtual void Awake()
        {
            defaultOutline = outline.color;
        }
#if UNITY_EDITOR
        private void OnEnable()
        {
            label.color = color;
        }
#endif
    }
}
