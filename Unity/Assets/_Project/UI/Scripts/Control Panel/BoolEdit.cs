using _Project.UI.Scripts.Tooltips;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that allows for editing a boolean value via a toggle. Presents a <see cref="OnValueChanged"/> event
    /// that is invoked whenever the value is changed. This change can come from the toggle or a direct assignment to
    /// the <see cref="IsOn"/> field.
    /// </summary>
    public class BoolEdit : MonoBehaviour
    {
        [Serializable]
        public class ValueChanged : UnityEvent<bool> { };
        [Serializable]
        public class Event : UnityEvent { };
        public ValueChanged OnValueChanged;
        public Event OnEnabled, OnDisabled;

        [SerializeField]
        private TextMeshProUGUI title;
        [SerializeField]
        private Toggle toggle;
        [SerializeField]
        private string tooltip;

        /// <summary>
        /// The displayed title of this <see cref="BoolEdit"/>.
        /// </summary>
        public string Title
        {
            get { return title.text; }
            set { title.text = value; }
        }

        // TODO rather have this not like this
        [SerializeField]
        private bool isOn;
        /// <summary>
        /// The value of this <see cref="BoolEdit"/>. When set, the toggle UI element will be updated.
        /// <see cref="OnValueChanged"/> will be invoked when it is set.
        /// </summary>
        public bool IsOn
        {
            get { return isOn; }
            set
            {
                if (isOn == value) return;
                isOn = value;
                
                toggle.isOn = isOn;

                // Notify listeners of the change.
                OnValueChanged?.Invoke(toggle.isOn);
                if (isOn)
                    OnEnabled?.Invoke();
                else
                    OnDisabled?.Invoke();
            }
        }

        [SerializeField]
        private bool interactable;
        /// <summary>
        /// Whether this <see cref="BoolEdit"/>'s UI is interactable.
        /// </summary>
        public bool Interactable
        {
            get { return interactable; }
            set
            {
                interactable = value;
                toggle.interactable = interactable;

                Graphic graphic = toggle.graphic;

                if (interactable)
                {
                    title.color = new Color(title.color.r, title.color.g, title.color.b, 1.0f);
                    graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 1.0f);
                }
                else
                {
                    title.color = new Color(title.color.r, title.color.g, title.color.b, 0.4f);
                    graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0.4f);
                }
            }
        }

        private void CheckToggleValueChanged()
        {
            IsOn = toggle.isOn;
        }

        private void Awake()
        {
            toggle.isOn = IsOn;
            toggle.onValueChanged.AddListener(delegate { CheckToggleValueChanged(); });


            // Update interactability based on serialized value in inspector.
            Interactable = interactable;

            TooltipTrigger tooltipTrigger = title.GetComponent<TooltipTrigger>();
            tooltipTrigger.Content = tooltip;
        }
    }
}
