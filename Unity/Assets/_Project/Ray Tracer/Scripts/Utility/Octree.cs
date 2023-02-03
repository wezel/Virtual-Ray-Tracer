using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Utility
{

    /// <summary>
    /// An Octree component for an object who has MeshFilter and MeshRenderer components.
    /// MonoBehaviour enabled in order to draw it in the scene
    /// </summary>
    public class Octree : MonoBehaviour
    {

        public OctreeRoot octreeRoot;

        /// <summary>
        /// Root node of this Octree
        /// </summary>

        public int maxDepth = 3;

        /// <summary>
        /// Max subdivision depth of this Octree
        /// 3-4 works well overall
        /// </summary>

        private bool drawOctree = false;

        /// <summary>
        /// Boolean flag for drawing or hiding the Octree from UI
        /// </summary>
        /// 

        void Start()
        {
            // Initalize the Octree and draw
            octreeRoot = new OctreeRoot(this.gameObject, maxDepth);
            Draw(octreeRoot.rootNode);
        }

        /// <summary>
        /// MonoBehaviour update function which runs every frame
        /// We make sure to not recalculate the Octree each frame when not needed
        /// </summary>
        void Update()
        {
            // If drawing is not enabled, return
            if (!drawOctree) return;

            Draw(octreeRoot.rootNode);

            // Recalculate octree if transformed
            octreeRoot = new OctreeRoot(gameObject, maxDepth);

        }

        /// <summary>
        /// Recursively draws an Octree given any node (root or any child)
        /// When the root node is passed, it will draw the whole Octree
        /// </summary>
        /// <param name="node"></param>
        public void Draw(OctreeNode node)
        {
            // Draw the current nodes Bounds using Popcron Gizmos package
            // node: current node
            // nodeBounds: Bounds of the node being drawn
            Popcron.Gizmos.Bounds(node.nodeBounds, Color.green);

            // Recursively call Draw() on children
            if (node.children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (node.children[i] != null)
                        Draw(node.children[i]);
                }
            }
        }

        /// <summary>
        /// Used to toggle the drawOctree boolean from the UI
        /// </summary>
        /// <param name="tog"></param>
        public void showOctreeToggle()
        {
            drawOctree = !drawOctree;
        }

        /// <summary>
        /// Used to change depth of the Octree
        /// </summary>
        /// <param name="tog"></param>
        public void changeOctreeDepthSlider(float val)
        {
            maxDepth = (int) val;
            octreeRoot = new OctreeRoot(this.gameObject, maxDepth);
        }
    }
}

