using System;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.RT_Light
{
    [CustomEditor(typeof(RTLight))]
    public class RTLightInspector : UnityEditor.Editor
    {

        private RTLight rtLight;
        private Light light;

        void OnEnable()
        {
            rtLight = (RTLight)target;
            light = rtLight.gameObject.GetComponent<Light>();
            light.hideFlags = HideFlags.None;
        }

        private void OnPreSceneGUI()
        {
            rtLight.Color = rtLight.Color;
            rtLight.Ambient = rtLight.Ambient;
            rtLight.Diffuse = rtLight.Diffuse;
            rtLight.Specular = rtLight.Specular;
        }
    }
}