using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;

namespace _Project.Ray_Tracer.Scripts
{
    /// <summary>
    /// Manages the visible rays in the Unity scene. Gets new rays from the ray tracer each frame and draws them.
    /// </summary>
    public class RayManager : MonoBehaviour
    {
        [Header("Render Settings")]

        [SerializeField]
        private bool hideNegligibleRays;
        /// <summary>
        /// Whether this ray manager hides negligible rays it would normally draw. When this is <c>false</c>, no ray
        /// will be hidden for contributing too little.
        /// </summary>
        public bool HideNegligibleRays
        {
            get => hideNegligibleRays;
            set
            {
                if (value == hideNegligibleRays) return;
                hideNegligibleRays = value;
                redraw = true;
            }
        }

        [SerializeField, Range(0.00f, 1.00f)]
        private float rayHideThreshold = 0.02f;
        /// <summary>
        /// The draw threshold of the rays this ray manager draws.
        /// </summary>
        public float RayHideThreshold
        {
            get { return rayHideThreshold; }
            set
            {
                if (value == rayHideThreshold) return;
                rayHideThreshold = value;
                redraw = true;
            }
        }

        [SerializeField]
        private bool rayTransparencyEnabled = true;
        /// <summary>
        /// Whether this ray manager makes rays transparent when it draws.
        /// </summary>
        public bool RayTransparencyEnabled
        {
            get { return rayTransparencyEnabled; }
            set
            {
                if (value == rayTransparencyEnabled) return;
                rayTransparencyEnabled = value;
                ReloadMaterials();
            }
        }

        [SerializeField, Range(0.00f, 2.00f)]
        public static float rayTransExponent = 1.00f;
        /// <summary>
        /// The transparency threshold of the rays this ray manager draws.
        /// </summary>
        public float RayTransExponent
        {
            get { return rayTransExponent; }
            set
            {
                if (value == rayTransExponent) return;
                rayTransExponent = value;
                ReloadMaterials();
            }
        }

        //[SerializeField, Range(0.00f, 1.00f)]
        //private float rayTransThreshold = 0.25f;
        ///// <summary>
        ///// The transparency threshold of the rays this ray manager draws.
        ///// </summary>
        //public float RayTransThreshold
        //{
        //    get { return rayTransThreshold; }
        //    set { rayTransThreshold = value; }
        //}

        [SerializeField]
        private bool rayDynamicRadiusEnabled = true;
        /// <summary>
        /// Whether this ray manager's rays' radius is contribution-based.
        /// </summary>
        public bool RayDynamicRadiusEnabled
        {
            get { return rayDynamicRadiusEnabled; }
            set
            {
                if (value == rayDynamicRadiusEnabled) return;
                rayDynamicRadiusEnabled = value;
                redraw = true;
            }
        }

        [SerializeField]
        private bool rayColorContributionEnabled = true;
        /// <summary>
        /// Whether this ray manager's rays' color is contribution-based.
        /// </summary>
        public bool RayColorContributionEnabled
        {
            get { return rayColorContributionEnabled; }
            set 
            {
                if (value == rayColorContributionEnabled) return;
                rayColorContributionEnabled = value;
                ReloadMaterials();
            }
        }

        [SerializeField, Range(0.001f, 0.1f)]
        private float rayRadius = 0.01f;
        /// <summary>
        /// The radius of the rays this ray manager draws.
        /// </summary>
        public float RayRadius
        {
            get { return rayRadius; }
            set
            {
                if (value == rayRadius) return;
                rayRadius = value;
                redraw = true;
            }
        }

        [SerializeField, Range(0.001f, 0.1f)]
        private float rayMinRadius = 0.003f;
        /// <summary>
        /// The minimum radius of the rays this ray manager draws.
        /// </summary>
        public float RayMinRadius
        {
            get { return rayMinRadius; }
            set
            {
                if (value == rayMinRadius) return;
                rayMinRadius = value;
                redraw = true;
            }
        }

        [SerializeField, Range(0.001f, 0.1f)]
        private float rayMaxRadius = 0.025f;
        /// <summary>
        /// The maximum radius of the rays this ray manager draws.
        /// </summary>
        public float RayMaxRadius
        {
            get { return rayMaxRadius; }
            set 
            {
                if (value == rayMaxRadius) return;
                rayMaxRadius = value;
                redraw = true;
            }
        }

        [SerializeField, Range(0.0f, 25.0f)]
        private float infiniteRayDrawLength = 10.0f;
        /// <summary>
        /// The length with which this ray manager draws rays that do not intersect an object.
        /// </summary>
        public float InfiniteRayDrawLength
        {
            get { return infiniteRayDrawLength; }
            set { infiniteRayDrawLength = value; }
        }

        [SerializeField]
        private RayObject rayPrefab;
        [SerializeField]
        private RayObject areaRayPrefab;
        [Range(0, 1024)]
        private int initialRayPoolSize = 64;

        private bool hideNoHitRays = false;
        /// <summary>
        /// Whether this ray manager hides rays that do not intersect an object.
        /// </summary>
        public bool HideNoHitRays
        {
            get { return hideNoHitRays; }
            set
            {
                if (value == hideNoHitRays) return;
                hideNoHitRays = value;
                redraw = true;
            }
        }

        [SerializeField]
        private bool showRays = true;

        /// <summary>
        /// Whether this ray manager hides all rays it would normally draw. When this is <c>false</c>, all ray drawing
        /// and animation code will be skipped.
        /// </summary>
        public bool ShowRays
        {
            get => showRays;
            set
            {
                if (value == showRays) return;
                showRays = value;
                if (!value) rayObjectPool.DeactivateAll();
                else redraw = true;
            }
        }

        /// <summary>
        /// Whether this ray manager should reload the rays' materials.
        /// </summary>            
        private void ReloadMaterials() => rayObjectPool.ReloadMaterials();

        [SerializeField] private Material noHitMaterial;
        [SerializeField] private Material reflectMaterial;
        [SerializeField] private Material reflectMaterialTransparent;
        [SerializeField] private Material refractMaterial;
        [SerializeField] private Material refractMaterialTransparent;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material normalMaterialTransparent;
        [SerializeField] private Material shadowMaterial;
        [SerializeField] private Material shadowMaterialTransparent;
        [SerializeField] private Material lightMaterial;
        [SerializeField] private Material lightMaterialTransparent;
        [SerializeField] private Material colorRayMaterial;
        [SerializeField] private Material colorRayMaterialTransparent;
        [SerializeField] private Material errorMaterial;

        [Header("Animation Settings")]

        [SerializeField]
        private bool animate = false;
        /// <summary>
        /// Whether this ray manager animates the rays it draws.
        /// </summary>
        public bool Animate
        {
            get { return animate; }
            set
            {
                if (value == animate) return;
                Reset = animate != value; // Reset the animation if we changed the value.
                animate = value;
                redraw = true;
            }
        }

        [SerializeField]
        private bool animateSequentially = false;
        /// <summary>
        /// Whether this ray manager animates the rays sequentially. Does nothing if <see cref="Animate"/> is not set.
        /// </summary>
        public bool AnimateSequentially
        {
            get { return animateSequentially; }
            set
            {
                if (value == animateSequentially) return;
                Reset = animateSequentially != value; // Reset the animation if we changed the value.
                animateSequentially = value;
            }
        }

        [SerializeField]
        private bool loop = false;
        /// <summary>
        /// Whether this ray manager loops its animation. Does nothing if <see cref="Animate"/> is not set.
        /// </summary>
        public bool Loop
        {
            get { return loop; }
            set { loop = value; }
        }

        [SerializeField]
        private bool reset = false;
        /// <summary>
        /// Whether this ray manager will reset its animation on the next frame. After resetting this variable will be
        /// set to <c>false</c> again. Does nothing if <see cref="Animate"/> is not set.
        /// </summary>
        public bool Reset
        {
            get { return reset; }
            set { reset = value; }
        }

        [SerializeField, Range(0.0f, 10.0f)]
        private float speed = 2.0f;
        /// <summary>
        /// The speed of this ray manager's animation. Does nothing if <see cref="Animate"/> is not set.
        /// </summary>
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        private static RayManager instance = null;

        private List<TreeNode<RTRay>> rays;
        private RayObjectPool rayObjectPool;

        private TreeNode<RTRay> selectedRay;
        private Vector2Int selectedRayCoordinates;
        private bool hasSelectedRay = false;

        private RTSceneManager rtSceneManager;
        private UnityRayTracer rayTracer;
        
        private float distanceToDraw = 0.0f;
        private int rayTreeToDraw = 0; // Used when animating sequentially.
        private bool animationDone = false;

        //private bool shouldUpdateRays = true;

        //private bool ShouldUpdateRays
        //{
        //    get => shouldUpdateRays;
        //    set
        //    {
        //        if (value == shouldUpdateRays) return;
        //        shouldUpdateRays = value;
        //        if (value) redraw = true;
        //    }
        //}

        /// <summary>
        /// Get the current <see cref="RayManager"/> instance.
        /// </summary>
        /// <returns> The current <see cref="RayManager"/> instance. </returns>
        public static RayManager Get()
        {
            return instance;
        }

        /// <summary>
        /// Get the colors of the root rays managed by this <see cref="RayManager"/>.
        /// </summary>
        /// <returns> A <see cref="Color"/><c>[]</c> of from rays at the root of their ray tree. </returns>
        public Color[] GetRayColors()
        {
            Color[] colors = new Color[rays.Count];
            for (int i = 0; i < rays.Count; i++)
            {
                colors[i] = rays[i].Data.Color;
                colors[i].a = 1.0f; // The ray tracer messes up the alpha channel, but it should always be 1.
            }
            return colors;
        }

        /// <summary>
        /// Get the material used to render rays of the given <see cref="RTRay.RayType"/>.
        /// </summary>
        /// <param name="contribution"> The fraction of the pixel-color this ray adds to it. </param>
        /// <param name="type"> What type of ray to get the material for. </param>
        /// <param name="color"> The color this ray contributes to the pixel-color </param>
        /// <returns>
        /// The <see cref="Material"/> for <paramref name="contribution"/>, <paramref name="type"/> and <paramref name="color"/>.
        /// </returns>
        public Material GetRayMaterial(float contribution, RTRay.RayType type, Color color, bool areaLight)
        {
            if (RayTransparencyEnabled || areaLight)
            {
                Material mat;
                if (RayColorContributionEnabled)
                    mat = GetRayColorMaterialTransparent(color.r, color.g, color.b);
                else
                    mat = GetRayTypeMaterialTransparent(type);

                return TranspariceMaterial(mat, contribution, areaLight);
            }
            else
            {
                if (RayColorContributionEnabled)
                    return GetRayColorMaterial(color.r, color.g, color.b);
                else
                    return GetRayTypeMaterial(type);
            }
        }

        private Material TranspariceMaterial(Material mat, float contribution, bool areaLight)
        {
            float baseFactor = Mathf.Pow(contribution * 0.65f + 0.25f, RayTransExponent);
            float alphaFactor = baseFactor * (areaLight ? (RayTransparencyEnabled ? 0.25f : 0.75f) : 0.6f);
            mat.SetFloat("_Ambient", baseFactor * 0.3f + 0.7f);
            mat.color = new Color(mat.color.r * (0.75f + baseFactor * 0.25f),
                                  mat.color.g * (0.75f + baseFactor * 0.25f),
                                  mat.color.b * (0.75f + baseFactor * 0.25f),
                                  mat.color.a * alphaFactor);
            return mat;
        }

        /// <summary>
        /// Get the material used to render rays of the given <see cref="RTRay.RayType"/>.
        /// </summary>
        /// <param name="type"> What type of ray to get the material for. </param>
        /// <returns>
        /// The <see cref="Material"/> for <paramref name="type"/>. The material for <see cref="RTRay.RayType.NoHit"/>
        /// is returned if <paramref name="type"/> is not a recognized <see cref="RTRay.RayType"/>.
        /// </returns>
        private Material GetRayTypeMaterial(RTRay.RayType type)
        {
            switch (type)
            {
                case RTRay.RayType.NoHit:
                    return noHitMaterial;
                case RTRay.RayType.Reflect:
                    return reflectMaterial;
                case RTRay.RayType.Refract:
                    return refractMaterial;
                case RTRay.RayType.Normal:
                    return normalMaterial;
                case RTRay.RayType.Shadow:
                case RTRay.RayType.AreaShadow:
                    return shadowMaterial;
                case RTRay.RayType.Light:
                case RTRay.RayType.AreaLight:
                    return lightMaterial;
                default:
                    Debug.LogError("Unrecognized ray type " + type + "!");
                    return errorMaterial;
            }
        }

        /// <summary>
        /// Get the transparent material used to render rays of the given <see cref="RTRay.RayType"/>.
        /// </summary>
        /// <param name="type"> What type of ray to get the material for. </param>
        /// <param name="transparency"> How transparent the ray should be. </param>
        /// <returns>
        /// The transparent <see cref="Material"/> for <paramref name="type"/>.
        /// </returns>
        private Material GetRayTypeMaterialTransparent(RTRay.RayType type)
        {
            switch (type)
            {
                case RTRay.RayType.NoHit:
                    return new Material(noHitMaterial);
                case RTRay.RayType.Reflect:
                    return new Material(reflectMaterialTransparent);
                case RTRay.RayType.Refract:
                    return new Material(refractMaterialTransparent);
                case RTRay.RayType.Normal:
                    return new Material(normalMaterialTransparent);
                case RTRay.RayType.Shadow:
                case RTRay.RayType.AreaShadow:
                    return new Material(shadowMaterialTransparent);
                case RTRay.RayType.Light:
                case RTRay.RayType.AreaLight:
                    return new Material(lightMaterialTransparent);
                default:
                    Debug.LogError("Unrecognized ray type " + type + "!");
                    return errorMaterial;
            }
        }

        /// <summary>
        /// Get the material used to render rays of the given Ray based on its color.
        /// </summary>
        /// <param name="r"> The red component of the color </param>
        /// <param name="g"> The green component of the color </param>
        /// <param name="b"> The blue component of the color </param>
        /// <returns>
        /// The <see cref="Material"/> for <paramref name="r"/>, <paramref name="g"/> and <paramref name="b"/>.
        /// </returns>
        private Material GetRayColorMaterial(float r, float g, float b)
        {
            return new Material(colorRayMaterial) { color = new Color(r, g, b, 1f) };
        }

        /// <summary>
        /// Get the transparent material used to render rays of the given Ray based on its color.
        /// </summary>
        /// <param name="r"> The red component of the color </param>
        /// <param name="g"> The green component of the color </param>
        /// <param name="b"> The blue component of the color </param>
        /// <returns> 
        /// The transparent <see cref="Material"/> for <paramref name="r"/>, <paramref name="g"/> and <paramref name="b"/>.
        /// </returns>
        private Material GetRayColorMaterialTransparent(float r, float g, float b)
        {
            return new Material(colorRayMaterialTransparent) { color = new Color(r, g, b, colorRayMaterialTransparent.color.a) };
        }

        public void SelectRay(Vector2Int rayCoordinates)
        {
            selectedRayCoordinates = rayCoordinates;
            hasSelectedRay = true;
            redraw = true;
            Reset = true;
        }

        public void DeselectRay()
        {
            hasSelectedRay = false;
            redraw = true;
            Reset = true;
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            rays = new List<TreeNode<RTRay>>();

            rayObjectPool = new RayObjectPool(rayPrefab, areaRayPrefab, initialRayPoolSize, transform);
            Reset = true;

            rtSceneManager = RTSceneManager.Get();
            rayTracer = UnityRayTracer.Get();

            rtSceneManager.Scene.OnSceneChanged += () => { UpdateRays(); };
            rayTracer.OnRayTracerChanged += () => { UpdateRays(); };
            UpdateRays();   // This is needed for level-changes.
        }

        private bool redraw = true;

        private void FixedUpdate()
        {
            if (ShowRays && (redraw || (Animate && (Reset || !animationDone || Loop))))
            {
                Redraw();
                redraw = false;
            }
        }

        private void Redraw()
        {
            // Determine the selected ray.
            if (hasSelectedRay)
            {
                int width = rtSceneManager.Scene.Camera.ScreenWidth;
                int index = selectedRayCoordinates.x + width * selectedRayCoordinates.y;
                selectedRay = rays[index];
                rayObjectPool.DeactivateAll();
            }

            if (Animate)
                DrawRaysAnimated();
            else
                DrawRays();
        }

        /// <summary>
        /// Get new ray trees from the ray tracer.
        /// </summary>
        public void UpdateRays()
        {
            rays = rayTracer.Render();
            rayObjectPool.MakeRayObjects(rays);
            rtSceneManager.UpdateImage(GetRayColors());
            redraw = true;
        }

        /// <summary>
        /// Returns the radius of the ray.
        /// </summary>
        /// <param name="rayTree">ray to get radius from</param>
        /// <returns></returns>
        private float GetRayRadius(TreeNode<RTRay> rayTree)
        {
            if (RayDynamicRadiusEnabled)
                return RayMinRadius + rayTree.Data.Contribution * (RayMaxRadius - RayMinRadius);
            else
                return RayRadius;
        }

        /// <summary>
        /// Draw <see cref="rays"/> in full.
        /// </summary>
        private void DrawRays()
        {
            // If we have selected a ray we only draw its ray tree.
            if (hasSelectedRay)
                foreach (var ray in selectedRay.Children) // Skip the zero-length base-ray 
                    DrawRayTree(ray);
            // Otherwise we draw all ray trees.
            else
                foreach (var pixel in rays)
                    foreach (var ray in pixel.Children) // Skip the zero-length base-ray 
                        DrawRayTree(ray);
        }

        private void DrawRayTree(TreeNode<RTRay> rayTree)
        {
            if ((HideNoHitRays && rayTree.Data.Type == RTRay.RayType.NoHit) ||
                (HideNegligibleRays && rayTree.Data.Contribution <= rayHideThreshold))
            {
                HideRays(rayTree);
                return;
            }

            RayObject rayObject = rayObjectPool.GetRayObject(rayTree.Data.ObjectPoolIndex, rayTree.Data.AreaRay);
            rayObject.Draw(GetRayRadius(rayTree));

            if (!rayTree.IsLeaf())
                foreach (var child in rayTree.Children)
                    DrawRayTree(child);
        }

        /// <summary>
        /// Draw a part of <see cref="rays"/> up to <see cref="distanceToDraw"/>. The part drawn grows each frame.
        /// </summary>
        private void DrawRaysAnimated()
        {
            // Reset the animation if we are looping or if a reset was requested.
            if ((animationDone && Loop) || Reset)
            {
                rayObjectPool.DeactivateAll();
                distanceToDraw = 0.0f;
                rayTreeToDraw = 0;
                animationDone = false;
                Reset = false;
            }

            // Animate all ray trees if we are not done animating already.
            if (!animationDone)
            {
                distanceToDraw += Speed * Time.deltaTime;
                animationDone = true; // Will be reset to false if one tree is not finished animating.

                // If we have selected a ray we only draw its ray tree.
                if (hasSelectedRay)
                {
                    foreach (var ray in selectedRay.Children) // Skip the zero-length base-ray 
                        animationDone &= DrawRayTreeAnimated(ray, distanceToDraw);
                }
                // If specified we animate the ray trees sequentially (pixel by pixel).
                else if (animateSequentially)
                {
                    // Draw all previous ray trees in full.
                    for (int i = 0; i < rayTreeToDraw; ++i)
                        foreach (var ray in rays[i].Children) // Skip the zero-length base-ray 
                            DrawRayTree(ray);

                    // Animate the current ray tree. If it is now fully drawn we move on to the next one.
                    bool treeDone = true;
                    foreach(var ray in rays[rayTreeToDraw].Children) // Skip the zero-length base-ray 
                        treeDone &= DrawRayTreeAnimated(ray, distanceToDraw);

                    if (treeDone)
                    {
                        distanceToDraw = 0.0f;
                        ++rayTreeToDraw;
                    }

                    animationDone = treeDone && rayTreeToDraw >= rays.Count;
                }
                // Otherwise we animate all ray trees.
                else
                {
                    foreach (var pixel in rays)
                        foreach (var rayTree in pixel.Children) // Skip the zero-length base-ray 
                            animationDone &= DrawRayTreeAnimated(rayTree, distanceToDraw);
                }
            }
            // Otherwise we can just draw all rays in full.
            else
                DrawRays();
        }

        private bool DrawRayTreeAnimated(TreeNode<RTRay> rayTree, float distance)
        {
            if ((HideNoHitRays && rayTree.Data.Type == RTRay.RayType.NoHit) ||
                (HideNegligibleRays && rayTree.Data.Contribution < rayHideThreshold))
            {
                HideRays(rayTree);
                return true;
            }

            RayObject rayObject = rayObjectPool.GetRayObject(rayTree.Data.ObjectPoolIndex, rayTree.Data.AreaRay);
            rayObject.Draw(GetRayRadius(rayTree), distance);

            float leftover = distance - rayObject.DrawLength;

            // If this ray is not at its full length we are not done animating.
            if (leftover <= 0.0f)
                return false;
            // If this ray is at its full length and has no children we are done animating.
            if (rayTree.IsLeaf())
                return true;

            // Otherwise we start animating the children.
            bool done = true;
            foreach (var child in rayTree.Children)
                done &= DrawRayTreeAnimated(child, leftover);
            return done;
        }

        public void HideRays(TreeNode<RTRay> rayTree)
        {
            rayObjectPool.HideRayObject(rayTree.Data.ObjectPoolIndex, rayTree.Data.AreaRay);
            rayTree.Children.ForEach(child => HideRays(child));
        }
    }
}