using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Area_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Point_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using TMPro;
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
        private Vector3Edit rotationEdit;
        [SerializeField]
        private Vector3Edit scaleEdit;

        [SerializeField]
        private ColorEdit colorEdit;
        [SerializeField]
        private FloatEdit intensityEdit;
        [SerializeField]
        private FloatEdit ambientEdit;
        [SerializeField]
        private FloatEdit diffuseEdit;
        [SerializeField]
        private FloatEdit specularEdit;
        [SerializeField]
        private PowerEdit lightSamplesEdit;

        [SerializeField]
        private TMP_Dropdown typeDropdown;

        /// <summary>
        /// Show the light properties for <paramref name="light"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="light"> The <see cref="RTLight"/> whose properties will be shown. </param>
        public void Show(RTLight light)
        {
            gameObject.SetActive(true);
            this.light = light;

            positionEdit.Value = light.Position;
            if (light.Type == RTLight.RTLightType.Area)
            {
                rotationEdit.Value = ((RTAreaLight)light).Rotation;
                scaleEdit.Value = ((RTAreaLight)light).Scale;
                lightSamplesEdit.Value = Mathf.Pow(((RTAreaLight)light).LightSamples, 2);
            }

            typeDropdown.value = typeDropdown.options.FindIndex(option => option.text == light.Type.ToString());
            colorEdit.Color = light.Color;
            intensityEdit.Value = light.Intensity;
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

        private void ChangeObjectType(RTLight.RTLightType type)
        {
            rotationEdit.gameObject.SetActive(type == RTLight.RTLightType.Area);
            scaleEdit.gameObject.SetActive(type == RTLight.RTLightType.Area);
            //light.ChangeLightType(type);
            Show(light);
        }

        private void Awake()
        {
            positionEdit.OnValueChanged += value => { light.Position = value; };
            rotationEdit.OnValueChanged += value => { light.Rotation = value; };
            scaleEdit.OnValueChanged += value => { light.Scale = value; };
            
            colorEdit.OnValueChanged += value => { light.Color = value; };
            intensityEdit.OnValueChanged += value => { light.Intensity = value; };
            ambientEdit.OnValueChanged += (value) => { light.Ambient = value; };
            diffuseEdit.OnValueChanged += (value) => { light.Diffuse = value; };
            specularEdit.OnValueChanged += (value) => { light.Specular = value; };

            lightSamplesEdit.OnValueChanged += value => light.LightSamples = (int)Mathf.Sqrt(value);
            typeDropdown.onValueChanged.AddListener(type => ChangeObjectType((RTLight.RTLightType)type));
        }

        private void FixedUpdate()
        {
            // Update the UI based on external changes to the light transform (e.g. through the transformation gizmos).
            bool inUI = EventSystem.current.currentSelectedGameObject != null; // Only update if we are not in the UI.
            bool draggingEdit = positionEdit.IsDragging() || rotationEdit.IsDragging() || scaleEdit.IsDragging();
            if (light != null && light.transform.hasChanged && !inUI && !draggingEdit)
            {
                positionEdit.Value = light.transform.position;
                rotationEdit.Value = light.transform.eulerAngles;
                scaleEdit.Value = light.transform.localScale;
            }
        }

        private void Update()
        {
            light.transform.hasChanged = false;   // Do this in Update to let other scripts also check
        }
    }
}
