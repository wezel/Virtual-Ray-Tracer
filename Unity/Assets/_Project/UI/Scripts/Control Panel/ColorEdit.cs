using _Project.UI.Scripts.Tooltips;
using HSVPicker;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that allows for editing a color value via a color picker UI. Presents a <see cref="OnValueChanged"/>
    /// event that is invoked whenever the color is changed. This change can come from the color picker or from a direct
    /// assignment to the <see cref="Color"/> field.
    /// </summary>
    public class ColorEdit : MonoBehaviour
    {
        [Serializable]
        public class ValueChanged : UnityEvent<Color> { }
        public ValueChanged OnValueChanged;

        /// <summary>
        /// Whether the mouse is hovered over the color picker UI component. It is set through event triggers on the color
        /// picker game object. The setter is public to facilitate this. Setting this value manually is not recommended.
        /// </summary>
        public bool PickerHovered { get; set; }

        [SerializeField]
        private TextMeshProUGUI title;
        
        [SerializeField]
        Button colorButton;
        
        [SerializeField]
        ColorPicker colorPicker;

        [SerializeField]
        private string tooltip;

        /// <summary>
        /// The displayed text of this <see cref="ColorEdit"/>.
        /// </summary>
        public string Title
        {
            get { return title.text; }
            set { title.text = value; }
        }

        private Color color;
        /// <summary>
        /// The color of this <see cref="ColorEdit"/>. When set, the color of the button UI element will be updated.
        /// <see cref="OnValueChanged"/> will be invoked when it is set.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set
            {
                if (color == value) return;

                // Set the color.
                color = value;

                // Update the UI elements.
                colorButton.image.color = color;
                colorPicker.AssignColor(color);

                // Notify listeners of the change.
                OnValueChanged?.Invoke(color);
            }
        }

        [SerializeField]
        private bool interactable;
        /// <summary>
        /// Whether this <see cref="ColorEdit"/>'s UI is interactable.
        /// </summary>
        public bool Interactable
        {
            get { return interactable; }
            set
            {
                interactable = value;
                colorButton.interactable = interactable;

                if (!interactable)
                    CloseColorPicker();

                if (interactable)
                    title.color = new Color(title.color.r, title.color.g, title.color.b, 1.0f);
                else
                    title.color = new Color(title.color.r, title.color.g, title.color.b, 0.4f);
            }
        }

        public void ToggleColorPicker()
        {
            if(!colorPicker.gameObject.activeSelf) OpenColorPicker();
            else CloseColorPicker();
        }
        private void OpenColorPicker()
        {
            colorPicker.AssignColor(Color);
            colorPicker.gameObject.SetActive(true);
            UIManager.Get().AddEscapable(CloseColorPicker);
        }

        private void CloseColorPicker()
        {
            colorPicker.gameObject.SetActive(false);
            UIManager.Get().RemoveEscapable(CloseColorPicker);
        }

        private void Awake()
        {
            CloseColorPicker();
            
            // Update interactability based on serialized value in inspector.
            Interactable = interactable;
        }

        private void OnDisable()
        {
            CloseColorPicker();
        }
        

        private void Update()
        {
            // Close the color picker if we click outside of its bounds.
            if (Input.GetMouseButtonDown(0) && colorPicker.gameObject.activeSelf && !PickerHovered)
                CloseColorPicker();

            // Update the color based on the color picker's color.
            if (colorPicker.gameObject.activeSelf)
                Color = colorPicker.CurrentColor;

            TooltipTrigger tooltipTrigger = title.GetComponent<TooltipTrigger>();
            tooltipTrigger.Content = tooltip;
        }
    }
}
