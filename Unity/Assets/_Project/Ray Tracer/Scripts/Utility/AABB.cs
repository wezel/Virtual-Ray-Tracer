using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Utility
{

    public class AABB : MonoBehaviour
    {

        private MeshRenderer meshRenderer = null;
        public Bounds bounds;
        public Vector3 hitpoint;
        public bool drawHitpoint = false;
        private bool drawAABB = false;
        GameObject hitpointSphere = null;
        

        void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            
            bounds = meshRenderer.bounds;
        }

        void Update()
        {
            bounds = meshRenderer.bounds;

            if (!drawAABB)
            {
                if (hitpointSphere != null) Destroy(hitpointSphere);
                return;
            }
            Popcron.Gizmos.Bounds(bounds, Color.green);

            if (drawHitpoint)
            {
                if (hitpointSphere == null)
                {
                    hitpointSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Renderer renderer = hitpointSphere.GetComponent<Renderer>();
                    Material material = new Material(Shader.Find("Diffuse"));
                    material.color = Color.green;
                    renderer.material = material;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

                hitpointSphere.transform.position = hitpoint;
                hitpointSphere.transform.localScale = 0.05f * Vector3.one;
            }
            else
            {
                if (hitpointSphere != null) Destroy(hitpointSphere);
            }
            
        }

        public void showAABBToggle()
        {
            drawAABB = !drawAABB;
        }
    }
}
