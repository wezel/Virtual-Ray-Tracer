// Demonstration of RuntimeInitializeOnLoadMethod and the argument it can take.

using UnityEngine;

namespace _Project.Scripts
{
    class Initialize
    {
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnBeforeSplashScreenLoadRuntimeMethod()
        {
            
            // TODO editor code
            // SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            // SceneManager.LoadScene(0);
        }
    
        // Before first Scene loaded
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
        }
        


        // After first Scene loaded after awake
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoadRuntimeMethod()
        {
            
            // TODO might (re)move this
#if UNITY_WEBGL
            Object gameObject = Object.Instantiate(Resources.Load("Initialize/Game Manager WEB_GL"));
#else
            Object gameObject = Object.Instantiate(Resources.Load("Initialize/Game Manager"));
#endif
            gameObject.name = "Game Manager";
            
        }
    
        // RuntimeMethodLoad: After first Scene loaded after awake
        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeMethodLoad()
        {
        }
    }
}