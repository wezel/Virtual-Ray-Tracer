using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using UnityEngine;

namespace _Project.UI.Scripts.Animation_Tools
{
    /// <summary>
    /// A UI class specifically created to animate the objects in the opening scene and give easy
    /// access to ray tracer settings <see cref="RayManager"/>. This class contains a couple of variables
    /// describing what ray tracing settings should be used and how the object should rotate. 
    /// </summary>

    [RequireComponent(typeof(RTMesh))]
    public class MeshAnimationSettings : MonoBehaviour
    {
        [SerializeField]
        private int maxDepth = 3;

        [SerializeField]
        private bool hideNoHitRays;

        [SerializeField]
        private bool animate;

        [SerializeField]
        private bool loop;

        [SerializeField]
        private bool sequentialAnimate;

        [SerializeField]
        private float speed = 2;

        [SerializeField]
        private float rotationSpeed;

        [SerializeField]
        private Vector3 axis;

        private RTMesh mesh;

        private bool update;

        /// <summary>
        /// Reset the script to its starting configuration when enabled.
        /// </summary>
        private void OnEnable()
        {
            UnityRayTracer.Get().MaxDepth = maxDepth;
            RayManager rayManager = RayManager.Get();
            rayManager.Animate = animate;
            rayManager.Loop = loop;
            rayManager.Speed = speed;
            rayManager.AnimateSequentially = sequentialAnimate;
            rayManager.HideNoHitRays = hideNoHitRays;
            rayManager.Reset = true;
            mesh = gameObject.GetComponent<RTMesh>();
            update = axis.magnitude != 0;
        }

        /// <summary>
        /// If the object should be rotated rotate it by a given amount in a given direction.
        /// </summary>
        private void FixedUpdate()
        {
            if (update) mesh.Rotation += axis * rotationSpeed;
        }

        /// <summary>
        /// If the object should be rotated, set the transform.hasChanged to true.
        /// Because of the OnEnable, this script has to run after default time and RTMesh checks
        /// transform.hasChanged before the rotation. Set is back to true to let in update the next frame.
        /// </summary>
        private void Update()
        {
            if (update) mesh.transform.hasChanged = true;
        }
    }
}