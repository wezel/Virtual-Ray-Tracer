using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOctree : MonoBehaviour
{

    Octree ot;
    public float nodeMinSize = 0.25f;

    
    void Start()
    {
        ot = new Octree(this.gameObject, nodeMinSize);
        Draw(ot.rootNode);
    }

    void Update()
    {
        Draw(ot.rootNode);
        // Recalculate octree if transformed
        if (transform.hasChanged)
        {
            ot = new Octree(this.gameObject, nodeMinSize);
        }
    }

    public void Draw(OctreeNode node)
    {
        // Draw octree ot using Popcron Gizmos package
        Popcron.Gizmos.Bounds(node.nodeBounds, Color.green);
        if (node.children != null)
        {
            for (int i = 0; i < 8; i++)
            {
                if (node.children[i] != null)
                    Draw(node.children[i]);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Draw using debugging Gizmos (not available in build)
            //ot.rootNode.Draw();
        }
    }
}
