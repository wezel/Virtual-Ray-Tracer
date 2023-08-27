using UnityEngine;

namespace _Project.Scripts
{
    
    /// <summary>
    /// Base functions for unique objects.
    /// To give a script unique functionality inherit from this class.
    /// Set <typeparamref name="T"/> to the type of the child script:
    /// <example><code>
    /// public class UniqueScript : Unique&lt;UniqueScript>
    /// </code></example>
    /// and add the following code to the top of the <see cref="Awake"/> function:
    /// <example><code>
    /// // make this object unique
    /// if (!MakeUnique(this)) return;
    /// </code></example>
    /// <typeparam name="T">the Type of the child class.</typeparam>
    /// </summary>
    public class Unique<T> : MonoBehaviour
    {

        private static T _instance = default(T);

        /// <summary>
        /// Get the current Unique instance.
        /// </summary>
        /// <returns> The current Unique instance of type <typeparamref name="T"/>. </returns>
        public static T Get()
        {
            return _instance;
        }

        /// <summary>
        /// Make the object unique and notify if we were successful.
        /// 
        /// </summary>
        /// <returns> The success state of making the object unique </returns>
        protected bool MakeUnique(T script)
        {

            // make this object unique
            // if (_instance != null)
            // {
            //     Destroy(gameObject);
            //     return false;
            // }
            
            _instance = script;
            // DontDestroyOnLoad(gameObject);
            return true;

        }

        private void Awake()
        {}
    }
}