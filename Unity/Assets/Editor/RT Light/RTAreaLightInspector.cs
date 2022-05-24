using System;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Area_Light;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.RT_Light
{
    [CustomEditor(typeof(RTAreaLight))]
    public class RTAreaLightInspector : UnityEditor.Editor
    {

        private RTAreaLight rtAreaLight;
        private Light[] lights;

        void OnEnable()
        {
            rtAreaLight = (RTAreaLight)target;
            lights = rtAreaLight.gameObject.GetComponentsInChildren<Light>();
            foreach (Light light in lights)
                light.hideFlags = HideFlags.None;
        }

        private void OnPreSceneGUI()
        {
            rtAreaLight.UpdateLightData();
        }
    }
}