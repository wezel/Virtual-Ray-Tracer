using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts;
using _Project.UI.Scripts.Main_Menu;
using _Project.UI.Scripts.Render_Image_Window;
using UnityEngine;

namespace _Project.UI.Scripts
{
    public class Overlay : Unique<Overlay>
    {
    
        [SerializeField]
        private RectTransform blocker;
        
        [SerializeField]
        private MainMenu mainMenu;
        public MainMenu MainMenu => mainMenu;
   
        
        [SerializeField]
        private RenderedImageWindow renderedImageWindow;
        /// <summary>
        /// The window in which the full resolution rendered image will be shown.
        /// </summary>
        public RenderedImageWindow RenderedImageWindow
        {
            get => renderedImageWindow;
            private set => renderedImageWindow = value;
        }
        
        [SerializeField]
        private FPSCounter fpsCounter;
        

        private bool fpsEnabled = false;

        public bool FPSEnabled
        {
            get => fpsEnabled;
            set
            {
                fpsEnabled = value;
                fpsCounter.gameObject.SetActive(value);
            }
        }


        private int blockerCount = 0;
        private List<Action> escapables = new();
        
        /// <summary>
        /// Enable a blocker panel that block all input except for the main menu and introduction panel.
        /// </summary>
        public void ShowBlocker(Action escapable)
        {
            AddEscapable(escapable);
            blockerCount++;
            blocker.gameObject.SetActive(true);
            Time.timeScale = 0;
        }

        
        public void AddEscapable(Action escapable)
        {
            escapables.Add(escapable);
        }

        /// <summary>
        /// Disable the blocker.
        /// </summary>
        public void HideBlocker(Action escapable)
        {
            if (blockerCount == 0) return;
            escapables.Remove(escapable);
            blockerCount--;
            if (blockerCount == 0)
            {
                blocker.gameObject.SetActive(false);
                Time.timeScale = 1;
            }

        }
        
        
        public void RemoveEscapable(Action escapable)
        {
            escapables.Remove(escapable);
        }

        private void ResetEscapables()
        {
            while (escapables.Count > 0)
                escapables.Last().Invoke();
            blockerCount = 0;
            blocker.gameObject.SetActive(false);
        }

        private void InvokeEscapable()
        {
            if (escapables.Count == 0) mainMenu.Show();
            else escapables.Last().Invoke();
        }
        

        private void Awake()
        {
            // make this object unique
            if (!MakeUnique(this)) return;
        }

        private void Start()
        {
            LevelManager.Get().OnLevelLoadFinished += ResetEscapables;
            InputManager.Get().OnEscape += InvokeEscapable;
        }
    }
}
