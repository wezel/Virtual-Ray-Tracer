using System;
using UnityEngine;
using UnityEngine.UI;

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
    //[RequireComponent(typeof(Light))]
    public class RTAreaLight : MonoBehaviour
    {

        /// <summary>
        /// This function encodes the color data. By encoding the color data we have extra room to send other data to
        /// the graphic renderer.
        /// </summary>
        [SerializeField]
        private Color color;
        public Color Color
        {
            get => color;
            set
            {
                value.a = 1;
                color = value;
                //label.color = value;

                Color lightData = light.color;
                lightData.r = Mathf.Floor(value.r * 256) + value.g / 2;
                lightData.g = value.b;
                light.color = lightData;

                OnLightChanged?.Invoke();
            }
        }

        [SerializeField, Range(0,1)]
        private float ambient;
        public float Ambient
        {
            get => ambient;
            set
            {
                ambient = value;

                Color lightData = light.color;
                lightData.b = lightData.b % 1 + Mathf.Floor(value * 256);
                light.color = lightData;

                OnLightChanged?.Invoke();
            }
        }

        [SerializeField, Range(0,1)]
        private float diffuse;

        public float Diffuse
        {
            get => diffuse;
            set
            {
                diffuse = value;

                Color lightData = light.color;
                lightData.b = Mathf.Floor(lightData.b) + value / 2;
                light.color = lightData;

                OnLightChanged?.Invoke();
            }
        }

        [SerializeField, Range(0,1)]
        private float specular;

        public float Specular
        {
            get => specular;
            set
            {
                specular = value;

                Color lightData = light.color;
                lightData.a = specular;
                light.color = lightData;

                OnLightChanged?.Invoke();
            }
        }
        
        public delegate void LightChanged();
        /// <summary>
        /// An event invoked whenever a property of this light is changed.
        /// </summary>
        public event LightChanged OnLightChanged;

        /// <summary>
        /// The underlying <see cref="UnityEngine.Light"/> used by the light.
        /// </summary>
        [SerializeField]
        private new Light light;

        public LightShadows Shadows { get => light.shadows; set => light.shadows = value; }


        /// <summary>
        /// The position of the light.
        /// </summary>
        public Vector3 Position
        {
            get { return transform.position; }
            set
            {
                transform.position = value;
                OnLightChanged?.Invoke();
            }
        }

        private readonly System.Random rnd = new System.Random();

        private float range = 1f;
        public Vector3[] RandomPointsOnLight(int samples)
        {
            Vector3[] points = new Vector3[samples*samples];
            Vector3 start = Position;
            start.x -= range / 2f;
            start.z -= range / 2f;

            float step = range / samples;
            for (int i = 0; i < samples * samples; i++)
            {
                points[i] = start;
                points[i].x += (i / samples) * step + (float)rnd.NextDouble() * step;
                points[i].z += (i % samples) * step + (float)rnd.NextDouble() * step;
            }

            return points;
        }
        public Vector3[] GetEdgePoints()
        {
            Vector3[] edgePoints = new Vector3[8];
            edgePoints[0] = Position; edgePoints[0].x += range / 2f; edgePoints[0].y += range / 2f;
            edgePoints[1] = Position; edgePoints[1].x -= range / 2f; edgePoints[1].y += range / 2f;
            edgePoints[2] = Position; edgePoints[2].x += range / 2f; edgePoints[2].y -= range / 2f;
            edgePoints[3] = Position; edgePoints[3].x -= range / 2f; edgePoints[3].y -= range / 2f;

            edgePoints[4] = (edgePoints[0] + edgePoints[1]) / 2f;
            edgePoints[5] = (edgePoints[0] + edgePoints[2]) / 2f;
            edgePoints[6] = (edgePoints[1] + edgePoints[3]) / 2f;
            edgePoints[7] = (edgePoints[2] + edgePoints[3]) / 2f;

            return edgePoints;
        }

        //[SerializeField]
        //private Image label;

        //[SerializeField]
        //private Image outline;

        //[SerializeField]
        //private Canvas canvas;

        private Color defaultOutline;

        public void Higlight(Color value)
        {
            //outline.color = value;
        }

        public void ResetHighlight()
        {
            //outline.color = defaultOutline;
        }

        private void Awake()
        {
            //defaultOutline = outline.color;
        }
        
        private void LateUpdate()
        {
            #if UNITY_EDITOR
                if(!Application.isPlaying) return;
            #endif
            // Make the label face the camera. We do this in LateUpdate to make sure the camera has finished its moving.
            // From: https://answers.unity.com/questions/52656/how-i-can-create-an-sprite-that-always-look-at-the.html
            //canvas.transform.forward = Camera.main.transform.forward;
        }

#if UNITY_EDITOR

        private void OnEnable()
        {
            //label.color = color;
        }

        private void OnRenderObject()
        {
            // Fix maximize window errors
            if (UnityEditor.SceneView.lastActiveSceneView == null) 
                return;
            //canvas.transform.forward = UnityEditor.SceneView.lastActiveSceneView.camera.transform.forward;
        }
#endif
        
    }
}
