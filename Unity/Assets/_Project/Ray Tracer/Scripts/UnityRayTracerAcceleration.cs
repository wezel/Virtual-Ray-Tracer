using System;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using TMPro;

namespace _Project.Ray_Tracer.Scripts
{
    public partial class UnityRayTracer
    {
        [SerializeField]
        public GameObject acceleratedObject;
        /// <summary>
        /// The game object that is used in the AABB and OctreeRoot levels.
        /// </summary>
        /// 
        [SerializeField]
        public TextMeshProUGUI accelerationStatus;
        /// <summary>
        /// Text to display acceleration status
        /// </summary>
        ///

        [SerializeField]
        public TextMeshProUGUI raysIgnored;
        /// <summary>
        /// Text to display amount of rays ignored
        /// </summary>
        ///

        [SerializeField]
        public TextMeshProUGUI trianglesIgnored;
        /// <summary>
        /// Text to display amount of hit tests saved
        /// </summary>
        /// 

        [SerializeField]
        int savedAmount = 0;
        /// <summary>
        /// Number of hit tests saved / rays ignored 
        /// </summary>
        /// 

        [SerializeField]
        int totalTriangles = 0;

        /// <summary>
        /// Number of total triangles in mesh
        /// </summary>
        ///

        public enum TraceMode
        {
            Normal,
            AABB,
            Octree
        }
        
        
        private int trianglesTests = 0;
        private bool octreeStatusFlag = false;
        private int trianglesNotIgnored = 0;

        private Func<Vector3, Vector3, int, RTRay.RayType, TreeNode<RTRay>> traceFunc;
        private Func<Vector3, Vector3, int, Color> imageTraceFunc;
        private TraceMode mode;


        private void SetTraceMode(TraceMode mode)
        {
            this.mode = mode;
            // TODO we ignore the image tracer for now because it is not that important and takes more work to integrate (like this)
            switch (mode)
            {
                case TraceMode.Normal:
                {
                    traceFunc = Trace;
                    imageTraceFunc = TraceImage;
                    break;
                }
                case TraceMode.AABB:
                {
                    traceFunc = TraceAABB;
                    imageTraceFunc = TraceImageAABB;
                    break;
                }
                case TraceMode.Octree:
                {
                    traceFunc = TraceOctree;
                    imageTraceFunc = TraceImageOctree;
                    break;
                }

            }

        }

        private void AccelerationPrep()
        {
            trianglesNotIgnored = 0;
            octreeStatusFlag = false;
            savedAmount = 0;
        }

        private void AccelerationCleanup()
        {
            // TODO this should be data not strings 
            if (mode == TraceMode.Octree)
                trianglesIgnored.text = (totalTriangles - trianglesNotIgnored).ToString() + " triangles ignored\n" + trianglesNotIgnored.ToString() + " triangles ray traced";

            if(mode == TraceMode.AABB)
                raysIgnored.text = savedAmount.ToString();
        }

        private TreeNode<RTRay> TraceAABB(Vector3 origin, Vector3 direction, int depth, RTRay.RayType type)
        {
            RaycastHit hit;
            
            TreeNode<RTRay> rayTree = new TreeNode<RTRay>(new RTRay());
            
            // todo make this function for "all" objects
            AABB aabb = acceleratedObject.GetComponent<AABB>();
            aabb.drawHitpoint = true;
            Bounds bounds = aabb.bounds;
            float distance;
            Ray raycast = new Ray(origin, direction);
            if (!bounds.IntersectRay(raycast, out distance))
            {
                accelerationStatus.color = Color.green;
                accelerationStatus.text = "Ray misses AABB! We accelerate by ignoring this ray";
                aabb.drawHitpoint = false; // Tell AABB script to not draw the hit point
                rayTree.Data = new RTRay(origin, direction, Mathf.Infinity, BackgroundColor, RTRay.RayType.NoHit);
                return rayTree;
            }


            
            // Hit AABB, so check for intersection with object itself
            bool intersected = Physics.Raycast(origin, direction, out hit, Mathf.Infinity, rayTracerLayer);
            
            Vector3 point = raycast.origin + raycast.direction * distance;
            // Tell AABB script to draw the hit point.
            aabb.hitpoint = point;
            aabb.drawHitpoint = true;

            // If we did not hit the object itself we return a no hit ray whose result color is black.
            
            if (!intersected)
            {
                accelerationStatus.color = Color.red;
                accelerationStatus.text = "Ray hits AABB but misses object! Ray is not ignored.";
                rayTree.Data = new RTRay(origin, direction, Mathf.Infinity, BackgroundColor, RTRay.RayType.NoHit);
                return rayTree;
            } else
            {
                accelerationStatus.color = Color.red;
                accelerationStatus.text = "Ray hits AABB and the object! Ray is not ignored.";
            }

            RTMesh mesh = hit.transform.GetComponent<RTMesh>();
            HitInfo hitInfo = new HitInfo(ref hit, ref direction, ref mesh);

            // Add the ambient component once, regardless of the number of lights.
            Color color = hitInfo.Ambient * hitInfo.Color;

            // Add diffuse and specular components.
            foreach (RTLight light in scene.PointLights)
            {
                Vector3 lightVector = (light.transform.position - hit.point).normalized;

                if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                    rayTree.AddChild(TraceLight(ref lightVector, light.transform.position, light, in hitInfo));
            }

            // Cast reflection and refraction rays.
            if (depth > 0)
            {
                var newRays = TraceReflectionAndRefraction(depth, hitInfo);
                foreach (var ray in newRays)
                    rayTree.AddChild(ray);
            }

            // Add the child ray colors to the parent ray.
            foreach (var child in rayTree.Children)
                color += child.Data.Color;

            rayTree.Data = new RTRay(origin, direction, hit.distance, ClampColor(color), type);
            return rayTree;
        }
        
        private void IntersectOctreeNode(OctreeNode node, Ray ray)
        {
            // Assume we never hit a node with triangles
            
            if (node.children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (node.children[i] != null)
                    {
                        if (node.children[i].nodeBounds.IntersectRay(ray))
                        {
                            if (node.children[i].containedTriangles.Count != 0)
                            {
                                trianglesTests += node.children[i].containedTriangles.Count;
                                octreeStatusFlag = true; // hit a node with triangles, so set flag to true
                                trianglesNotIgnored += node.children[i].containedTriangles.Count; // Keep track of non-ignored triangles
                                // node.children[i].containedTriangles should be ray traced. With Physics.Raycast this is not feasible to filter.
                                // For further work, this is where the acceleration of focusing only on these triangles in the mesh should occur.
                                // Workarounds:
                                // Mesh could be edited to only contain these triangles, then reverted back to original after rendering
                                // A new RTMesh could be created, and the original RTMesh could be disabled while ray tracing
                                // Physics.Raycast could be replaced with manual ray cast calculations between ray - triangles
                            }
                            IntersectOctreeNode(node.children[i], ray);
                        }
                    }
                }
            }
        }
        
        private TreeNode<RTRay> TraceOctree(Vector3 origin, Vector3 direction, int depth, RTRay.RayType type)
        {
            RaycastHit hit;

            TreeNode<RTRay> rayTree = new TreeNode<RTRay>(new RTRay());
            
            // todo make this function for "all" objects
            OctreeNode ot = acceleratedObject.GetComponent<Octree>().octreeRoot.rootNode;
            
            float distance;
            Ray raycast = new Ray(origin, direction);

            // Check root node intersection
            if (!ot.nodeBounds.IntersectRay(raycast, out distance))
            {
                accelerationStatus.color = Color.green;
                accelerationStatus.text = "Ray misses Octree! We can safely ignore this ray to accelerate.";
                rayTree.Data = new RTRay(origin, direction, Mathf.Infinity, BackgroundColor, RTRay.RayType.NoHit);
                return rayTree;
            } else
            {
                // Hit octree root, so check childs. If at any point a node that contains triangles is hit, we do not ignore this ray.
                if (ot.children != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (ot.children[i] != null)
                            IntersectOctreeNode(ot.children[i], raycast);
                    }
                }
            }

            accelerationStatus.color = Color.green;
            // If flag is false, it means we never hit a node with triangles, so we can ignore ray to accelerate :(
            if (!octreeStatusFlag)
            {
                accelerationStatus.text = "Ray does not hit any node with triangles! We can ignore it.";
                rayTree.Data = new RTRay(origin, direction, Mathf.Infinity, BackgroundColor, RTRay.RayType.NoHit);
                return rayTree;
            } else
            {
                accelerationStatus.text = "Ray hits node(s) with triangles in it! We can not ignore it.";
            }
            
            // Hit OctreeRoot, so check for intersection with object itself
            bool intersected = Physics.Raycast(origin, direction, out hit, Mathf.Infinity, rayTracerLayer);

            if (!intersected)
            {
                accelerationStatus.text = "Ray hits node(s) with triangles in it but misses mesh! Still accelerated because we only need to check for the triangles within the node(s).";
                rayTree.Data = new RTRay(origin, direction, Mathf.Infinity, BackgroundColor, RTRay.RayType.NoHit);
                return rayTree;
            }
            else
            {
                accelerationStatus.text = "Ray hits Octree and the triangles in a node! Accelerated!";
            }

            RTMesh mesh = hit.transform.GetComponent<RTMesh>();
            HitInfo hitInfo = new HitInfo(ref hit, ref direction, ref mesh);

            // Add the ambient component once, regardless of the number of lights.
            Color color = hitInfo.Ambient * hitInfo.Color;

            // Add diffuse and specular components.
            foreach (RTLight light in scene.PointLights)
            {
                Vector3 lightVector = (light.transform.position - hit.point).normalized;

                if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                    rayTree.AddChild(TraceLight(ref lightVector, light.transform.position, light, in hitInfo));
            }

            // Cast reflection and refraction rays.
            if (depth > 0)
            {
                var newRays = TraceReflectionAndRefraction(depth, hitInfo);
                foreach (var ray in newRays)
                    rayTree.AddChild(ray);
            }

            // Add the child ray colors to the parent ray.
            foreach (var child in rayTree.Children)
                color += child.Data.Color;

            rayTree.Data = new RTRay(origin, direction, hit.distance, ClampColor(color), type);
            return rayTree;
        }
        
        private Color TraceImageAABB(Vector3 origin, Vector3 direction, int depth)
        {
            RaycastHit hit;
            
            // todo make this function for "all" objects
            AABB aabb = acceleratedObject.GetComponent<AABB>();
            aabb.drawHitpoint = true;
            Bounds bounds = aabb.bounds;
            float distance;
            Ray raycast = new Ray(origin, direction);

            // If AABB is missed, accelerate by stopping here (returning background color
            if (!bounds.IntersectRay(raycast, out distance))
            {
                savedAmount++;
                return BackgroundColor;
            }

           

            bool intersected = Physics.Raycast(origin, direction, out hit, Mathf.Infinity, rayTracerLayer);

            // If we did not hit anything we return the background color.
            if (!intersected)
            {
                
                return BackgroundColor;
            }

            RTMesh mesh = hit.transform.GetComponent<RTMesh>();
            HitInfo hitInfo = new HitInfo(ref hit, ref direction, ref mesh);

            // Add the ambient component once, regardless of the number of lights.
            Color color = hitInfo.Ambient * hitInfo.Color;

            // Add diffuse and specular components.
            foreach (RTLight light in scene.PointLights)
            {
                Vector3 lightVector = (light.transform.position - hit.point).normalized;

                if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                    color += TraceLightImage(ref lightVector, light.transform.position, light, in hitInfo);
            }

            // Cast reflection and refraction rays.
            if (depth > 0)
                color += TraceReflectionAndRefractionImage(depth, hitInfo);

            return ClampColor(color);
        }
        
         private Color TraceImageOctree(Vector3 origin, Vector3 direction, int depth)
        {
            RaycastHit hit;
            // todo make this function for "all" objects
            OctreeNode ot = acceleratedObject.GetComponent<Octree>().octreeRoot.rootNode;

            Ray raycast = new Ray(origin, direction);

            // Check root node intersection
            if (!ot.nodeBounds.IntersectRay(raycast))
            {
                savedAmount++;
                return BackgroundColor;
            }
            else
            {
                // Hit octree root, so check childs. If at any point a node that contains triangles is hit, we do not ignore this ray.
                if (ot.children != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (ot.children[i] != null)
                            IntersectOctreeNode(ot.children[i], raycast);
                    }
                }
            }

            if (!octreeStatusFlag)
            {
                savedAmount++;
                return BackgroundColor;
            }
       
            bool intersected = Physics.Raycast(origin, direction, out hit, Mathf.Infinity, rayTracerLayer);

            // If we did not hit anything we return the background color.
            if (!intersected)
            {
                return BackgroundColor;
            }

            RTMesh mesh = hit.transform.GetComponent<RTMesh>();
            HitInfo hitInfo = new HitInfo(ref hit, ref direction, ref mesh);

            // Add the ambient component once, regardless of the number of lights.
            Color color = hitInfo.Ambient * hitInfo.Color;

            // Add diffuse and specular components.
            foreach (RTLight light in scene.PointLights)
            {
                Vector3 lightVector = (light.transform.position - hit.point).normalized;

                if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                    color += TraceLightImage(ref lightVector, light.transform.position, light, in hitInfo);
            }

            // Cast reflection and refraction rays.
            if (depth > 0)
                color += TraceReflectionAndRefractionImage(depth, hitInfo);

            return ClampColor(color);
        }

         private void AccelerationAwake()
         {
             SetTraceMode(TraceMode.Normal);
         }
    }
}
