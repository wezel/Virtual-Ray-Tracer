#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using System.Collections.Generic;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.RT_Area_Light
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
    public class RTAreaLight : RTLight
    {
        private const float areaSpotAngle = 175f;
        public override float SpotAngle { get => areaSpotAngle; }

        /// <summary>
        /// The underlying <see cref="UnityEngine.Light"/>s used by the light.
        /// </summary>
        private Light[] lights;
        public override LightShadows Shadows
        {
            get => lights[0].shadows;
            set
            {
                foreach (Light light in lights) light.shadows = value;
            }
        }

        public override void UpdateLightData()
        {
            Color lightData;
            lightData.r = Mathf.Floor(color.r / LightSamples * 256) + color.g / LightSamples / 2;
            lightData.g = Mathf.Floor(color.b / LightSamples * 256) + (intensity / intensityDivisor);
            lightData.b = Mathf.Floor(ambient / (LightSamples * LightSamples) * 256) + diffuse / LightSamples / 2;
            lightData.a = Mathf.Floor(specular / LightSamples * 256) + Mathf.Clamp01(Mathf.Cos(areaSpotAngle * Mathf.PI / 360f)) / 2f + (lightDistanceAttenuation ? 512 : 0);
            foreach (Light light in lights)
                light.color = lightData;
        }

        private RectTransform rectTransform;
        private RectTransform RectTransform { get => rectTransform; }

        private readonly System.Random rnd = new System.Random();

        /// <summary>
        /// Samples uniformly random points on the arealight
        /// </summary>
        /// <param name="samples"> The square root of how many points should be sampled </param>
        /// <returns> <paramref name="samples"/>^2 points on the arealight. </returns>
        public Vector3[] SampleLight()
        {
            Vector3[] points = new Vector3[LightSamples * LightSamples];
            Vector3[] corners = new Vector3[4];
            RectTransform.GetWorldCorners(corners);  // clockwise corners

            Vector3 step_1 = (corners[1] - corners[0]) / LightSamples; // corner 0 to corner 1
            Vector3 step_2 = (corners[3] - corners[0]) / LightSamples; // corner 0 to corner 3

            for (int i = 0; i < LightSamples * LightSamples; i++)
                points[i] = corners[0]
                        + (i / LightSamples) * step_1 + (float)rnd.NextDouble() * step_1
                        + (i % LightSamples) * step_2 + (float)rnd.NextDouble() * step_2;

            return points;
        }

        [SerializeField, Range(2, 10)]
        private int lightSamples = 4;

        /// <summary>
        /// The square root of the numer of samples this light uses to estimate an area light.
        /// </summary>
        public override int LightSamples
        {
            get { return lightSamples; }
            set
            {
                if (value == lightSamples) return;
                if (value >= 2 && value <= 10)
                    lightSamples = value;
                OnLightChangedInvoke();
            }
        }

        [SerializeField]
        private GameObject spotLightPrefab;

        private void UpdateLabelColor()
        {
#if UNITY_EDITOR
            Vector3 cameraPos;
            if (!Application.isPlaying)
                cameraPos = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
            else
                cameraPos = Camera.main.transform.position;
#else
            Vector3 cameraPos = Camera.main.transform.position;
#endif
            if (Vector3.Dot(transform.forward, (transform.position - cameraPos).normalized) > 0)
                label.color = Color.black;
            else
                label.color = Color;
        }

        private void UpdateLights()
        {
            foreach (Light light in GetComponentsInChildren<Light>())
#if UNITY_EDITOR
                DestroyImmediate(light.gameObject);
#else
                Destroy(light.gameObject);
#endif
            lights = new Light[LightSamples * LightSamples];
            float stepx = RectTransform.rect.width / (LightSamples - 1);
            float stepy = RectTransform.rect.height / (LightSamples - 1);
            float startx = -RectTransform.rect.width / 2;
            float starty = -RectTransform.rect.height / 2;
            float maxBias = 2 * (LightSamples - 1);

            for (int i = 0; i < LightSamples; i++)
            {
                for (int j = 0; j < LightSamples; j++)
                {
                    Light light = Instantiate(spotLightPrefab, transform).GetComponent<Light>();
                    light.transform.localPosition = new Vector3(startx + i * stepx, starty + j * stepy, 0f);
                    light.shadowBias = (i + j) / maxBias * 0.035f + 0.005f;
                    light.spotAngle = 175;
                    lights[i * LightSamples + j] = light;
                }
            }

            // Set all lights to the correct data.
            UpdateLightData();
            OnLightChangedInvoke();
        }

        protected override void Awake()
        {
            Type = RTLightType.Area;
            rectTransform = GetComponent<RectTransform>();
            UpdateLights();
            base.Awake();
        }

        private new void Update()
        {
            base.Update();

            // Update label; black at the back; light color at the front
            UpdateLabelColor();

            // Update the (amount of) lights
            if (lights != null && lights.Length == LightSamples * LightSamples)
                return;
#if UNITY_EDITOR
            else if (PrefabStageUtility.GetCurrentPrefabStage() != null) // In Prefab Mode
                return;                                                  // Don't add lights to the prefab
#endif
            else
                UpdateLights();
        }
                
    }
}
