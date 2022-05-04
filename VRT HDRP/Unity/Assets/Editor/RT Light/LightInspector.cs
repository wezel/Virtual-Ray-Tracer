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
            //var p = serializedObject.GetIterator();

            DrawPropertiesExcluding(serializedObject, "m_Color");
        }
    }
}