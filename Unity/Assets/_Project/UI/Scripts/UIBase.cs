using _Project.Scripts;
using _Project.UI.Scripts.Control_Panel;
using _Project.UI.Scripts.Toolbar;
using UnityEngine;

namespace _Project.UI.Scripts
{ 
    /// <summary>
    /// Manages the UI in a scene.
    /// </summary>
    public class UIBase : Unique<UIBase>
    {
        
        [SerializeField]
        private Canvas canvas;
        
        
        [SerializeField]
        private HelpPanel helpPanel;
        public HelpPanel HelpPanel => helpPanel;
        
        [SerializeField]
        private ControlPanel controlPanel;
        public ControlPanel ControlPanel => controlPanel;
        
        private void ToggleUI()
        {
            canvas.enabled = !canvas.enabled;
        }

        private void ReloadUI()
        {
            
            if (LevelManager.Get().InMainMenu)
            {
                canvas.enabled = false;
                return;
            }

            canvas.enabled = true;
        }
        
        private void Awake()
        {
            // make this object unique
            if (!MakeUnique(this)) return;
        }
        

        private void Start()
        {
            LevelManager.Get().OnLevelLoaded += ReloadUI;
            InputManager.Get().OnToggleUI += ToggleUI;
            
            // TODO make editor reference
            canvas = GetComponent<Canvas>();
            
            controlPanel.gameObject.SetActive(true);
            controlPanel.ShowRayTracerProperties();
        }
        
    }
}
