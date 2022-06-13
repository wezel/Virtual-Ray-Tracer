using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Triangle struct to keep track of which triangle(s) in the mesh this node contains
// Not sure how to make use of it yet
public struct Triangle
{
    public Vector3 v1, v2, v3;

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }
}

public class OctreeNode
{
    public Bounds nodeBounds;    // Bounds of the current node
    Bounds[] childBounds; // Bounds used to split into more cubes
    public OctreeNode[] children = null;
    float minSize;
    List<Triangle> containedTriangles = new List<Triangle>();

    public OctreeNode(Bounds b, float minNodeSize)
    {
        nodeBounds = b;
        minSize = minNodeSize;

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

    public void AddTriangle(Triangle t)
    {
        DivideAndAdd(t);
    }

    public void DivideAndAdd(Triangle t)
    {
        
        if (nodeBounds.size.y <= minSize)
        {
            containedTriangles.Add(new Triangle(t.v1, t.v2, t.v3));
            return; // Stop dividing octree
        }
        if (children == null)
            children = new OctreeNode[8];
        bool dividing = false;
        for(int i = 0; i < 8; i++)
        {
            // If children does not already exist, instantiate its octree (node)
            if (children[i] == null)
                children[i] = new OctreeNode(childBounds[i], minSize);
            // Check if triangle is included in child
            
            if (childBounds[i].Contains(t.v1) || childBounds[i].Contains(t.v2) || childBounds[i].Contains(t.v3))
            {
                dividing = true;
                children[i].DivideAndAdd(t);
            }
        }

        if (dividing == false)
        {
            containedTriangles.Add(t);
        }
    }

    public void Draw()
    {
        Gizmos.color = new Color(0, 1, 0);
        Gizmos.DrawWireCube(nodeBounds.center, nodeBounds.size);

        
        
        // Draw children
        if (children != null)
        {
            for(int i = 0; i < 8; i++)
            {
                if (children[i] != null)
                    children[i].Draw();
            }
        }
    }
}
