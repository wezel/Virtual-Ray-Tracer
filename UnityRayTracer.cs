using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using _Project.Ray_Tracer.Scripts.Utility;
using TMPro;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts
{
    /// <summary>
    /// A simple ray tracer that can render <see cref="RTScene"/> objects. The <see cref="Render"/> function is not
    /// efficient as it stores all rays it traces in a list of ray trees. Therefore, this function should only be used
    /// to produce a relatively small number of rays for the <see cref="RayManager"/> to visualize. For larger images
    /// (and no ray trees) the <see cref="RenderImage"/> function should be used.
    /// </summary>
    public class UnityRayTracer : MonoBehaviour
    {
        public delegate void RayTracerChanged();
        /// <summary>
        /// An event invoked whenever a property of this ray tracer is changed.
        /// </summary>
        public event RayTracerChanged OnRayTracerChanged;

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

        [SerializeField]
        private float epsilon = 0.001f;
        /// <summary>
        /// A small floating point value used to prevent shadow acne.
        /// </summary>
        /// 

        private int trianglesTests = 0;

        public float Epsilon
        {
            get { return epsilon; }
            set 
            {
                epsilon = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField]
        private bool renderShadows = true;
        /// <summary>
        /// Whether this ray tracer renders shadows.
        /// </summary>
        public bool RenderShadows
        {
            get { return renderShadows; }
            set
            {
                renderShadows = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField]
        private int maxDepth = 3;
        /// <summary>
        /// The maximum depth of any ray tree produced by this ray tracer.
        /// </summary>
        public int MaxDepth
        {
            get { return maxDepth; }
            set
            {
                maxDepth = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField]
        private int superSamplingFactor = 1;
        /// <summary>
        /// The supersampling factor.
        /// </summary>
        public int SuperSamplingFactor
        {
            get { return superSamplingFactor; }
            set
            {
                superSamplingFactor = value;
                OnRayTracerChanged?.Invoke();
            }
        }

        [SerializeField]
        private Color backgroundColor = Color.black;
        /// <summary>
        /// The color produced by rays that don't intersect an object.
        /// </summary>
        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                backgroundColor = value;
                Camera.main.backgroundColor = backgroundColor;
                OnRayTracerChanged?.Invoke();
            }
        }

        static private UnityRayTracer instance = null;
        private RTSceneManager rtSceneManager;

        private RTScene scene;
        private new RTCamera camera;

        private int rayTracerLayer;

        /// <summary>
        /// A class that stores the raw mesh data of a collider. This is used to cache a list of recently intersected
        /// meshes.
        /// </summary>
        private class MeshData
        {
            public Collider Collider;
            public Vector3[] Normals;
            public int[] Indices;
        }

        private static int cacheCapacity = 8;
        private static List<MeshData> meshCache = new List<MeshData>(cacheCapacity);

        /// <summary>
        /// A struct that calculates and stores all relevant information about a ray-object intersection.
        /// </summary>
        private readonly struct HitInfo 
        {
            public readonly Vector3 Point;
            public readonly Vector3 View ;
            public readonly Vector3 Normal;
            public readonly bool InversedNormal;

            public readonly Color Color;
            public readonly float Ambient;
            public readonly float Diffuse;
            public readonly float Specular;
            public readonly float Shininess;
            public readonly float RefractiveIndex;
            public readonly bool IsTransparent;

            public HitInfo(ref RaycastHit hit, ref Vector3 direction, ref RTMesh mesh)
            {
                Point = hit.point;
                View = -direction;
                Normal = hit.normal;
                InversedNormal = false;

                // Get the material's properties.
                Color = mesh.Color;
                Ambient = mesh.Ambient;
                Diffuse = mesh.Diffuse;
                Specular = mesh.Specular;
                Shininess = mesh.Shininess;
                RefractiveIndex = mesh.RefractiveIndex;
                IsTransparent = mesh.Type == RTMesh.ObjectType.Transparent;

                // Interpolate the hit normal to achieve smooth shading.
                if (mesh.ShadeSmooth)
                    Normal = SmoothedNormal(ref hit);
                
                // The shading normal always points in the direction of the view, as required by the Phong illumination
                // model.
                InversedNormal = Vector3.Dot(Normal, View) < -0.1f;
                Normal = InversedNormal ? -Normal : Normal;
            }
            
            private static Vector3 SmoothedNormal(ref RaycastHit hit)
            {
                // See if we have this mesh cached.
                MeshCollider meshCollider = hit.collider as MeshCollider;
                MeshData cachedMesh = meshCache.Find(data => data.Collider == meshCollider);

                // If not, we add it to the cache.
                if (cachedMesh == null)
                {
                    Mesh mesh = meshCollider.sharedMesh;
                    cachedMesh = new MeshData();
                    cachedMesh.Collider = meshCollider;
                    cachedMesh.Normals = mesh.normals;
                    cachedMesh.Indices = mesh.triangles;
                    meshCache.Add(cachedMesh);
                }

                // Prevent excess memory use by limiting the cache capacity.
                while (meshCache.Count > cacheCapacity)
                    meshCache.RemoveAt(0);

                // Extract local space normals of the triangle we hit.
                Vector3 n0 = cachedMesh.Normals[cachedMesh.Indices[hit.triangleIndex * 3 + 0]];
                Vector3 n1 = cachedMesh.Normals[cachedMesh.Indices[hit.triangleIndex * 3 + 1]];
                Vector3 n2 = cachedMesh.Normals[cachedMesh.Indices[hit.triangleIndex * 3 + 2]];

                // Interpolate the normal using the barycentric coordinate of the hit point.
                Vector3 baryCenter = hit.barycentricCoordinate;
                Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
                interpolatedNormal = interpolatedNormal.normalized;

                // Transform local space normals to world space.
                Transform hitTransform = hit.collider.transform;
                interpolatedNormal = hitTransform.TransformDirection(interpolatedNormal);

                return interpolatedNormal;
            }
        }

        /// <summary>
        /// Get the current <see cref="UnityRayTracer"/> instance.
        /// </summary>
        /// <returns> The current <see cref="UnityRayTracer"/> instance. </returns>
        public static UnityRayTracer Get()
        {
            return instance;
        }

        /// <summary>
        /// Render the current <see cref="RTSceneManager"/>'s <see cref="RTScene"/> while building up a list of ray trees.
        /// </summary>
        /// <returns> The list of ray trees that were traced to render the image. </returns>
        public List<TreeNode<RTRay>> Render()
        {
            bool accelerationAABB = false;
            AABB aabb = null;
            bool accelerationOctree = false;
            Octree createOctree = null;
            if (acceleratedObject)
            {
                if (aabb = acceleratedObject.GetComponent<AABB>())
                {
                    accelerationAABB = true;
                }

                if (createOctree = acceleratedObject.GetComponent<Octree>())
                {
                    accelerationOctree = true;
                }
            }
            trianglesNotIgnored = 0;

            List<TreeNode<RTRay>> rayTrees = new List<TreeNode<RTRay>>();
            scene = rtSceneManager.Scene;
            camera = scene.Camera;

            int width = camera.ScreenWidth;
            int height = camera.ScreenHeight;
            float aspectRatio = (float) width / height;
            float halfScreenHeight = camera.ScreenDistance * Mathf.Tan(Mathf.Deg2Rad * camera.FieldOfView / 2.0f);
            float halfScreenWidth = aspectRatio * halfScreenHeight;
            float pixelWidth = halfScreenWidth * 2.0f / width;
            float pixelHeight = halfScreenHeight * 2.0f / height;
            Vector3 origin = camera.transform.position;

            // Trace a ray for each pixel. 
            
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    // Convert the pixel coordinates to camera space positions.
                    float pixelX = -halfScreenWidth + pixelWidth * (x + 0.5f);
                    float pixelY = -halfScreenHeight + pixelHeight * (y + 0.5f);

                    // Create and rotate the pixel location. Note that the camera looks along the positive z-axis.
                    Vector3 pixel = new Vector3(pixelX, pixelY, camera.ScreenDistance);
                    pixel = camera.transform.rotation * pixel;

                    // This is the distance between the pixel on the screen and the origin. We need this to compensate
                    // for the length of the returned RTRay. Since we have this factor we also use it to normalize this
                    // vector to make the code more efficient.
                    float pixelDistance = pixel.magnitude;
                    
                    // Compensate for the location of the screen so we don't render objects that are behind the screen.
                    TreeNode<RTRay> rayTree;
                    if (acceleratedObject)
                    {
                        if (accelerationAABB)
                        {
                            rayTree = TraceAABB(origin + pixel, pixel / pixelDistance, MaxDepth, RTRay.RayType.Normal); // AABB
                        } else if (accelerationOctree)
                        {
                            rayTree = TraceOctree(origin + pixel, pixel / pixelDistance, MaxDepth, RTRay.RayType.Normal); // OctreeRoot
                        } else
                        {
                            rayTree = Trace(origin + pixel, pixel / pixelDistance, MaxDepth, RTRay.RayType.Normal); // Default
                        }

                    } else rayTree = Trace(origin + pixel,
                                    pixel / pixelDistance, // Division by magnitude == .normalized.
                                    MaxDepth, RTRay.RayType.Normal);
                    
                    // Fix the origin and the length so we visualize the right ray.
                    rayTree.Data.Origin = origin;
                    rayTree.Data.Length += pixelDistance; 
                    rayTrees.Add(rayTree);
                }
            }
            
            if (accelerationOctree)
                trianglesIgnored.text = (totalTriangles - trianglesNotIgnored).ToString() + " triangles ignored\n" + trianglesNotIgnored.ToString() + " triangles ray traced";
            return rayTrees;
        }

        private TreeNode<RTRay> TraceAABB(Vector3 origin, Vector3 direction, int depth, RTRay.RayType type)
        {
            RaycastHit hit;
            
            TreeNode<RTRay> rayTree = new TreeNode<RTRay>(new RTRay());
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
            foreach (RTLight light in scene.Lights)
            {
                Vector3 lightVector = (light.transform.position - hit.point).normalized;

                if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                    rayTree.AddChild(TraceLight(ref lightVector, light, in hitInfo));
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


        private bool octreeStatusFlag = false;
        private int trianglesNotIgnored = 0;
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
            return;
        }

      
        private TreeNode<RTRay> TraceOctree(Vector3 origin, Vector3 direction, int depth, RTRay.RayType type)
        {
            RaycastHit hit;

            TreeNode<RTRay> rayTree = new TreeNode<RTRay>(new RTRay());
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
            foreach (RTLight light in scene.Lights)
            {
                Vector3 lightVector = (light.transform.position - hit.point).normalized;

                if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                    rayTree.AddChild(TraceLight(ref lightVector, light, in hitInfo));
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

        private TreeNode<RTRay> Trace(Vector3 origin, Vector3 direction, int depth, RTRay.RayType type)
        {
            RaycastHit hit;

           
            bool intersected = Physics.Raycast(origin, direction, out hit, Mathf.Infinity, rayTracerLayer);
            TreeNode<RTRay> rayTree = new TreeNode<RTRay>(new RTRay());

            // If we did not hit anything we return a no hit ray whose result color is black.
            if (!intersected)
            {
                rayTree.Data = new RTRay(origin, direction, Mathf.Infinity, BackgroundColor, RTRay.RayType.NoHit);
                return rayTree;
            }

            RTMesh mesh = hit.transform.GetComponent<RTMesh>();
            HitInfo hitInfo = new HitInfo(ref hit, ref direction, ref mesh);
            
            // Add the ambient component once, regardless of the number of lights.
            Color color = hitInfo.Ambient * hitInfo.Color;

            // Add diffuse and specular components.
            foreach (RTLight light in scene.Lights)
            {
                Vector3 lightVector = (light.transform.position - hit.point).normalized;

                if (Vector3.Dot(hitInfo.Normal,lightVector) >= 0.0f) 
                    rayTree.AddChild(TraceLight(ref lightVector, light, in hitInfo));
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

        private RTRay TraceLight(ref Vector3 lightVector, RTLight light, in HitInfo hitInfo)
        {
            // Determine the distance to the light source. Note the clever use of the dot product.
            float lightDistance = Vector3.Dot(lightVector, light.transform.position - hitInfo.Point); 

            // If we render shadows, check whether a shadow ray first meets the light or an object.
            if (RenderShadows)
            {
                RaycastHit shadowHit;
                Vector3 shadowOrigin = hitInfo.Point + Epsilon * hitInfo.Normal;
                
                // Trace a ray until we reach the light source. If we hit something return a shadow ray.
                if (Physics.Raycast(shadowOrigin, lightVector, out shadowHit, lightDistance, rayTracerLayer))
                    return new RTRay(hitInfo.Point, lightVector, shadowHit.distance, Color.black,
                        RTRay.RayType.Shadow);
            }

            // We either don't render shadows or nothing is between the object and the light source.
            
            // Calculate the color influence of this light.
            Vector3 reflectionVector = Vector3.Reflect(-lightVector, hitInfo.Normal);
            Color color = light.Ambient * light.Color * hitInfo.Color;
            color += Vector3.Dot(hitInfo.Normal, lightVector) * hitInfo.Diffuse * light.Diffuse *
                     light.Color * hitInfo.Color; // Id
            color += Mathf.Pow(Mathf.Max(Vector3.Dot(reflectionVector, hitInfo.View), 0.0f), hitInfo.Shininess) * 
                     hitInfo.Specular * light.Specular * light.Color; // Is

            return new RTRay(hitInfo.Point, lightVector, lightDistance, ClampColor(color), RTRay.RayType.Light);
        }

        private List<TreeNode<RTRay>> TraceReflectionAndRefraction(int depth, in HitInfo hitInfo)
        {
            List<TreeNode<RTRay>> rays = new List<TreeNode<RTRay>>();
            TreeNode<RTRay> node;

            // The object is transparent, and thus refracts and reflects light.
            if (hitInfo.IsTransparent)
            {
                // Calculate the refractive index.
                float nint = hitInfo.InversedNormal ? hitInfo.RefractiveIndex : 1.0f / hitInfo.RefractiveIndex;

                // Use Schlick's approximation to determine the ratio between refraction and reflection.
                float kr0 = Mathf.Pow((nint - 1.0f) / (nint + 1.0f), 2);
                float kr = kr0 + (1.0f - kr0) * Mathf.Pow(1.0f - Vector3.Dot(hitInfo.Normal, hitInfo.View), 5);
                float kt = 1.0f - kr;

                // Reflect.
                node = Trace(hitInfo.Point + hitInfo.Normal * Epsilon,
                    Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                    depth - 1, RTRay.RayType.Reflect);
                node.Data.Color *= kr;
                rays.Add(node);

                // Refract.
                node = Trace(hitInfo.Point - hitInfo.Normal * Epsilon,
                    Refract(-hitInfo.View, hitInfo.Normal, nint),
                    depth - 1, RTRay.RayType.Refract);
                node.Data.Color *= kt;
                rays.Add(node);

                return rays;
            }

            // The object is not transparent, so we only reflect (provided it has a non zero specular component).
            if (hitInfo.Specular <= 0.0f) return rays;
            
            node = Trace(hitInfo.Point + hitInfo.Normal * Epsilon,
                Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                depth - 1, RTRay.RayType.Reflect);
            node.Data.Color *= hitInfo.Specular;
            rays.Add(node);

            return rays;
        }

        /// <summary>
        /// Render the current <see cref="RTSceneManager"/>'s <see cref="RTScene"/> while building up a "high resolution"
        /// image.
        /// </summary>
        /// <returns> A high resolution render in the form of a <see cref="Texture2D"/>. </returns>
        public Texture2D RenderImage()
        {
            // Avoiding GetComponent<> calls in loops
            // The get components we have to be careful with. If they are done redundantly, it will slow down execution time!
            // Here we get them once and for good before starting the actual accelerated ray tracer rendering.
            bool accelerationAABB = false;
            AABB aabb = null;
            bool accelerationOctree = false;
            Octree createOctree = null;
            OctreeNode octreeRoot = null;
            if (acceleratedObject)
            {
                if (aabb = acceleratedObject.GetComponent<AABB>())
                {
                    accelerationAABB = true;
                }

                if (createOctree = acceleratedObject.GetComponent<Octree>())
                {
                    octreeRoot = createOctree.octreeRoot.rootNode;
                    accelerationOctree = true;
                }
            }
            trianglesTests = 0;
            scene = rtSceneManager.Scene;
            camera = scene.Camera;
            
            int width = camera.ScreenWidth;
            int height = camera.ScreenHeight;
            float aspectRatio = (float) width / height;
            
            // Scale width and height in such a way that the image has around a total of 160,000 pixels.
            int scaleFactor = Mathf.RoundToInt(Mathf.Sqrt(160000f / (width * height)));
            width = scaleFactor * width;
            height = scaleFactor * height;
            
            Texture2D image = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            // Calculate the other variables.
            float halfScreenHeight = camera.ScreenDistance * Mathf.Tan(Mathf.Deg2Rad * camera.FieldOfView / 2.0f);
            float halfScreenWidth = aspectRatio * halfScreenHeight;
            float pixelWidth = halfScreenWidth * 2.0f / width;
            float pixelHeight = halfScreenHeight * 2.0f / height;
            int superSamplingSquared = SuperSamplingFactor * SuperSamplingFactor;
            Vector3 origin = camera.transform.position;

            float start = Time.realtimeSinceStartup;

            octreeStatusFlag = false;
            savedAmount = 0;
          
            // Trace a ray for each pixel.
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    Color color = Color.black;
                    float step = 1f / SuperSamplingFactor;

                    for (int supY = 0; supY < SuperSamplingFactor; supY++)
                    {
                        float difY = pixelHeight * (y + step * (0.5f + supY));

                        for (int supX = 0; supX < SuperSamplingFactor; supX++)
                        {
                            float difX = pixelWidth * (x + step * (0.5f + supX));

                            // Create and rotate the pixel location. Note that the camera looks along the positive z-axis.
                            Vector3 pixel = new Vector3(-halfScreenWidth + difX, -halfScreenHeight + difY, camera.ScreenDistance); 
                            pixel = camera.transform.rotation * pixel;
                            
                            // Compensate for the location of the screen so we don't render objects that are behind the screen.
                            if (accelerationAABB) color += TraceImageAABB(origin + pixel, pixel.normalized, MaxDepth, aabb);
                            else if (accelerationOctree) color += TraceImageOctree(origin + pixel, pixel.normalized, MaxDepth, octreeRoot);
                            else color += TraceImage(origin + pixel, pixel.normalized, MaxDepth);
                        }
                    }

                    // Divide by supersamplingFactor squared and set alpha levels back to 1. It should always be 1!
                    color /= superSamplingSquared;
                    color.a = 1.0f;
                    image.SetPixel(x, y, ClampColor(color));
                }
            }
            Debug.Log("Triangle tests: " + trianglesTests);
            Debug.Log("Time: " + (Time.realtimeSinceStartup - start)); // Time it took to render with ray tracing
            if (accelerationAABB) raysIgnored.text = savedAmount.ToString();
            
            image.Apply(); // Very important.


           
            return image;
        }

        private Color TraceImageAABB(Vector3 origin, Vector3 direction, int depth, AABB aabb)
        {
            RaycastHit hit;
           
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
            foreach (RTLight light in scene.Lights)
            {
                Vector3 lightVector = (light.transform.position - hit.point).normalized;

                if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                    color += TraceLightImage(ref lightVector, light, in hitInfo);
            }

            // Cast reflection and refraction rays.
            if (depth > 0)
                color += TraceReflectionAndRefractionImage(depth, hitInfo);

            return ClampColor(color);
        }

        private Color TraceImageOctree(Vector3 origin, Vector3 direction, int depth, OctreeNode ot)
        {
            RaycastHit hit;

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
            foreach (RTLight light in scene.Lights)
            {
                Vector3 lightVector = (light.transform.position - hit.point).normalized;

                if (Vector3.Dot(hitInfo.Normal, lightVector) >= 0.0f)
                    color += TraceLightImage(ref lightVector, light, in hitInfo);
            }

            // Cast reflection and refraction rays.
            if (depth > 0)
                color += TraceReflectionAndRefractionImage(depth, hitInfo);

            return ClampColor(color);
        }

        private Color TraceImage(Vector3 origin, Vector3 direction, int depth)
        {
            RaycastHit hit;

            bool intersected = Physics.Raycast(origin, direction, out hit, Mathf.Infinity, rayTracerLayer);

            // If we did not hit anything we return the background color.
            if (!intersected) return BackgroundColor;

            RTMesh mesh = hit.transform.GetComponent<RTMesh>();
            HitInfo hitInfo = new HitInfo(ref hit, ref direction, ref mesh);
            
            // Add the ambient component once, regardless of the number of lights.
            Color color = hitInfo.Ambient * hitInfo.Color;

            // Add diffuse and specular components.
            foreach (RTLight light in scene.Lights)
            {
                Vector3 lightVector = (light.transform.position - hit.point).normalized;

                if (Vector3.Dot(hitInfo.Normal,lightVector) >= 0.0f) 
                    color += TraceLightImage(ref lightVector, light, in hitInfo);
            }

            // Cast reflection and refraction rays.
            if (depth > 0)
                color += TraceReflectionAndRefractionImage(depth, hitInfo);
            
            return ClampColor(color);
        }

        private Color TraceLightImage(ref Vector3 lightVector, RTLight light, in HitInfo hitInfo)
        {
            // Determine the distance to the light source. Note the clever use of the dot product.
            float lightDistance = Vector3.Dot(lightVector, light.transform.position - hitInfo.Point);

            // If we render shadows, check whether a shadow ray first meets the light or an object.
            if (RenderShadows)
            {
                Vector3 shadowOrigin = hitInfo.Point + Epsilon * hitInfo.Normal;
                
                // Trace a ray until we reach the light source. If we hit something return a shadow ray.
                if (Physics.Raycast(shadowOrigin, lightVector, out _, lightDistance, rayTracerLayer))
                    return Color.black;
            }

            // We either don't render shadows or nothing is between the object and the light source.
            
            // Calculate the color influence of this light.
            Vector3 reflectionVector = Vector3.Reflect(-lightVector, hitInfo.Normal);
            Color color = light.Ambient * light.Color * hitInfo.Color;
            color += Vector3.Dot(hitInfo.Normal, lightVector) * hitInfo.Diffuse * light.Diffuse *
                          light.Color * hitInfo.Color; // Id
            color += Mathf.Pow(Mathf.Max(Vector3.Dot(reflectionVector, hitInfo.View), 0.0f), hitInfo.Shininess) *
                     hitInfo.Specular * light.Specular * light.Color; // Is

            return ClampColor(color);
        }

        private Color TraceReflectionAndRefractionImage(int depth, in HitInfo hitInfo)
        {
            // The object is transparent, and thus refracts and reflects light.
            if (hitInfo.IsTransparent)
            {
                Color color;

                // Calculate the refractive index.
                float nint = hitInfo.InversedNormal ? hitInfo.RefractiveIndex : 1.0f / hitInfo.RefractiveIndex;

                // Use Schlick's approximation to determine the ratio between refraction and reflection.
                float kr0 = Mathf.Pow((nint - 1.0f) / (nint + 1.0f), 2);
                float kr = kr0 + (1.0f - kr0) * Mathf.Pow(1.0f - Vector3.Dot(hitInfo.Normal, hitInfo.View), 5);
                float kt = 1.0f - kr;

                // Reflect.
                color = kr * TraceImage(hitInfo.Point + hitInfo.Normal * Epsilon,
                    Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                    depth - 1);

                // Refract.
                color += kt * TraceImage(hitInfo.Point - hitInfo.Normal * Epsilon,
                    Refract(-hitInfo.View, hitInfo.Normal, nint),
                    depth - 1);

                return ClampColor(color);
            }

            // The object is not transparent, so we only reflect (provided it has a non zero specular component).
            if (hitInfo.Specular > 0.0f)
                return hitInfo.Specular * TraceImage(hitInfo.Point + hitInfo.Normal * Epsilon,
                    Vector3.Reflect(-hitInfo.View, hitInfo.Normal),
                    depth - 1);
            
            return Color.black;
        }

        private Vector3 Refract(Vector3 incident, Vector3 normal, float refractiveIndex)
        {
            float inputDot = Vector3.Dot(incident, normal);
            float root = 1.0f - (1.0f - inputDot * inputDot) * refractiveIndex * refractiveIndex;
            Vector3 refraction = (incident - inputDot * normal) * refractiveIndex;
            if (root < 0.0) return Vector3.Reflect(incident, normal);
            return refraction - normal * Mathf.Sqrt(root);
        }

        private Color ClampColor(Color color)
        {
            float r = Mathf.Clamp01(color.r);
            float g = Mathf.Clamp01(color.g);
            float b = Mathf.Clamp01(color.b);
            return new Color(r, g, b);
        }

        private void Awake()
        {
            instance = this;
            rayTracerLayer = LayerMask.GetMask("Ray Tracer Objects");
            if (acceleratedObject)
                totalTriangles = acceleratedObject.GetComponent<MeshFilter>().mesh.triangles.Length/3;
        }

        private void Start()
        {
            rtSceneManager = RTSceneManager.Get();
        }
    }
}
