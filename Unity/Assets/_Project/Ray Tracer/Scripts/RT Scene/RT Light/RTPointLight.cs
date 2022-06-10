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
        public override void UpdateLightData()
        {
            Color lightData;
            lightData.r = Mathf.Floor(color.r * 256) + color.g / 2;
            lightData.g = Mathf.Floor(color.b * 256) + (intensity / intensityDivisor);
            lightData.b = Mathf.Floor(ambient * 256) + diffuse / 2;
            lightData.a = Mathf.Floor(specular * 256) + (lightDistanceAttenuation ? 512 : 0);
            light.color = lightData;
        }

        /// <summary>
        /// The underlying <see cref="UnityEngine.Light"/> used by the light.
        /// </summary>
        [SerializeField]
        private new Light light;
        
        public override LightShadows Shadows { get => light.shadows; set => light.shadows = value; }

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
            canvas.transform.forward = (Camera.main.transform.position - Position).normalized;
        }

#if UNITY_EDITOR

        private void OnRenderObject()
        {
            // Fix maximize window errors
            if (UnityEditor.SceneView.lastActiveSceneView == null) 
                return;
            canvas.transform.forward = (UnityEditor.SceneView.lastActiveSceneView.camera.transform.position - Position).normalized;
        }
#endif
        
    }
}
