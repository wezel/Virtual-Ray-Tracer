using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Area_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Point_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Spot_Light;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

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
        private GameObject zRotationEdit; // Qucik access to disable it for spotlights
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
        private FloatEdit spotAngleEdit;
        [SerializeField]
        private FloatEdit spotAttenuationEdit;
        [SerializeField]
        private BoolEdit distanceAttenuationEdit;

        [SerializeField]
        private TMP_Dropdown typeDropdown;

        [SerializeField] private RTPointLight pointLightPrefab; 
        [SerializeField] private RTSpotLight spotLightPrefab;
        [SerializeField] private RTAreaLight areaLightPrefab;

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
                spotAttenuationEdit.Value = ((RTAreaLight)light).SpotAttenuationPower;
            }
            if (light.Type == RTLight.RTLightType.Spot)
            {
                rotationEdit.Value = ((RTSpotLight)light).Rotation;
                spotAngleEdit.Value = ((RTSpotLight)light).SpotAngle;
                spotAttenuationEdit.Value = ((RTSpotLight)light).SpotAttenuationPower;
            }

            rotationEdit.gameObject.SetActive(light.Type == RTLight.RTLightType.Area || light.Type == RTLight.RTLightType.Spot);
            zRotationEdit.gameObject.SetActive(light.Type == RTLight.RTLightType.Area);
            scaleEdit.gameObject.SetActive(light.Type == RTLight.RTLightType.Area);
            lightSamplesEdit.gameObject.SetActive(light.Type == RTLight.RTLightType.Area);
            spotAngleEdit.gameObject.SetActive(light.Type == RTLight.RTLightType.Spot);
            spotAttenuationEdit.gameObject.SetActive(light.Type == RTLight.RTLightType.Area || light.Type == RTLight.RTLightType.Spot);

            typeDropdown.gameObject.SetActive(true);
            typeDropdown.interactable = RTSceneManager.Get().DeleteAllowed; // Should only be available if lights may be deleted
            SetTypeDropdown();
            typeDropdown.value = typeDropdown.options.FindIndex(option => option.text == light.Type.ToString());

            colorEdit.Color = light.Color;
            intensityEdit.Value = light.Intensity;
            ambientEdit.Value = light.Ambient;
            diffuseEdit.Value = light.Diffuse;
            specularEdit.Value = light.Specular;
            spotAngleEdit.Value = light.SpotAngle;
            distanceAttenuationEdit.IsOn = light.LightDistanceAttenuation;
        }

        private void SetTypeDropdown()
        {
            RTScene scene = RTSceneManager.Get().Scene;
            List<string> types = new List<string>(3);
            if (scene.EnablePointLights) types.Add(RTLight.RTLightType.Point.ToString());
            if (scene.EnableSpotLights) types.Add(RTLight.RTLightType.Spot.ToString());
            if (scene.EnableAreaLights) types.Add(RTLight.RTLightType.Area.ToString());
            typeDropdown.options.Clear();
            typeDropdown.AddOptions(types);
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
            if (type == light.Type) return;
            RTSceneManager manager = RTSceneManager.Get();
            if (type == RTLight.RTLightType.Point && !manager.Scene.EnablePointLights) return;
            if (type == RTLight.RTLightType.Spot && !manager.Scene.EnableSpotLights) return;
            if (type == RTLight.RTLightType.Area && !manager.Scene.EnableAreaLights) return;

            Vector3 position = light.Position;
            Vector3 rotation = light.Rotation;
            Vector3 scale = light.Scale;
            Color color = light.Color;
            float intensity = light.Intensity;
            float ambient = light.Ambient;
            float diffuse = light.Diffuse;
            float specular = light.Specular;
            float spotAngle = light.SpotAngle;
            float spotAttenuationPower = light.SpotAttenuationPower;
            int lightSamples = light.LightSamples;
            bool distanceAttenuation = light.LightDistanceAttenuation;

            manager.DeleteSelected();

#if UNITY_EDITOR
            DestroyImmediate(light);
#else
            Destroy(light);
#endif

            if (type == RTLight.RTLightType.Point)
                light = Instantiate(pointLightPrefab);
            else if (type == RTLight.RTLightType.Spot)
                light = Instantiate(spotLightPrefab);
            else // type == RTLight.RTLightType.Area
                light = Instantiate(areaLightPrefab);

            light.gameObject.SetActive(true);
            light.Position = position;
            light.Rotation = rotation;  
            light.Scale = scale;
            light.Color = color;
            light.Intensity = intensity;
            light.Ambient = ambient;
            light.Diffuse = diffuse;
            light.Specular = specular;
            light.SpotAngle = spotAngle;
            light.SpotAttenuationPower = spotAttenuationPower;
            light.LightSamples = lightSamples;
            light.LightDistanceAttenuation = distanceAttenuation;
            light.UpdateLightData(); // Call this explicitly in case all of the above are the same as prefab

            manager.Scene.AddLight(light);
            manager.Select(light.transform);
            Show(light);
        }

        private void Awake()
        {
            positionEdit.OnValueChanged.AddListener(value => { light.Position = value; });
            rotationEdit.OnValueChanged.AddListener(value => { light.Rotation = value; });
            scaleEdit.OnValueChanged.AddListener(value => { light.Scale = value; });

            colorEdit.OnValueChanged.AddListener(value => { light.Color = value; });
            intensityEdit.OnValueChanged.AddListener(value => { light.Intensity = value; });
            ambientEdit.OnValueChanged.AddListener((value) => { light.Ambient = value; });
            diffuseEdit.OnValueChanged.AddListener((value) => { light.Diffuse = value; });
            specularEdit.OnValueChanged.AddListener((value) => { light.Specular = value; });

            lightSamplesEdit.OnValueChanged.AddListener(value => light.LightSamples = (int)Mathf.Sqrt(value));
            spotAngleEdit.OnValueChanged.AddListener(value => light.SpotAngle = value);
            spotAttenuationEdit.OnValueChanged.AddListener(value => light.SpotAttenuationPower = value);
            distanceAttenuationEdit.OnValueChanged.AddListener(value => light.LightDistanceAttenuation = value);
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
            // Intensity can be changed by Distance-attenuation
            if (intensityEdit.Value != light.Intensity)
                intensityEdit.Value = light.Intensity;
        }

        private void Update()
        {
            light.transform.hasChanged = false;   // Do this in Update to let other scripts also check
        }
    }
}
