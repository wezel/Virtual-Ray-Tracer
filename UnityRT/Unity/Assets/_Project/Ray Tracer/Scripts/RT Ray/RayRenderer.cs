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

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}
