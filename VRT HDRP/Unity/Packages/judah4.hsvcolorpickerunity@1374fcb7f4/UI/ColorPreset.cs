using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HSVPicker
{
    public class ColorPreset : MonoBehaviour
    {
        [SerializeField]
        private Image image;
        public Color Color
        {
            get => image.color;
            set => image.color = value;
        }

        [SerializeField]
        private TextMeshProUGUI text;
        public bool TextEnable { set => text.gameObject.SetActive(value); }
    }
}