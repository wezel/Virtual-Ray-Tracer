#if UNITY_EDITOR

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
        private List<Light> lights;
        public override LightShadows Shadows
        {
            get => lights[0].shadows;
            set => lights.ForEach(light => light.shadows = value);
        }

        // Important! The max of this range is also hardcoded in the shader!
        [SerializeField, Range(0, 64)]
        private float spotAttenuationPower = 1f;
        public override float SpotAttenuationPower 
        {
            get { return spotAttenuationPower; }
            set
            {
                if (value == spotAttenuationPower) return;
                if (value < 0 || value > 64) return;
                spotAttenuationPower = value;
                UpdateLightData();
                OnLightChangedInvoke();
            }
        }

        public override void UpdateLightData()
        {
            Color lightData;
            lightData.r = Mathf.Floor(color.r / LightSamples * 256) + color.g / LightSamples / 2;
            lightData.g = Mathf.Floor(color.b / LightSamples * 256) + (intensity / intensityDivisor);
            lightData.b = Mathf.Floor(ambient / (LightSamples * LightSamples) * 256) + diffuse / LightSamples / 2
                        + Mathf.Floor(Mathf.Floor(spotAttenuationPower / 100f * 256) * 256 * 2);
            lightData.a = Mathf.Floor(specular / LightSamples * 256) + Mathf.Clamp01(Mathf.Cos(areaSpotAngle * Mathf.PI / 360f)) / 2f + (lightDistanceAttenuation ? 512 : 0);
            lights.ForEach(light => light.color = lightData);
        }

        private RectTransform rectTransform;
        private RectTransform RectTransform { get => rectTransform; }

        public Vector3[] GetWorldCorners()
        {
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            return worldCorners;
        }

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
                        + (i / LightSamples) * step_1 + Random.value * step_1
                        + (i % LightSamples) * step_2 + Random.value * step_2;

            return points;
        }

        [SerializeField, Range(2, 10)]
        private int lightSamples = 4;

        /// <summary>
        /// The square root of the number of samples this light uses to estimate an area light.
        /// </summary>
        public override int LightSamples
        {
            get { return lightSamples; }
            set
            {
                if (value == lightSamples) return;
                if (value < 2 || value > 10) return;
                lightSamples = value;
                UpdateLights();
                onLightSampleChanged?.Invoke();
            }
        }

        [SerializeField]
        private GameObject spotLightPrefab;

        public LightChanged onLightSampleChanged;

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
            RemoveUnnecessaryLights();  // If the lightsamples went down, we have to remove lights
            AddMissingLights();         // If the lightsamples went up,   we have to add    lights
            PositionLights();           // Position the new lights correctly            
            UpdateLightData();          // Set all lights to the correct data.
            OnLightChangedInvoke();
        }

        private void RemoveUnnecessaryLights()
        {
            for (int i = lights.Count - 1; i >= LightSamples * LightSamples; --i)
            {
#if UNITY_EDITOR
                DestroyImmediate(lights[i].gameObject);
#else
                Destroy(lights[i].gameObject);
#endif
                lights.RemoveAt(i);
            }
            lights.TrimExcess();
        }

        private void AddMissingLights()
        {
            for (int i = lights.Count; i < LightSamples * LightSamples; ++i)
                lights.Add(Instantiate(spotLightPrefab, transform).GetComponent<Light>());
        }

        private void PositionLights()
        {
            float stepx = RectTransform.rect.width / (LightSamples - 1);
            float stepy = RectTransform.rect.height / (LightSamples - 1);
            float startx = -RectTransform.rect.width / 2;
            float starty = -RectTransform.rect.height / 2;
            float maxBias = 2 * (LightSamples - 1);

            for (int i = 0; i < LightSamples; i++)
            {
                for (int j = 0; j < LightSamples; j++)
                {
                    Light light = lights[i * LightSamples + j];
                    light.transform.localPosition = new Vector3(startx + i * stepx, starty + j * stepy, 0f);
                    light.shadowBias = (i + j) / maxBias * 0.035f + 0.005f;
                    light.spotAngle = areaSpotAngle;
                }
            }
        }

        protected override void Awake()
        {
            Type = RTLightType.Area;
            rectTransform = GetComponent<RectTransform>();
            lights = new List<Light>();
            foreach (Light light in GetComponentsInChildren<Light>()) lights.Add(light);
            UpdateLights();
            base.Awake();
        }

        private new void Update()
        {
            base.Update();

            // Update label; black at the back; light color at the front
            UpdateLabelColor();

#if UNITY_EDITOR // Update the (amount of) lights in the editor
            if (lights != null && lights.Count == LightSamples * LightSamples)
                return;
            else if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) // In Prefab Mode
                return;                                                  // Don't add lights to the prefab
            else
                UpdateLights(); // Update lights as well in the editor
#endif
        }

    }
}
