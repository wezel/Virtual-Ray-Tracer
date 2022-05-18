using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light
{
    /// <summary>
    /// Base class of the RTPointLight and RTAreaLight
    /// </summary>
    [ExecuteAlways]
    public class RTLight : MonoBehaviour
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
                color = value;
                label.color = value;                
                OnLightChanged?.Invoke();
            }
        }

        [SerializeField, Range(0,1)]
        protected float ambient;
        public virtual float Ambient
        {
            get => ambient;
            set
            {
                ambient = value;                
                OnLightChanged?.Invoke();
            }
        }

        [SerializeField, Range(0,1)]
        protected float diffuse;

        public virtual float Diffuse
        {
            get => diffuse;
            set
            {
                diffuse = value;
                OnLightChanged?.Invoke();
            }
        }

        [SerializeField, Range(0,1)]
        protected float specular;

        public virtual float Specular
        {
            get => specular;
            set
            {
                specular = value;
                OnLightChanged?.Invoke();
            }
        }
        
        public delegate void LightChanged();
        /// <summary>
        /// An event invoked whenever a property of this light is changed.
        /// </summary>
        public event LightChanged OnLightChanged;
        protected void OnLightChangedInvoke() => OnLightChanged?.Invoke();

        /// <summary>
        /// The position of the light.
        /// </summary>
        public Vector3 Position
        {
            get { return transform.position; }
            set
            {
                transform.position = value;
                OnLightChanged?.Invoke();
            }
        }
        
        [SerializeField]
        protected Image label;
        
        [SerializeField]
        protected Image outline;

        [SerializeField]
        protected Canvas canvas;

        protected Color defaultOutline;

        public void Higlight(Color value) => outline.color = value;

        public void ResetHighlight() => outline.color = defaultOutline;
        
        protected void Awake()
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
