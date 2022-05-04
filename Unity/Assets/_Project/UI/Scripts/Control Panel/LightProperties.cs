using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that provides access to the properties of an <see cref="RTLight"/>. Any changes made to the shown
    /// properties will be applied to the light.
    /// </summary>
    public class LightProperties : MonoBehaviour
    {
        private new RTLight light; // Hides Component.light which is obsolete.

        [SerializeField]
        private Vector3Edit positionEdit;
        [SerializeField]
        private ColorEdit colorEdit;
        [SerializeField]
        private FloatEdit ambientEdit;
        [SerializeField]
        private FloatEdit diffuseEdit;
        [SerializeField]
        private FloatEdit specularEdit;

        /// <summary>
        /// Show the light properties for <paramref name="light"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="light"> The <see cref="RTLight"/> whose properties will be shown. </param>
        public void Show(RTLight light)
        {
            gameObject.SetActive(true);
            this.light = light;

            positionEdit.Value = light.Position;
            colorEdit.Color = light.Color;
            ambientEdit.Value = light.Ambient;
            diffuseEdit.Value = light.Diffuse;
            specularEdit.Value = light.Specular;
        }

        /// <summary>
        /// Hide the shown light properties.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            light = null;
        }
        

        private void Awake()
        {
            positionEdit.OnValueChanged.AddListener(value => light.Position = value);
            colorEdit.OnValueChanged.AddListener(value => light.Color = value);
            ambientEdit.OnValueChanged += (value) => { light.Ambient = value; };
            diffuseEdit.OnValueChanged += (value) => { light.Diffuse = value; };
            specularEdit.OnValueChanged += (value) => { light.Specular = value; };
        }

        private void Update()
        {
            // Update the UI based on external changes to the light transform (e.g. through the transformation gizmos).
            bool inUI = EventSystem.current.currentSelectedGameObject != null; // Only update if we are not in the UI.
            bool draggingEdit = positionEdit.IsDragging();
            if (light != null && light.transform.hasChanged && !inUI && !draggingEdit)
            {
                positionEdit.Value = light.transform.position;
                light.transform.hasChanged = false;
            }
        }
    }
}
