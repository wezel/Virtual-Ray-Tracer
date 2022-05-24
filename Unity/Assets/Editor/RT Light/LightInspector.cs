using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.RT_Light
{
    [CustomEditor(typeof(Light))]
    public class LightInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, "m_Color", "m_Intensity", "m_SpotAngle", "m_InnerSpotAngle");
            serializedObject.ApplyModifiedProperties();
        }
    }
}