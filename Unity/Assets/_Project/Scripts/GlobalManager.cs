using System;
using System.Collections.Generic;
using UnityEngine;
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
    public class GlobalManager : MonoBehaviour
    {
        private Dictionary<CursorType, Texture2D> cursorTextures = new Dictionary<CursorType, Texture2D>();

        // TODO add a key library here So keys can be remapped. GameObjects can check for these instead of the actual keys
    
        /// <summary>
        /// The cursor texture used when dragging certain UI components.
        /// </summary>
        private static GlobalManager instance = null;

        public bool FPSEnabled = false;
        public bool CheatMode = false;

        public static bool TutorialExpanded = true;
        public static int TutorialPoints = 0;
        public static int ObjectsCreated = 0;
        public static int EasterEggFound = 0;
        
        // TODO not the optimal way to do this including the way that task completion is registered.
        public List<Tasks> TutorialTasks;
        
        // Badges are fine here
        public List<Badge> Badges;

        /// <summary>
        /// Get the current <see cref="GlobalManager"/> instance.
        /// </summary>
        /// <returns> The current <see cref="GlobalManager"/> instance. </returns>
        public static GlobalManager Get()
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

#if true
            int required = 0, optional = 0;
            foreach (Tasks tasks in TutorialTasks)
            {
                required += tasks.GetRequiredTasksPoints();
                optional += tasks.GetOptionalTasksPoints();
            }
            Debug.Log("Total required points = " + required + ". Total optional points = " + optional + ". Total points = " + (required + optional));
#endif
        }
    }
}