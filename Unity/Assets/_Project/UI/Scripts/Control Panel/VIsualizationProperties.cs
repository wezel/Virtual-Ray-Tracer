using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Project.UI.Scripts.Control_Panel
{
    public class VisualizationProperties : Singleton<VisualizationProperties>
    {
        [SerializeField]
        private TextMeshProUGUI resetText;
        [SerializeField]
        private TextMeshProUGUI startLoopText;
        [SerializeField]
        private TextMeshProUGUI noCollisionText;
        [SerializeField]
        private TextMeshProUGUI computeRayText;
        [SerializeField]
        private TextMeshProUGUI castChildrenText;
        [SerializeField]
        private TextMeshProUGUI stopLoopText;

        private List<TextMeshProUGUI> steps;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        //JAY try with no named text vars
        private void Awake()
        {
            Debug.Log("calling awake on visprops");

            steps = new List<TextMeshProUGUI>
            {
                resetText,
                startLoopText,
                noCollisionText,
                computeRayText,
                castChildrenText,
                stopLoopText
            };
            //these two return null
            Debug.Log(steps[0]);
            Debug.Log(resetText.text);
        }

        public void onStepChange(int newStep, int prevStep)
        {
            Debug.Log(steps.Count);
            if (prevStep >= 0)
                steps[prevStep].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            steps[newStep].color = new Color(0.0f, 1.0f, 1.0f, 1.0f);
        }
    }
}
