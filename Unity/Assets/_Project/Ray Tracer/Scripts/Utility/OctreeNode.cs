using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Utility
{
    /// <summary>
    /// Triangle struct to keep track of which triangle(s) in the mesh this node contains
    /// The vertices are in world space coordinates
    /// </summary>
    public struct Triangle
    {
        public Vector3 v1, v2, v3;
        public int index;

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, int index)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.index = index;
        }
    }

    /// <summary>
    /// An Octree Node in the Octree structure
    /// </summary>
    public class OctreeNode
    {
        int currentDepth;

        /// <summary>
        /// Current subdivision depth at this node
        /// </summary>

        public Bounds nodeBounds;

        /// <summary>
        /// Bounds of current node
        /// </summary>

        private Bounds[] childBounds;

        /// <summary>
        /// Bounds array of the (potential) children of this node
        /// </summary>

        public OctreeNode[] children = null;

        /// <summary>
        /// Octree Node array for he children nodes of this node 
        /// </summary>


        public List<Triangle> containedTriangles = new List<Triangle>();

        /// <summary>
        /// Triangles list for the triangles of the mesh contained by this node
        /// </summary>

        public OctreeNode(Bounds b, int depth)
        {
            nodeBounds = b;
            currentDepth = depth;

            // Split into children
            float quarter = nodeBounds.size.y / 4f;
            float childLength = nodeBounds.size.y / 2f;
            Vector3 childSize = new Vector3(childLength, childLength, childLength);
            childBounds = new Bounds[8];
            childBounds[0] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, quarter), childSize);
            childBounds[1] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, -quarter), childSize);
            childBounds[2] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, quarter), childSize);
            childBounds[3] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, quarter), childSize);
            childBounds[4] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, -quarter), childSize);
            childBounds[5] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, -quarter), childSize);
            childBounds[6] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, quarter), childSize);
            childBounds[7] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, -quarter), childSize);

        }

        /// <summary>
        /// Subdivide this node and add the triangle to it selectively
        /// The triangle is added to this current node if it's children do not contain it.
        /// </summary>
        /// <param name="t"> Triangle to be added </param>
        public void DivideAndAdd(Triangle t)
        {
            // If max depth reached add the triangle and stop subdivision
            if (currentDepth == 0)
            {
                containedTriangles.Add(new Triangle(t.v1, t.v2, t.v3, t.index));
                return;
            }

            // If children does not exist, instantiate it.
            if (children == null)
                children = new OctreeNode[8];

            // Flag to control subdivision
            bool dividing = false;

            // For each children
            for (int i = 0; i < 8; i++)
            {
                // If children does not already exist, instantiate its node
                if (children[i] == null)
                    children[i] = new OctreeNode(childBounds[i], currentDepth - 1);

                // Check if triangle is included in child
                if (childBounds[i].Contains(t.v1) || childBounds[i].Contains(t.v2) || childBounds[i].Contains(t.v3))
                {
                    // If so, keep dividing and call subdivision with this triangle on the child
                    // See the if statement after this for loop 
                    dividing = true;

                    // Call subdivision on this child that contains the triangle
                    children[i].DivideAndAdd(t);
                }
            }

            // If not subdiving, add the triangle to this current node
            if (!dividing)
                containedTriangles.Add(t);

        }
    }
}