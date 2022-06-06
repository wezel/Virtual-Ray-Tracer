using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray
{
    /// <summary>
    /// Represents a ray produced by the ray tracer. It is effectively a plain data struct, but it also provides a few
    /// utility functions.
    /// </summary>
    public class RTRay
    {
        /// <summary>
        /// The type of a ray produced by the ray tracer.
        /// </summary>
        public enum RayType
        {
            NoHit,
            Reflect,
            Refract,
            Normal,
            Shadow,
            Light,
            AreaShadow,
            AreaLight
        }

        /// <summary>
        /// The origin from which this ray was traced. Generally this is the camera position.
        /// </summary>
        public Vector3 Origin { get; set; }

        /// <summary>
        /// The direction in which this ray was traced. This should be a unit vector.
        /// </summary>
        public Vector3 Direction { get; set; }

        /// <summary>
        /// The length of this ray. If this ray intersected an object it will be the distance between
        /// <see cref="Origin"/> and the intersection point, otherwise it will be infinity.
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// The color this ray contributes to its pixel in the final image. For example, the color of a ray at the root
        /// of a ray tree will be the color of a pixel (assuming no super sampling takes place), while the color of a
        /// reflection ray will only be the color that the reflection contributes to its root pixel.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The type of this ray. For example, reflection, refraction or shadow.
        /// </summary>
        public RayType Type { get; set; }

        private float contribution;
        /// <summary>
        /// The contribution of this ray.
        /// </summary>
        public float Contribution
        {
            get { return contribution; }
            set { contribution = value; }
        }

        public Vector3[] AreaLightPoints { get; set; }

        public bool AreaRay { get; set; } = false;

        /// <summary>
        /// Construct a default ray. The resulting ray is technically valid, but should only be used in the
        /// construction of a proper ray.
        /// </summary>
        public RTRay()
        {
            Origin = Vector3.zero;
            Direction = Vector3.right; // Not Vector3.zero because that is not a valid direction vector.
            Length = 0.0f;
            Color = Color.black;
            Type = RayType.NoHit;
        }

        /// <summary>
        /// Construct a new ray.
        /// </summary>
        /// <param name="origin"> The origin from which this ray was traced. </param>
        /// <param name="direction"> The direction in which this ray was traced. This should be a unit vector. </param>
        /// <param name="length"> The length of this ray. </param>
        /// <param name="color">  The color this ray contributes to its pixel in the final image. </param>
        /// <param name="type"> The type of this ray. </param>
        public RTRay(Vector3 origin, Vector3 direction, float length, Color color, RayType type)
        {
            Origin = origin;
            Direction = direction;
            Length = length;
            Color = color;
            Type = type;
            Contribution = type == RayType.NoHit || type == RayType.Shadow ? 0.0f : 1.0f;
        }

        public RTRay(Vector3 origin, Vector3 direction, float lengthScale, Color color, RayType type, Vector3[] areaLightPoints)
        :
            this(origin, direction, lengthScale, color, type)
        {
            AreaRay = true;
            AreaLightPoints = areaLightPoints;
        }

        public int ObjectPoolIndex { get; set; }

        /// <summary>
        /// Determine the position of a point a certain distance along this ray.
        /// </summary>
        /// <param name="distance"> The distance to travel along this ray. </param>
        /// <returns> A <see cref="Vector3"/> point <paramref name="distance"/> along this ray. </returns>
        public Vector3 At(float distance) => Origin + distance * Direction;

        /// <summary>
        /// Divides the <paramref name="ray"/>'s Color by <paramref name="div"/>
        /// </summary>
        /// <param name="ray"> The ray of which the color needs to be divided. </param>
        /// <param name="div"> The divisor for the color. </param>
        /// <returns> The original <paramref name="ray"/> </returns>
        public static RTRay operator /(RTRay ray, int div)
        {
            ray.Color /= div;
            return ray;
        }
    }
}