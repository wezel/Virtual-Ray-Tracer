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
        private int currentlyActive;
        private int currentlyUsed;

        /// <summary>
        /// Construct a new pool of <see cref="RayObject"/>s. All instantiated objects start inactive.
        /// </summary>
        /// <param name="rayPrefab"> The <see cref="RayObject"/> prefab to be instantiated by this pool. </param>
        /// <param name="initialAmount"> The initial amount of <see cref="RayObject"/>s to instantiate. </param>
        public RayObjectPool(RayObject rayPrefab, int initialAmount) : this(rayPrefab, initialAmount, null)
        {
        }

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

            currentlyActive = 0;
            currentlyUsed = 0;
        }

        /// <summary>
        /// Deactivate all <see cref="RayObject"/>s in this pool. This also marks the objects as unused.
        /// </summary>
        public void DeactivateAll()
        {
            for (int i = 0; i < rayObjects.Count; ++i)
                rayObjects[i].gameObject.SetActive(false);

            currentlyActive = 0;
            currentlyUsed = 0;
        }

        /// <summary>
        /// Deactivate all unused <see cref="RayObject"/>s in this pool.
        /// </summary>
        public void DeactivateUnused()
        {
            for (int i = currentlyUsed; i < currentlyActive; ++i)
                rayObjects[i].gameObject.SetActive(false);

            currentlyActive = currentlyUsed;
        }

        /// <summary>
        /// Mark all <see cref="RayObject"/>s in this pool unused. This does not mean they are deactivated, but they
        /// will be returned by <see cref="GetRayObject"/>. The intended usage is to first call this function, then get
        /// the objects you need using <see cref="GetRayObject"/> and finally deactivate all unused objects left active
        /// by calling <see cref="DeactivateUnused"/>.
        /// </summary>
        public void SetAllUnused()
        {
            currentlyUsed = 0;
        }

        /// <summary>
        /// Get an unused <see cref="RayObject"/> from the pool and, if necessary, activate it. If there are no unused
        /// objects in the pool a new one will be instantiated and returned.
        /// </summary>
        /// <returns> An unused activated <see cref="RayObject"/> from the pool. </returns>
        public RayObject GetRayObject()
        {

            // First try to get unused but already active ray objects from the pool.
            if (currentlyUsed < currentlyActive)
                return rayObjects[currentlyUsed++];

            // Otherwise we get the first unused ray object and activate it.
            if (currentlyUsed < rayObjects.Count)
            {
                rayObjects[currentlyUsed].gameObject.SetActive(true);
                ++currentlyActive;
                return rayObjects[currentlyUsed++];
            }

            // If all ray object are already in use we create a new one.
            RayObject ray;
            if (parent != null)
                ray = Object.Instantiate(rayPrefab, parent);
            else
                ray = Object.Instantiate(rayPrefab);

            rayObjects.Add(ray);
            ++currentlyActive;
            ++currentlyUsed;
            return ray;
        }
    }
}
