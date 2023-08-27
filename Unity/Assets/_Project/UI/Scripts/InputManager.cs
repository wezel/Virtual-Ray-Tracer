using System;
using System.Collections.Generic;
using _Project.Scripts;
using UnityEngine;

namespace _Project.UI.Scripts
{
    public class InputManager : Unique<InputManager>
    {
        
        /// <summary>
        /// Escape event.
        /// </summary>
        public delegate void Escape();
        public event Escape OnEscape;
        
        /// <summary>
        /// Help event.
        /// </summary>
        public delegate void Help();
        public event Help OnHelp;
        
        /// <summary>
        /// Help event.
        /// </summary>
        public delegate void ToggleUI();
        public event ToggleUI OnToggleUI;
        
        
        
        // Start is called before the first frame update
        void Awake()
        {
            // make this object unique
            if (!MakeUnique(this)) return;
        }

        // Update is called once per frame
        void Update()
        {
            // These Keys are are not checked in the openings scene

            //if (!inOpeningScene)
            {
#if UNITY_WEBGL
                if (Input.GetKeyDown(KeyCode.H))
#else
                if (Input.GetKeyDown(KeyCode.F1))
#endif
                    OnHelp?.Invoke();

                if (Input.GetKeyDown(KeyCode.F2))
                    OnToggleUI?.Invoke();
            }
            
            // All ui keys and keys shared between objects.
            if (Input.GetKeyDown(KeyCode.Escape))
                OnEscape?.Invoke();
        }
    }
}
