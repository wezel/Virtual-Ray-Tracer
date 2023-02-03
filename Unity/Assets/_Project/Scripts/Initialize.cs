// Demonstration of RuntimeInitializeOnLoadMethod and the argument it can take.

using UnityEngine;

namespace _Project.Scripts
{
    class Initialize
    {
    
        // Before first Scene loaded
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            
        }

        // After first Scene loaded
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoadRuntimeMethod()
        {
#if UNITY_WEBGL
            Object gameObject = Object.Instantiate(Resources.Load("Initialize/Game Manager WEB_GL"));
#else
            Object gameObject = Object.Instantiate(Resources.Load("Initialize/Game Manager"));
#endif
            gameObject.name = "Game Manager";
        }
    
        // RuntimeMethodLoad: After first Scene loaded
        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeMethodLoad()
        {
     
        }
    }
}