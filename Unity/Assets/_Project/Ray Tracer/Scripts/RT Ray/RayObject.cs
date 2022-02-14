using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray
{
    /// <summary>
    /// A Unity object that visually represent a ray traced by the ray tracer.
    /// </summary>
    [RequireComponent(typeof(RayRenderer))]
    public class RayObject : MonoBehaviour
    {
        private RTRay ray;
        /// <summary>
        /// The <see cref="RTRay"/> produced by the ray tracer that this ray object represents.
        /// </summary>
        public RTRay Ray 
        { 
            get { return ray; }
            set { ray = value; Reset(); }
        }

        /// <summary>
        /// The length to which this ray object is drawn. Generally this is the same as the length of <see cref="Ray"/>,
        /// but if the ray is infinitely long the drawn length will be set to
        /// <see cref="RayManager.InfiniteRayDrawLength"/>.
        /// </summary> 
        public float DrawLength { get; private set; }

        private RayRenderer rayRenderer;
        private RayManager rayManager;

        /// <summary>
        /// Draw the ray as a cylinder where <paramref name="radius"/> determines the drawn radius of the cylinder. The
        /// length of the cylinder is the same as the ray's true length unless that length is infinity, then the length is
        /// clamped to <see cref="RayManager.InfiniteRayDrawLength"/>.
        /// </summary>
        /// <param name="radius"> The drawn radius of the cylinder. </param>
        public void Draw(float radius)
        {
            rayRenderer.Radius = radius;
            rayRenderer.Length = DrawLength;
        }

        /// <summary>
        /// Draw the ray as a cylinder where <paramref name="radius"/> determines the drawn radius of the cylinder. The
        /// length of the cylinder is given by <paramref name="length"/>, but it is clamped between 0 and
        /// <see cref="DrawLength"/>.
        /// </summary>
        /// <param name="radius"> The drawn radius of the cylinder. </param>
        /// <param name="length"> The drawn length of the cylinder. Clamped between 0 and <see cref="DrawLength"/> </param>
        public void Draw(float radius, float length)
        {
            rayRenderer.Radius = radius;
            rayRenderer.Length = Mathf.Clamp(length, 0.0f, DrawLength);
        }

        private void Reset()
        {
            DetermineDrawLength();

            rayRenderer.Origin = Ray.Origin;
            rayRenderer.Direction = Ray.Direction;
            rayRenderer.Length = 0.0f;
            rayRenderer.Material = rayManager.GetRayTypeMaterial(Ray.Type);
        }

        private void DetermineDrawLength()
        {
            DrawLength = float.IsInfinity(Ray.Length) ? rayManager.InfiniteRayDrawLength : Ray.Length;
        }

        private void Awake()
        {
            rayRenderer = GetComponent<RayRenderer>();
        }

        private void Start()
        {
            rayManager = RayManager.Get();
        }

        private void OnEnable()
        {
            rayManager = RayManager.Get();
        }
    }
}
