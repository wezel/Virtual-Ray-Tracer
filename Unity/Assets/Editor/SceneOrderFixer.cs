using _Project.UI.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Editor
{
    [InitializeOnLoad]
    public class SceneOrderFixer
    {

        private static int currentLevel;
        
        public delegate void PlayActivated();
        public static event PlayActivated OnPlay;
        
        static SceneOrderFixer ()
        {
            EditorApplication.playModeStateChanged += PlayModeCheck;
            OnPlay += Load;
        }

        static void PlayModeCheck(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.ExitingEditMode)
                OnPlay?.Invoke();
        }

        static void Load()
        {
            const string scenePath = "Assets/_Project/Levels/00_Base.unity";
            
            currentLevel = SceneManager.GetActiveScene().buildIndex;
            
            SceneAsset myWantedStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (myWantedStartScene != null)
                
                EditorSceneManager.playModeStartScene = myWantedStartScene;
            else
                Debug.Log("Could not find Scene ");
            
        }
        
        // After first Scene loaded after awake
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoadRuntimeMethod()
        {
            if(currentLevel > 1)
                LevelManager.Get().FirstLevel = currentLevel;
            
        }
    }
}
