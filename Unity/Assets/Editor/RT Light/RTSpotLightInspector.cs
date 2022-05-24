using System;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Spot_Light;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.RT_Light
{
    [CustomEditor(typeof(RTSpotLight))]
    public class RTSpotLightInspector : UnityEditor.Editor
    {

        private RTSpotLight rtLight;
        private Light light;

        void OnEnable()
        {
            rtLight = (RTSpotLight)target;
            light = rtLight.gameObject.GetComponent<Light>();
            light.hideFlags = HideFlags.None;
        }

        private void OnPreSceneGUI()
        {
            rtLight.UpdateLightData();
        }
    }
}