using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.RT_Point_Light
{
    /// <summary>
    /// Represents a light in the ray tracer scene. Requires that the attached game object has a 
    /// <see cref="UnityEngine.Light"/> component. Should be considered something like a tag to indicate to the scene
    /// manager that this light should be sent to the ray tracer. All actual information for the ray tracer is stored
    /// in the transform and light components.
    ///
    /// Because of changes made to the render engine the light color "does not" represent the actual color in the scene.
    /// The color of the light symbol in the editor can be ignored and for accurate light changes the light should
    /// be changed with the color settings in the RT Light tab instead.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    public class RTPointLight : RTLight
    {
        public override Color Color
        {
            get => color;
            set
            {
                Color lightData = light.color;
                lightData.r = Mathf.Floor(value.r * 256) + value.g / 2;
                lightData.g = lightData.g % 1 + Mathf.Floor(value.b * 256);
                light.color = lightData;

                base.Color = value;
            }
        }

        public override float Intensity
        {
            get => intensity;
            set
            {
                // Besides dividing by 2, also divide by 20, as the range is 0 - 20.
                Color lightData = light.color;
                lightData.g = Mathf.Floor(lightData.g) + value / 40;
                light.color = lightData;
                base.Intensity = value;
            }
        }

        public override float Ambient
        {
            get => ambient;
            set
            {                
                Color lightData = light.color;
                lightData.b = lightData.b % 1 + Mathf.Floor(value * 256);
                light.color = lightData;

                base.Ambient = value;
            }
        }

        public override float Diffuse
        {
            get => diffuse;
            set
            {
                Color lightData = light.color;
                lightData.b = Mathf.Floor(lightData.b) + value / 2;
                light.color = lightData;

                base.Diffuse = value;
            }
        }

        public override float Specular
        {
            get => specular;
            set
            {
                Color lightData = light.color;
                lightData.a = specular;
                light.color = lightData;

                base.Specular = value;
            }
        }
        
        /// <summary>
        /// The underlying <see cref="UnityEngine.Light"/> used by the light.
        /// </summary>
        [SerializeField]
        private new Light light;
        
        public override LightShadows Shadows { get => light.shadows; set => light.shadows = value; }

//        public override void ChangeLightType(RTLightType type)
//        {
//            if (Type == type) return;

//#if UNITY_EDITOR
//            DestroyImmediate(light.gameObject);
//#else
//            Destroy(light.gameObject);
//#endif
//            light = null;

//            base.ChangeLightType(type);
//        }

        protected override void Awake()
        {
            Type = RTLightType.Point;
            base.Awake();
        }

        private void LateUpdate()
        {
            #if UNITY_EDITOR
                if(!Application.isPlaying) return;
            #endif
            // Make the label face the camera. We do this in LateUpdate to make sure the camera has finished its moving.
            // From: https://answers.unity.com/questions/52656/how-i-can-create-an-sprite-that-always-look-at-the.html
            canvas.transform.forward = Camera.main.transform.forward;
        }

#if UNITY_EDITOR

        private void OnRenderObject()
        {
            // Fix maximize window errors
            if (UnityEditor.SceneView.lastActiveSceneView == null) 
                return;
            canvas.transform.forward = UnityEditor.SceneView.lastActiveSceneView.camera.transform.forward;
        }
#endif
        
    }
}
