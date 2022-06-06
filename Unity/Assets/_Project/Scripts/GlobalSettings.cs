using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using _Project.UI.Scripts.Tutorial;
using Cursor = UnityEngine.Cursor;
using Object = UnityEngine.Object;

namespace _Project.Scripts
{
    public enum CursorType
    {
        DragCursor,
        GrabCursor,
        ModeCursor,
        RotateCursor,
        ZoomCursor
    }

    /// <summary>
    /// Manages the application in general.
    /// </summary>
    public class GlobalSettings : MonoBehaviour
    {
        private Dictionary<CursorType, Texture2D> cursorTextures = new Dictionary<CursorType, Texture2D>();

        // TODO add a key library here So keys can be remapped. GameObjects can check for these instead of the actual keys
    
        /// <summary>
        /// The cursor texture used when dragging certain UI components.
        /// </summary>
        private static GlobalSettings instance = null;

        public bool FPSEnabled = false;
        public bool Unlimited = false;

        public static bool TutorialExpanded = true;
        public List<Tasks> TutorialTasks;

        /// <summary>
        /// Get the current <see cref="GlobalSettings"/> instance.
        /// </summary>
        /// <returns> The current <see cref="GlobalSettings"/> instance. </returns>
        public static GlobalSettings Get()
        {
            return instance;
        }

        /// <summary>
        /// Set the cursor texture to <see cref="DragCursor"/>.
        /// </summary>
        // TODO change this to accept a cursor type to be set to
        public void SetCursor(CursorType type)
        {
            Texture2D cursorTexture = cursorTextures[type];
            if(cursorTexture == null) return;
        
            Vector2 cursorOffset = new Vector2(cursorTexture.width / 2.0f, cursorTexture.height / 2.0f);
            Cursor.SetCursor(cursorTexture, cursorOffset, CursorMode.Auto);
        }

        /// <summary>
        /// Reset the cursor texture to the default.
        /// </summary>
        public void ResetCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void Start()
        {
            // The cursors in this folder have to have the CursorType enumerate names seen above for it to work.
            Object[] textures =  Resources.LoadAll("Textures/Cursors", typeof(Texture2D));
        
            foreach (var texture in textures) 
                if (Enum.TryParse(texture.name, out CursorType type)) 
                    cursorTextures.Add(type, (Texture2D) texture);
        }

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            //Debug.Log(Time.maximumDeltaTime);
            //Time.maximumDeltaTime = /*1f /*/ 3f;
            //Debug.Log(Time.maximumDeltaTime);
        }
    }
}