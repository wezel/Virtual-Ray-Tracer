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
        private List<RayObject> rayObjects;
        private RayObject rayPrefab;
        private Transform parent;
        private int currentlyUsed;

        /// <summary>
        /// Construct a new pool of <see cref="RayObject"/>s. All instantiated objects start inactive.
        /// </summary>
        /// <param name="rayPrefab"> The <see cref="RayObject"/> prefab to be instantiated by this pool. </param>
        /// <param name="initialAmount"> The initial amount of <see cref="RayObject"/>s to instantiate. </param>
        /// <param name="parent"> The parent object of all <see cref="RayObject"/>s instantiated by this pool. </param>
        public RayObjectPool(RayObject rayPrefab, int initialAmount, Transform parent)
        {
            this.rayPrefab = rayPrefab;
            this.parent = parent;

            rayObjects = new List<RayObject>(initialAmount);
            RayObject ray;

            for (int i = 0; i < initialAmount; ++i)
            {
                if (parent != null)
                    ray = Object.Instantiate(rayPrefab, parent);
                else
                    ray = Object.Instantiate(rayPrefab);

                ray.gameObject.SetActive(false);
                rayObjects.Add(ray);
            }

            currentlyUsed = 0;
        }

        /// <summary>
        /// Deactivate all <see cref="RayObject"/>s in this pool. This also marks the objects as unused.
        /// </summary>
        public void DeactivateAll()
        {
            for (int i = 0; i < rayObjects.Count; ++i)
                rayObjects[i].gameObject.SetActive(false);

            currentlyUsed = 0;
        }

        /// <summary>
        /// Destroy all unused <see cref="RayObject"/>s in this pool.
        /// </summary>
        private void CleanUp()
        {
            for (int i = currentlyUsed; i < rayObjects.Count; ++i)
            {
                if (!rayObjects[i].isActiveAndEnabled)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(rayObjects[i].gameObject);
#else
                    Object.Destroy(x.gameObject);
#endif
                    rayObjects.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Mark all <see cref="RayObject"/>s in this pool unused. This does not mean they are deactivated, but they
        /// will be returned by <see cref="GetRayObject"/>. The intended usage is to first call this function, then make
        /// all objects needed using <see cref="MakeRayObjects"/> and finally deactivate all unused objects left active
        /// by calling <see cref="CleanUp"/>.
        /// </summary>
        private void SetAllUnused()
        {
            currentlyUsed = 0;
        }

        /// <summary>
        /// Reloads the materials for all rays in this <see cref="RayObject"/>.
        /// </summary>
        public void ReloadMaterials()
        {
            rayObjects.ForEach(ray => { if (ray != null) ray.ReloadMaterial(); });
        }

        /// <summary>
        /// Make all rays objects for all the rays in <paramref name="rays"/>
        /// </summary>
        /// <param name="rays"> Rays that need to be turned into objects. </param>
        public void MakeRayObjects(ref List<TreeNode<RTRay>> rays)
        {
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
            MakeRayObject();    // Make sure there's a rayObject at the index currentlyUsed
            rayTree.Data.ObjectPoolIndex = currentlyUsed;
            rayObjects[currentlyUsed].Ray = rayTree.Data;
            rayObjects[currentlyUsed].gameObject.SetActive(false);
            currentlyUsed++;

            if (!rayTree.IsLeaf())
                foreach (TreeNode<RTRay> child in rayTree.Children)
                    MakeRayTreeObjects(child);
        }

        /// <summary>
        /// Get an unused <see cref="RayObject"/> from the pool and, if necessary, activate it. If there are no unused
        /// objects in the pool a new one will be instantiated and returned.
        /// </summary>
        /// <returns> An unused activated <see cref="RayObject"/> from the pool. </returns>
        private void MakeRayObject()
        {
            // Otherwise we get the first unused ray object and activate it.
            if (currentlyUsed < rayObjects.Count)
                return;

            rayObjects.Add(Object.Instantiate(rayPrefab, parent));
        }

        public RayObject GetRayObject(int index)
        {
            rayObjects[index].gameObject.SetActive(true);
            return rayObjects[index];
        }
    }
}
