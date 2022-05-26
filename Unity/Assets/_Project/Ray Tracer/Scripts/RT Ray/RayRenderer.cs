using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray
{
    /// <summary>
    /// Renders a ray traced by the ray tracer as an elongated cylinder.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RayRenderer : MonoBehaviour
    {
        private Vector3 origin;
        /// <summary>
        /// The origin from which the ray should be drawn.
        /// </summary>
        public Vector3 Origin
        {
            get { return origin; }
            set
            {
                // Because we often reset the origin to the same value this check improves performance.
                if (origin == value)
                    return;

                origin = value;
                transform.position = origin;
            }
        }

        private Vector3 direction;
        /// <summary>
        /// The direction in which the ray should be drawn.
        /// </summary>
        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                // Because we often reset the direction to the same value this check improves performance.
                if (direction == value)
                    return;

                direction = value;
                transform.up = direction;
            }
        }

        private float length;
        /// <summary>
        /// The length of the drawn ray.
        /// </summary>
        public float Length
        {
            get { return length; }
            set
            {
                // Because we often reset the length to the same value this check improves performance.
                if (length == value)
                    return;

                length = value;
                transform.localScale = new Vector3(Radius, length, Radius);
            }
        }

        private float radius;
        /// <summary>
        /// The radius of the drawn ray.
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set
            {
                // Because we often reset the radius to the same value this check improves performance.
                if (radius == value)
                    return;

                radius = value;
                transform.localScale = new Vector3(radius, Length, radius);
            }
        }

        private Material material;
        /// <summary>
        /// The material used to draw the ray.
        /// </summary>
        public Material Material
        {
            get { return material; }
            set
            {
                // Because we often reset the material to the same value this check improves performance.
                if (material == value)
                    return;

                material = value;
                meshRenderer.material = material;
            }
        }

        private MeshRenderer meshRenderer;

        public void SetAreaLightRay(Vector3[] areaLightVerts)
        {
            Vector3[] correctedVerts = new Vector3[areaLightVerts.Length];
            for (int i = 0; i < areaLightVerts.Length; ++i) // Move a bit to prevent z-fighting and move away from light
                correctedVerts[i] = areaLightVerts[i] + 0.001f * Random.insideUnitSphere;

            transform.localScale = Vector3.one * 1.002f; // Set the scale so the ray stops just before the light.
            Vector3[] newverts = new Vector3[5];
            // This order doesn't correspond to the .obj, but somehow this is Unity's order.
            newverts[0] = transform.InverseTransformPoint(correctedVerts[1]);
            newverts[1] = transform.InverseTransformPoint(correctedVerts[0]);
            newverts[2] = GetComponent<MeshFilter>().mesh.vertices[2];
            newverts[3] = transform.InverseTransformPoint(correctedVerts[2]);
            newverts[4] = transform.InverseTransformPoint(correctedVerts[3]);
            GetComponent<MeshFilter>().mesh.vertices = newverts;
        }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        //[SerializeField]
        //private bool Print = false;

        //private void Update()
        //{
        //    if (Print)
        //    {
        //        Vector3[] verts = GetComponent<MeshFilter>().mesh.vertices;
        //        for (int i = 0; i < verts.Length; i++)
        //            Debug.Log(verts[i]);
        //        Print = false;
        //    }
        //}
    }
}
