using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera
{
    /// <summary>
    /// Generates a <see cref="MeshCollider"/> for an <see cref="RTCamera"/> which can be used to select the camera.
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    public class CameraCollisionMesh : MonoBehaviour
    {
        private Mesh mesh;
        private new MeshCollider collider; // Hides Component.collider which is obsolete.

        private Vector3[] vertices;
        private int[] triangles;

        /// <summary>
        /// Recalculate the mesh and mesh collider based on the world space vertex coordinates of the camera frustum. Make
        /// sure the corner vertices are provided in a circular order, so no jumping along the diagonal.
        /// </summary>
        /// <param name="origin"> The camera origin. The apex of the frustum pyramid. </param>
        /// <param name="corner1"> The first corner of the base of the frustum pyramid. </param>
        /// <param name="corner2"> The second corner of the base of the frustum pyramid. </param>
        /// <param name="corner3"> The third corner of the base of the frustum pyramid. </param>
        /// <param name="corner4"> The fourth corner of the base of the frustum pyramid. </param>
        public void RecalculateMesh(Vector3 origin, Vector3 corner1, Vector3 corner2, Vector3 corner3, Vector3 corner4)
        {
            // The mesh expects local vertex coordinates.
            Vector3 localOrigin = transform.InverseTransformPoint(origin);
            Vector3 localCorner1 = transform.InverseTransformPoint(corner1);
            Vector3 localCorner2 = transform.InverseTransformPoint(corner2);
            Vector3 localCorner3 = transform.InverseTransformPoint(corner3);
            Vector3 localCorner4 = transform.InverseTransformPoint(corner4);

            vertices = new Vector3[5]{ localOrigin, localCorner1, localCorner2, localCorner3, localCorner4 };
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            // For some reason the collider needs to be toggled enabled in order to work.
            collider.enabled = false;
            collider.enabled = true;
        }

        private void Awake()
        {
            mesh = new Mesh();
            collider = GetComponent<MeshCollider>();
            collider.sharedMesh = mesh;

            // The indices stay the same throughout, so we set them here.
            triangles = new int[]
            {
                0, 1, 4, // Side 1
                0, 2, 1, // Side 2
                0, 3, 2, // Side 3
                0, 4, 3, // Side 4
                1, 2, 3, // Base 1
                1, 3, 4  // Base 2
            };
        }
    }
}
