using System.Collections;
using _Project.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.UI.Scripts
{
    
    // TODO add to UI Manager
    public class LevelManager : Unique<LevelManager>
    {
        
        public delegate void LevelLoaded();
        /// <summary>
        /// An event invoked whenever a new scene is loaded is changed.
        /// </summary>
        public event LevelLoaded OnLevelLoaded;
        
        public delegate void CheatModeChanged(bool state);
        /// <summary>
        /// An event invoked whenever a new scene is loaded is changed.
        /// </summary>
        public event CheatModeChanged OnCheatModeChanged;

        public int CurrentLevel { get; private set; } = 2;
        public bool InMainMenu => CurrentLevel == 2;
        public int FirstLevel;
        
        private bool cheatMode = false;
        public bool CheatMode
        {
            get => cheatMode;
            set
            {
                cheatMode = value;
                OnCheatModeChanged?.Invoke(value);
            }
        }
        
        
        [Header("Level transition")]

        [SerializeField]
        private Animator Fade;
        
        [SerializeField]
        private float transitionTime = 1f;
        
        public void LoadNextLevel()
        {
            // TODO make better ruling for multiple sub categories in levels
            if (CurrentLevel < SceneManager.sceneCountInBuildSettings - 3)
            {
                StartCoroutine(LoadLevelInternal(CurrentLevel + 1));
            }
        }

        public void LoadLevel(int level)
        {
            StartCoroutine(LoadLevelInternal(level));
        }
        
        IEnumerator LoadLevelInternal(int level)
        {
            // TODO transition needs fixing
            Fade.SetTrigger("Start");
            //yield return new WaitForSeconds(transitionTime);
            if (CurrentLevel == 2)
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(1));
            } 
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(CurrentLevel));

            yield return LoadInternal(level);
        }

        IEnumerator LoadInternal(int level)
        {
            if (level == 2)
            {
                SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            }
            AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(level, LoadSceneMode.Additive);
            while (!asyncLoadLevel.isDone){
                yield return null;
            }
            
            CurrentLevel = level;
            OnLevelLoaded?.Invoke();
        }
        
        private void Awake()
        {
            // make this object unique
            if (!MakeUnique(this)) return;
            
        }
        
        private void Start()
        {
            StartCoroutine(LoadInternal(FirstLevel));
        }
    }
}
