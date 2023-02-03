using System;
using _Project.Scripts;
using _Project.UI.Scripts.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that allows for editing a vector 3 value via three input fields. Presents a <see cref="OnValueChanged"/>
    /// event that is invoked whenever the value is changed. This change can come from the any of the three input fields or
    /// from a direct assignment to the <see cref="Value"/> field.
    /// </summary>
    public class Vector3Edit : MonoBehaviour
    {
        /// <summary>
        /// One of the three fields of a <see cref="Vector3Edit"/>.
        /// </summary>
        public enum Field
        {
            X, Y, Z
        }

        [Serializable]
        public class ValueChanged : UnityEvent<Vector3> { };
        public ValueChanged OnValueChanged;

        /// <summary>
        /// Whether the mouse is dragging the x component of this <see cref="Vector3Edit"/>. It is set through event
        /// triggers on the input field text game object. The setter is public to facilitate this. Setting this value
        /// manually is not recommended.
        /// </summary>
        public bool DraggingX { get; set; }

        /// <summary>
        /// Whether the mouse is dragging the y component of this <see cref="Vector3Edit"/>. It is set through event
        /// triggers on the input field text game object. The setter is public to facilitate this. Setting this value
        /// manually is not recommended.
        /// </summary>
        public bool DraggingY { get; set; }

        /// <summary>
        /// Whether the mouse is dragging the z component of this <see cref="Vector3Edit"/>. It is set through event
        /// triggers on the input field text game object. The setter is public to facilitate this. Setting this value
        /// manually is not recommended.
        /// </summary>
        public bool DraggingZ { get; set; }

        /// <summary>
        /// The displayed title of this <see cref="Vector3Edit"/>.
        /// </summary>
        public string Title
        {
            get { return title.text; }
            set { title.text = value; }
        }

        private Vector3 value;
        /// <summary>
        /// The value of this <see cref="Vector3Edit"/>. When set, the input field UI elements will be updated.
        /// <see cref="OnValueChanged"/> will be invoked when it is set.
        /// </summary>
        public Vector3 Value
        {
            get { return value; }
            set
            {
                if (this.value == value) return;

                // Set the value.
                this.value = value;

                // Update the text of the input fields if we are not already changing the text directly.
                if (!changingInputText)
                {
                    UpdateInputText(Field.X);
                    UpdateInputText(Field.Y);
                    UpdateInputText(Field.Z);
                }

                // Notify listeners of the change.
                OnValueChanged?.Invoke(this.value);
            }
        }

        [SerializeField]
        private int digits;
        /// <summary>
        /// The number of digits displayed in this <see cref="Vector3Edit"/>'s input fields. Unlike 
        /// <see cref="FloatEdit.Digits"/> this field is purely visual; the underlying <see cref="Value"/> will have full 
        /// floating point precision.
        /// </summary>
        public int Digits
        {
            get { return digits; }
            set
            {
                digits = value;

                // Update the text of the input fields for the new number of digits.
                UpdateInputText(Field.X);
                UpdateInputText(Field.Y);
                UpdateInputText(Field.Z);
            }
        }

        [SerializeField]
        private bool interactable;
        /// <summary>
        /// Whether this <see cref="Vector3Edit"/>'s UI is interactable.
        /// </summary>
        public bool Interactable
        {
            get { return interactable; }
            set
            {
                interactable = value;
                input1.interactable = interactable;
                input2.interactable = interactable;
                input3.interactable = interactable;

                Text input1Text = input1.transform.Find("Text").GetComponent<Text>();
                Text input2Text = input2.transform.Find("Text").GetComponent<Text>();
                Text input3Text = input3.transform.Find("Text").GetComponent<Text>();

                if (interactable)
                {
                    title.color = new Color(title.color.r, title.color.g, title.color.b, 1.0f);
                    text1.color = new Color(text1.color.r, text1.color.g, text1.color.b, 1.0f);
                    text2.color = new Color(text2.color.r, text2.color.g, text2.color.b, 1.0f);
                    text3.color = new Color(text3.color.r, text3.color.g, text3.color.b, 1.0f);
                    input1Text.color = new Color(input1Text.color.r, input1Text.color.g, input1Text.color.b, 1.0f);
                    input2Text.color = new Color(input2Text.color.r, input2Text.color.g, input2Text.color.b, 1.0f);
                    input3Text.color = new Color(input3Text.color.r, input3Text.color.g, input3Text.color.b, 1.0f);
                }
                else
                {
                    title.color = new Color(title.color.r, title.color.g, title.color.b, 0.4f);
                    text1.color = new Color(text1.color.r, text1.color.g, text1.color.b, 0.4f);
                    text2.color = new Color(text2.color.r, text2.color.g, text2.color.b, 0.4f);
                    text3.color = new Color(text3.color.r, text3.color.g, text3.color.b, 0.4f);
                    input1Text.color = new Color(input1Text.color.r, input1Text.color.g, input1Text.color.b, 0.4f);
                    input2Text.color = new Color(input2Text.color.r, input2Text.color.g, input2Text.color.b, 0.4f);
                    input3Text.color = new Color(input3Text.color.r, input3Text.color.g, input3Text.color.b, 0.4f);
                }
            }
        }

        [SerializeField]
        private float dragSpeed = 1.0f;
        private Vector3 previousMousePostion;

        [SerializeField]
        private TextMeshProUGUI title;
        [SerializeField]
        private TextMeshProUGUI text1;
        [SerializeField]
        private TextMeshProUGUI text2;
        [SerializeField]
        private TextMeshProUGUI text3;
        [SerializeField]
        private InputField input1;
        [SerializeField]
        private InputField input2;
        [SerializeField]
        private InputField input3;
        [SerializeField]
        private string tooltip;

        private bool changingInputText = false;
        private bool updatingInputText = false;

        /// <summary>
        /// Handle the mouse entering an area where it can be dragged by setting the cursor texture.
        /// </summary>
        public void EnterDragArea()
        {
            if (Interactable)
                GlobalManager.Get().SetCursor(CursorType.DragCursor);
        }

        /// <summary>
        /// Handle the mouse exiting an area where it can be dragged by resetting the cursor texture.
        /// </summary>
        public void ExitDragArea()
        {
            GlobalManager.Get().ResetCursor();
        }

        /// <summary>
        /// Determine whether the mouse is dragging the x, y or z component of this <see cref="Vector3Edit"/>
        /// </summary>
        /// <returns> <c><see cref="DraggingX"/> || <see cref="DraggingY"/> || <see cref="DraggingZ"/></c> </returns>
        public bool IsDragging()
        {
            return DraggingX || DraggingY || DraggingZ;
        }

        /// <summary>
        /// Update the text of <paramref name="field"/> based on <see cref="Value"/>. The text will be the relevant
        /// component of <see cref="Value"/> rounded to the number of digits specified by <see cref="Digits"/>. Will result
        /// in an <see cref="InputField.onValueChanged"/> event, but we ignore this event in
        /// <see cref="CheckInputValueChanged"/> by setting <see cref="updatingInputText"/>.
        /// </summary>
        /// <param name="field"> Which of the three input fields to update. </param>
        private void UpdateInputText(Field field)
        {
            float rounded = 0.0f;
            updatingInputText = true;

            // Try to update the input field text.
            try
            {
                switch (field)
                {
                    case Field.X:
                        rounded = (float)Math.Round(Value.x, Digits);
                        input1.text = rounded.ToString();
                        break;
                    case Field.Y:
                        rounded = (float)Math.Round(Value.y, Digits);
                        input2.text = rounded.ToString();
                        break;
                    case Field.Z:
                        rounded = (float)Math.Round(Value.z, Digits);
                        input3.text = rounded.ToString();
                        break;
                }
            }
            // An improperly formated input field is interpreted as a 0. Update it if the value is not actually 0.
            catch (FormatException)
            {
                switch (field)
                {
                    case Field.X:
                        if (Value.x != 0.0f)
                            input1.text = rounded.ToString();
                        break;
                    case Field.Y:
                        if (Value.y != 0.0f)
                            input2.text = rounded.ToString();
                        break;
                    case Field.Z:
                        if (Value.z != 0.0f)
                            input3.text = rounded.ToString();
                        break;
                }
            }
        }

        /// <summary>
        /// Get the value from <paramref name="input"/>.
        /// </summary>
        /// <param name="input"> The <see cref="InputField"/> to get the value from. </param>
        /// <returns> 
        /// The <see cref="InputField.text"/> variable of <paramref name="input"/> converted to a <c>float</c> or <c>0</c>
        /// if <paramref name="input"/> is not formatted properly.
        /// </returns>
        private float GetInputValue(InputField input)
        {
            // Try to get the value from the input field.
            try
            {
                return float.Parse(input.text);
            }
            // If the input field is not formatted properly we default to 0.
            catch (FormatException)
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// Callback for <see cref="InputField.onValueChanged"/>. We update <see cref="Value"/>, but we don't set the input
        /// field's text. This means that, while changing, an input field can be in an improperly formatted state (e.g.
        /// empty). <see cref="Value"/> will then be set to 0.
        /// </summary>
        /// <param name="field"> Which of the three input fields to check. </param>
        private void CheckInputValueChanged(Field field)
        {
            // If this callback was due to UpdateInputText being called, we don't update the value.
            if (updatingInputText)
            {
                updatingInputText = false;
                return;
            }

            changingInputText = true;

            // When an input field changes we update the value.
            switch (field)
            {
                case Field.X:
                    Value = new Vector3(GetInputValue(input1), Value.y, Value.z);
                    break;
                case Field.Y:
                    Value = new Vector3(Value.x, GetInputValue(input2), Value.z);
                    break;
                case Field.Z:
                    Value = new Vector3(Value.x, Value.y, GetInputValue(input3));
                    break;
            }

            changingInputText = false;
        }

        /// <summary>
        /// Callback for <see cref="InputField.onEndEdit"/>. We update <see cref="Value"/> and set the input field's text.
        /// This means that if an input field was left in an improperly formatted state its text will be set to 0.
        /// </summary>
        /// <param name="field"> Which of the three input fields to check. </param>
        private void CheckInputEndEdit(Field field)
        {
            switch (field)
            {
                case Field.X:
                    float xValue = GetInputValue(input1);
                    Value = new Vector3(xValue, Value.y, Value.z);
                    input1.text = Math.Round(xValue, Digits).ToString();
                    break;
                case Field.Y:
                    float yValue = GetInputValue(input2);
                    Value = new Vector3(Value.x, yValue, Value.z);
                    input2.text = Math.Round(yValue, Digits).ToString();
                    break;
                case Field.Z:
                    float zValue = GetInputValue(input3);
                    Value = new Vector3(Value.x, Value.y, zValue);
                    input3.text = Math.Round(zValue, Digits).ToString();
                    break;
            }
        }

        private void Awake()
        {
            input1.text = Value.x.ToString();
            input1.onValueChanged.AddListener(delegate { CheckInputValueChanged(Field.X); });
            input1.onEndEdit.AddListener(delegate { CheckInputEndEdit(Field.X); });

            input2.text = Value.y.ToString();
            input2.onValueChanged.AddListener(delegate { CheckInputValueChanged(Field.Y); });
            input2.onEndEdit.AddListener(delegate { CheckInputEndEdit(Field.Y); });

            input3.text = Value.z.ToString();
            input3.onValueChanged.AddListener(delegate { CheckInputValueChanged(Field.Z); });
            input3.onEndEdit.AddListener(delegate { CheckInputEndEdit(Field.Z); });

            // Update interactability based on serialized value in inspector.
            Interactable = interactable;

            TooltipTrigger tooltipTrigger = title.GetComponent<TooltipTrigger>();
            tooltipTrigger.Content = tooltip;
        }

        private void Update()
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 deltaMousePosition = currentMousePosition - previousMousePostion;
            float deltaX = deltaMousePosition.x * dragSpeed * Time.deltaTime;
            float deltaY = deltaMousePosition.y * dragSpeed * Time.deltaTime;

            if (Interactable)
            {
                if (DraggingX)
                    Value = new Vector3(Value.x + deltaX + deltaY, Value.y, Value.z);
                if (DraggingY)
                    Value = new Vector3(Value.x, Value.y + deltaX + deltaY, Value.z);
                if (DraggingZ)
                    Value = new Vector3(Value.x, Value.y, Value.z + deltaX + deltaY);
            }

            previousMousePostion = currentMousePosition;
        }
    }
}