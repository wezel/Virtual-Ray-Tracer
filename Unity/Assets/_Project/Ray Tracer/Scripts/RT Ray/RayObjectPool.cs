using _Project.Ray_Tracer.Scripts.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Ray
{
    /// <summary>
    /// A simple class used to pool <see cref="RayObject"/>s for drawing by the <see cref="RayManager"/>. For more
    /// information on object pooling in Unity see: https://learn.unity.com/tutorial/introduction-to-object-pooling.
    /// Note that our implementation differs a somewhat because we have optimized the pool for our specific use case.
    /// </summary>
    public class RayObjectPool
    {
        private readonly List<RayObject> rayObjects;
        private readonly List<RayObject> areaRayObjects;
        private readonly RayObject rayPrefab;
        private readonly RayObject areaRayPrefab;
        private readonly Transform parent;
        private int nextIndex;
        private int nextAreaIndex;

        /// <summary>
        /// Construct a new pool of <see cref="RayObject"/>s. All instantiated objects start inactive.
        /// </summary>
        /// <param name="rayPrefab"> The <see cref="RayObject"/> prefab to be instantiated by this pool. </param>
        /// <param name="initialAmount"> The initial amount of <see cref="RayObject"/>s to instantiate. </param>
        /// <param name="parent"> The parent object of all <see cref="RayObject"/>s instantiated by this pool. </param>
        public RayObjectPool(RayObject rayPrefab, RayObject areaRayPrefab, int initialAmount, Transform parent)
        {
            this.rayPrefab = rayPrefab;
            this.areaRayPrefab = areaRayPrefab;
            this.parent = parent;

            rayObjects = new List<RayObject>(initialAmount);
            areaRayObjects = new List<RayObject>(initialAmount / 16);
            nextIndex = 0;
            nextAreaIndex = 0;
        }

        /// <summary>
        /// Reloads the materials for all rays in this <see cref="RayObject"/>.
        /// </summary>
        public void ReloadMaterials()
        {
            rayObjects.ForEach(ray => ray.ReloadMaterial());
            areaRayObjects.ForEach(areaRay => areaRay.ReloadMaterial());
        }

        /// <summary>
        /// Deactivate all <see cref="RayObject"/>s in this pool. This also marks the objects as unused.
        /// </summary>
        public void DeactivateAll()
        {
            rayObjects.ForEach(rayObject => rayObject.gameObject.SetActive(false));
            areaRayObjects.ForEach(areaRayObject => areaRayObject.gameObject.SetActive(false));
            nextIndex = 0;
            nextAreaIndex = 0;
        }

        /// <summary>
        /// Destroy all unused <see cref="RayObject"/>s in this pool.
        /// </summary>
        private void CleanUp()
        {
            for (int i = rayObjects.Count - 1; i >= nextIndex; --i)
                DestroyObject(rayObjects[i].gameObject);
            rayObjects.RemoveRange(nextIndex, rayObjects.Count - nextIndex);

            for (int i = areaRayObjects.Count - 1; i >= nextAreaIndex; --i)
                DestroyObject(areaRayObjects[i].gameObject);
            areaRayObjects.RemoveRange(nextAreaIndex, areaRayObjects.Count - nextAreaIndex);
        }

        private void DestroyObject(GameObject obj)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(obj);
#else
            Object.Destroy(obj);
#endif
        }

        /// <summary>
        /// Mark all <see cref="RayObject"/>s in this pool unused. This does not mean they are deactivated, but they
        /// will be returned by <see cref="GetRayObject"/>. The intended usage is to first call this function, then make
        /// all objects needed using <see cref="MakeRayObjects"/> and finally deactivate all unused objects left active
        /// by calling <see cref="CleanUp"/>.
        /// </summary>
        private void SetAllUnused()
        {
            nextIndex = 0;
            nextAreaIndex = 0;
        }

        /// <summary>
        /// Make all rays objects for all the rays in <paramref name="rays"/>
        /// </summary>
        /// <param name="rays"> Rays that need to be turned into objects. </param>
        public void MakeRayObjects(List<TreeNode<RTRay>> rays)
        {
            DeactivateAll();
            SetAllUnused(); // Mark all rays as unused; Start all over

            foreach (TreeNode<RTRay> pixel in rays)                 // Make RayObjects for all rays
                foreach (TreeNode<RTRay> rayTree in pixel.Children) // Skip the zero-length base-ray 
                    MakeRayTreeObjects(rayTree);

            CleanUp();      // Remove any rayobjects that are no longer necessary
        }

        /// <summary>
        /// Make all rays objects for all the rays in <paramref name="rayTree"/>
        /// </summary>
        /// <param name="rayTree"> Rays that need to be turned into objects. </param>
        private void MakeRayTreeObjects(TreeNode<RTRay> rayTree)
        {
            // Make sure there's a rayObject at the nextIndex
            rayTree.Data.ObjectPoolIndex = MakeRayObject(rayTree.Data);

            if (!rayTree.IsLeaf())
                foreach (TreeNode<RTRay> child in rayTree.Children)
                    MakeRayTreeObjects(child);
        }

        /// <summary>
        /// Get an unused <see cref="RayObject"/> from the pool and, if necessary, activate it. If there are no unused
        /// objects in the pool a new one will be instantiated and returned.
        /// </summary>
        /// <returns> An unused activated <see cref="RayObject"/> from the pool. </returns>
        private int MakeRayObject(RTRay ray)
        {
            if (ray.AreaRay)
            {
                MakeAreaRayObject();
                areaRayObjects[nextAreaIndex].Ray = ray;
                areaRayObjects[nextAreaIndex].gameObject.SetActive(false);
                return nextAreaIndex++;
            }
            else
            {
                MakeRayObject();
                rayObjects[nextIndex].Ray = ray;
                rayObjects[nextIndex].gameObject.SetActive(false);
                return nextIndex++;
            }
        }

        private void MakeRayObject()
        {
            // First we check if an unused ray object already exists.
            if (nextIndex < rayObjects.Count) return;

            // Else we add a new object to the pool.
            rayObjects.Add(Object.Instantiate(rayPrefab, parent));
        }

        private void MakeAreaRayObject()
        {
            // First we check if an unused arearay object already exists.
            if (nextAreaIndex < areaRayObjects.Count) return;

            // Else we add a new arearay object to the areapool.
            areaRayObjects.Add(Object.Instantiate(areaRayPrefab, parent));
        }

        public RayObject GetRayObject(int index, bool areaRay)
        {
           return areaRay ? GetAreaRayObject(index) : GetRayObject(index);
        }

        private RayObject GetRayObject(int index)
        {
            rayObjects[index].gameObject.SetActive(true);
            return rayObjects[index];
        }

        private RayObject GetAreaRayObject(int index)
        {
            areaRayObjects[index].gameObject.SetActive(true);
            return areaRayObjects[index];
        }

        public void HideRayObject(int index, bool areaRay)
        {
            if (areaRay) areaRayObjects[index].gameObject.SetActive(false);
            else rayObjects[index].gameObject.SetActive(false);
        }
    }
}
