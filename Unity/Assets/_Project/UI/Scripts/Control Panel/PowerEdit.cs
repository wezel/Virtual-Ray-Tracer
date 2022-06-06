using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that allows for editing a floating point value via a slider and an input field. Presents a
    /// <see cref="OnValueChanged"/> event that is invoked whenever the value is changed. This change can come from the
    /// slider, input field or a direct assignment to the <see cref="Value"/> field.
    /// </summary>
    public class PowerEdit : FloatEdit
    {
        [SerializeField]
        private float powerBase = 2;
        protected override float CorrectValue(float value)
        {
            float correctedValue = (float)Math.Round(Math.Pow(value, 1f / powerBase), Digits);
            return Mathf.Clamp(Mathf.Pow(correctedValue, powerBase), MinValue, MaxValue);
        }

    }
}
