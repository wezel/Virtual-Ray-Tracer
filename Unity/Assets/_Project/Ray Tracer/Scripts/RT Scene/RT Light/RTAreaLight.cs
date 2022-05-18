using System;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;

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
        /// <summary>
        /// This function encodes the color data. By encoding the color data we have extra room to send other data to
        /// the graphic renderer.
        /// </summary>
        public override Color Color
        {
            get => color;
            set
            {
                Color color = value;
                value /= lightSamples;
                value.a = 1f;

                Color lightData = lights[0].color;
                lightData.r = Mathf.Floor(value.r * 256) + value.g / 2;
                lightData.g = value.b;
                foreach (Light light in lights)
                    light.color = lightData;

                base.Color = color;
            }
        }

        public override float Ambient
        {
            get => ambient;
            set
            {
                float ambient = value;
                value /= (lightSamples);

                Color lightData = lights[0].color;
                lightData.b = lightData.b % 1 + Mathf.Floor(value * 256);
                foreach (Light light in lights)
                    light.color = lightData;

                base.Ambient = ambient;
            }
        }

        public override float Diffuse
        {
            get => diffuse;
            set
            {
                float diffuse = value;
                value /= (lightSamples);

                Color lightData = lights[0].color;
                lightData.b = Mathf.Floor(lightData.b) + value / 2;
                foreach (Light light in lights)
                    light.color = lightData;

                base.Diffuse = diffuse;
            }
        }

        //[SerializeField, Range(0, 1)]
        //private float specular;

        public override float Specular
        {
            get => specular;
            set
            {
                float specular = value;
                value /= (lightSamples);

                Color lightData = lights[0].color;
                lightData.a = value;
                foreach (Light light in lights)
                    light.color = lightData;

                base.Specular = specular;
            }
        }

        /// <summary>
        /// The underlying <see cref="UnityEngine.Light"/>s used by the light.
        /// </summary>
        private Light[] lights;

        private RectTransform rectTransform { get => GetComponent<RectTransform>(); }

        /// <summary>
        /// The rotation of the mesh.
        /// </summary>
        public Vector3 Rotation
        {
            get => transform.eulerAngles;
            set
            {
                transform.eulerAngles = value;
                OnLightChangedInvoke();
            }
        }

        /// <summary>
        /// The scale of the mesh.
        /// </summary>
        public Vector3 Scale
        {
            get => transform.localScale;
            set
            {
                transform.localScale = value;
                OnLightChangedInvoke();
            }
        }

        private readonly System.Random rnd = new System.Random();

        /// <summary>
        /// Samples uniformly random points on the arealight
        /// </summary>
        /// <param name="samples"> The square root of how many points should be sampled </param>
        /// <returns> <paramref name="samples"/>^2 points on the arealight. </returns>
        public Vector3[] RandomPointsOnLight(int samples)
        {
            Vector3[] points = new Vector3[samples * samples];
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);  // clockwise corners

            Vector3 step_1 = (corners[1] - corners[0]) / samples; // corner 0 to corner 1
            Vector3 step_2 = (corners[3] - corners[0]) / samples; // corner 0 to corner 3


            for (int i = 0; i < samples * samples; i++)
                points[i] = corners[0]
                        + (i / samples) * step_1 + (float)rnd.NextDouble() * step_1
                        + (i % samples) * step_2 + (float)rnd.NextDouble() * step_2;

            return points;
        }

        /// <summary>
        /// Returns <paramref name="n"/> points between point <paramref name="p1"/> and <paramref name="p2"/>.
        /// </summary>
        /// <param name="n"> The numer of points requested. </param>
        /// <param name="p1"> The first point. </param>
        /// <param name="p2"> The second point. </param>
        /// <returns> n points between p1 and p2 </returns>
        private Vector3[] GetPointsBetween(int n, Vector3 p1, Vector3 p2)
        {
            Vector3[] points = new Vector3[n];
            Vector3 step = (p2 - p1) / (n + 1);

            for (int i = 0; i < n; ++i)
                points[i] = p1 + (i + 1) * step;

            return points;
        }

        /// <summary>
        /// Returns points on the edges of the arealight, with <paramref name="pointsBetweenCorners"/> between each corner.
        /// </summary>
        /// <param name="pointsBetweenCorners"> The numer of point between the corners. </param>
        /// <returns> Points on the edges of the arealight, including the corners. </returns>
        public Vector3[] GetEdgePoints(int pointsBetweenCorners)
        {
            int idx = 4;
            Vector3[] edgePoints = new Vector3[4 * (pointsBetweenCorners + 1)];
            rectTransform.GetWorldCorners(edgePoints); // Corners clockwise

            foreach (Vector3 point in GetPointsBetween(pointsBetweenCorners, edgePoints[0], edgePoints[1]))
                edgePoints[idx++] = point;
            foreach (Vector3 point in GetPointsBetween(pointsBetweenCorners, edgePoints[1], edgePoints[2]))
                edgePoints[idx++] = point;
            foreach (Vector3 point in GetPointsBetween(pointsBetweenCorners, edgePoints[2], edgePoints[3]))
                edgePoints[idx++] = point;
            foreach (Vector3 point in GetPointsBetween(pointsBetweenCorners, edgePoints[0], edgePoints[3]))
                edgePoints[idx++] = point;

            return edgePoints;
        }


        [SerializeField]
        [Range(2, 16)]
        private int lightSamples;

        [SerializeField]
        private GameObject pointLightPrefab;

        private bool update = false;

        private void Update()
        {
            // Update label; black at the back; light color at the front
#if UNITY_EDITOR
            if (!Application.isPlaying)
                if (Vector3.Dot(transform.forward, (transform.position - UnityEditor.SceneView.lastActiveSceneView.camera.transform.position).normalized) > 0)
                    label.color = Color.black;
                else
                    label.color = Color;
            else
#endif
                if (Vector3.Dot(transform.forward, (transform.position - Camera.main.transform.position).normalized) > 0)
                    label.color = Color.black;
                else
                    label.color = Color;

            // Update the (amount of) lights
#if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) // In Prefab Mode
                return;                                             // Don't add lights to the prefab
#endif
            if (!update && lights != null && lights.Length == lightSamples * lightSamples)
                return;

            foreach (Light light in GetComponentsInChildren<Light>())
#if UNITY_EDITOR
                DestroyImmediate(light.gameObject);
#else
                Destroy(light.gameObject);
#endif
            lights = new Light[lightSamples * lightSamples];
            float stepx = rectTransform.rect.width / (lightSamples - 1);
            float stepy = rectTransform.rect.height / (lightSamples - 1);
            Vector3 start = new Vector3(0f, 0f, 0f);
            start.x -= rectTransform.rect.width / 2;
            start.y -= rectTransform.rect.height / 2;
            float maxBias = 2 * (lightSamples - 1);

            for (int i = 0; i < lightSamples; i++)
            {
                for (int j = 0; j < lightSamples; j++)
                {
                    Light light = Instantiate(pointLightPrefab, transform).GetComponent<Light>();
                    Vector3 pos = start;
                    pos.x += i * stepx;
                    pos.y += j * stepy;
                    light.transform.localPosition = pos;
                    light.shadowBias = (i + j)/ maxBias * 0.035f + 0.005f;
                    lights[i * lightSamples + j] = light;
                }
            }

            // 'Call' these funtions to set the all the lights correctly
            Color = Color;
            Ambient = Ambient;
            Diffuse = Diffuse;
            Specular = Specular;
            update = false;
        }
                
    }
}
