using _Project.Ray_Tracer.Scripts;
using System;
using System.Collections.Generic;
using TMPro;
using TreeEditor;
using UnityEngine;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.Utility;

//no start bc no need to change values through the environment
namespace _Project.UI.Scripts.Control_Panel
{
    public class VisualizationProperties : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI rayType;
        [SerializeField]
        private TextMeshProUGUI rayChildren;
/*        [SerializeField]
        private TextMeshProUGUI noCollisionText;
        [SerializeField]
        private TextMeshProUGUI computeRayText;
        [SerializeField]
        private TextMeshProUGUI castChildrenText;
        [SerializeField]
        private TextMeshProUGUI stopLoopText;

        //[SerializeField]
        private List<TextMeshProUGUI> steps;*/
        private RayManager rayManager;
        private TreeNode<RTRay> ray;

        private void Awake()
        {
        }

        private void Start()
        {
        }
        public void Show(TreeNode<RTRay> ray)
        {
            gameObject.SetActive(true);
            rayManager = RayManager.Get();
            rayManager.drawingNewRay += RMDrawingNewRay;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void RMDrawingNewRay(object sender, TreeNode<RTRay> rayBeingDrawn)
        {
            ray = rayBeingDrawn;
            rayType.text = ray.Data.Type.ToString();
            rayChildren.text = ray.Children.Count.ToString();
        }

        /*        public void onStepChange(int prevStep, int newStep)
                {
                    if (prevStep >= 0)
                        steps[prevStep].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    steps[newStep].color = new Color(0.0f, 1.0f, 1.0f, 1.0f);
                }*/
    }
}
