using System.Collections;
using System.Linq;
using UnityEngine;

namespace _Project.UI.Scripts.Control_Panel
{
    public class VisualizationProperties : MonoBehaviour
    {

        [SerializeField]
        private BoolEdit highlight;

        public void Show()
        {
            gameObject.SetActive(true);
            highlight.IsOn = false;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            highlight.OnValueChanged += (value) =>
            {
                // THIS IS REALLY BAD!!! find another way so that when the name of the object changes i dont have to change it here too
                var text = highlight.GetComponentsInChildren<TMPro.TextMeshProUGUI>().First(c => c.gameObject.name == "Title");
                text.color = determineColor(value);
            };
        }

        private Color determineColor(bool value)
        {
            switch (value)
            {
                case true:
                    return new Color(0.0f, 1.0f, 1.0f, 1.0f);
                case false:
                    return new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
    }
}
