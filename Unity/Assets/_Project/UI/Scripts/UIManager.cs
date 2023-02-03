using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts;
using _Project.UI.Scripts.Control_Panel;
using _Project.UI.Scripts.Main_Menu;
using _Project.UI.Scripts.Render_Image_Window;
using _Project.UI.Scripts.Toolbar;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.UI.Scripts
{ 
    /// <summary>
    /// Manages the UI in a scene.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private RectTransform blocker;
        private int blockerCount = 0;

        [SerializeField]
        private IntroductionPanel introductionPanel;

        [SerializeField]
        private FPSCounter fpsCounter;
        
        [SerializeField]
        private GameObject hidable;
        
        [SerializeField]
        private HelpPanel helpPanel;
        
        [SerializeField]
        private MainMenu mainMenu;
        
        [SerializeField]
        private ControlPanel controlPanel;

        [SerializeField]
        private RenderedImageWindow renderedImageWindow;
        /// <summary>
        /// The window in which the full resolution rendered image will be shown.
        /// </summary>
        public RenderedImageWindow RenderedImageWindow
        {
            get { return renderedImageWindow; }
            private set { renderedImageWindow = value; }
        }
        
        private static UIManager instance = null;

        private bool inOpeningScene;

        private List<Action> escapables = new List<Action>();

        public void AddEscapable(Action escapable)
        {
            escapables.Add(escapable);
        }

        public void RemoveEscapable(Action escapable)
        {
            escapables.Remove(escapable);
        }
        
        /// <summary>
        /// Get the current <see cref="UIManager"/> instance.
        /// </summary>
        /// <returns> The current <see cref="UIManager"/> instance. </returns>
        public static UIManager Get()
        {
            return instance;
        }

        /// <summary>
        /// Enable a blocker panel that block all input except for the main menu and introduction panel.
        /// </summary>
        public void EnableBlocker()
        {
            blockerCount++;
            blocker.gameObject.SetActive(true);
        }

        /// <summary>
        /// Disable the blocker.
        /// </summary>
        public void DisableBlocker()
        {
            // Only disable the blocker if all sources that enabled it also disabled it again.
            if (--blockerCount == 0)
                blocker.gameObject.SetActive(false);
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            fpsCounter.gameObject.SetActive(GlobalManager.Get().FPSEnabled);
          
            inOpeningScene = SceneManager.GetActiveScene().buildIndex == 0;
            
            if (inOpeningScene)
            {
                controlPanel.gameObject.SetActive(true);
                controlPanel.ShowRayTracerProperties();
                hidable.gameObject.SetActive(false);
                return;
            }
            
            introductionPanel.Show();
        }

        private void Update()
        {
            bool areThereEscapables = escapables.Count > 0;
            // These Keys are are not checked in the openings scene

            if (!inOpeningScene)
            {
                if (Input.GetKeyDown(KeyCode.Escape) && !areThereEscapables)
                    mainMenu.Show();
#if UNITY_WEBGL
                if (Input.GetKeyDown(KeyCode.H))
#else
                if (Input.GetKeyDown(KeyCode.F1))
#endif
                    helpPanel.Toggle();

                if (Input.GetKeyDown(KeyCode.F2))
                    hidable.gameObject.SetActive(!hidable.gameObject.activeSelf);
            }
            
            // All ui keys and keys shared between objects.
            if (Input.GetKeyDown(KeyCode.Escape) && areThereEscapables)
                escapables.Last().Invoke();
            
        }
    }
}
