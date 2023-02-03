using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Utility
{
    public class OctreeRoot
    {
        public OctreeNode rootNode;

        public Bounds bounds;


        public OctreeRoot(GameObject obj, int maxDepth)
        {
            // Get the axis aligned bounding box from mesh renderer component
            bounds = obj.GetComponent<MeshRenderer>().bounds;

            // Force the bounding box into a cube (equal sides) by getting the longest side
            float maxSize = Mathf.Max(new float[] {bounds.size.x, bounds.size.y, bounds.size.z});
            Vector3 sizeVector = new Vector3(maxSize, maxSize, maxSize) * 0.5f;
            //Debug.Log("Center: " + bounds.center + ", Size: " + bounds.size);
            bounds.SetMinMax(bounds.center - sizeVector, bounds.center + sizeVector);


            rootNode = new OctreeNode(bounds, maxDepth);
            AddTriangles(obj);
        }

        public void AddTriangles(GameObject obj)
        {
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < triangles.Length;)
            {
                // Derive the triangle (in world space) and add it

                // index i, i+1 and i+2 is the triangle vertices
                Matrix4x4 localToWorld = obj.transform.localToWorldMatrix;
                Vector3 v1 = localToWorld.MultiplyPoint3x4(vertices[triangles[i]]);
                Vector3 v2 = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 1]]);
                Vector3 v3 = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 2]]);
                Triangle t = new Triangle(v1, v2, v3, i);
                rootNode.DivideAndAdd(t);

                i += 3; // move to next triangle
            }
        }
    }
}