using _Project.Ray_Tracer.Scripts;
using System;
using System.Collections.Generic;
using TMPro;
//using TreeEditor;
using UnityEngine;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.Utility;
using System.Collections;

namespace _Project.UI.Scripts.Control_Panel
{
    public class VisualizationProperties : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI formRayText;
        [SerializeField]
        private TextMeshProUGUI computeIntersectionInput;
        [SerializeField]
        private TextMeshProUGUI ifIntersectText;
        [SerializeField]
        private TextMeshProUGUI enterShadeText;
        [SerializeField]
        private TextMeshProUGUI computeLightRayText;
        [SerializeField]
        private TextMeshProUGUI ifReflectiveText;
        [SerializeField]
        private TextMeshProUGUI computeReflectiveText;
        [SerializeField]
        private TextMeshProUGUI ifTransparentText;
        [SerializeField]
        private TextMeshProUGUI computeRefractiveText;

/*        private struct FlaggedRay
        {
            public TreeNode<RTRay> rayData;
            public bool flag;
        }*/

        //[SerializeField]
        private List<TextMeshProUGUI> steps;
        private int globalPrev;
        private RayManager rayManager;
        private TreeNode<RTRay> ray;


        private void Awake()
        {
            steps = new List<TextMeshProUGUI>() {
                formRayText,
                computeIntersectionInput,
                ifIntersectText,
                enterShadeText,
                computeLightRayText,
                ifReflectiveText,
                computeReflectiveText,
                ifTransparentText,
                computeRefractiveText
            };
            ray = null;
            //reset color - needed when we exit visprops and then start it again.
            resetColor();
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
            Debug.Log($"received {rayBeingDrawn.Data.Type}");
            //sometimes the last text box remains highlighted, so just get rid of that
            resetColor();
            ray = rayBeingDrawn;
            syncToCurrent();
        }

        private void syncToCurrent()
        {
            //root of tree
            if (ray.Parent == null)
            {
                if (ray.Data.Type != RTRay.RayType.NoHit)
                    StartCoroutine(highlightStepWait(globalPrev, 0, 2));
            }
            else
            {
                switch (ray.Data.Type)
                {
                    case RTRay.RayType.Light:
                        StartCoroutine(highlightStepWait(globalPrev, 3, 4));
                        break;
                    case RTRay.RayType.Reflect:
                        StartCoroutine(highlightStepWait(3, 5, 6));
                        break;
                    case RTRay.RayType.Refract:
                        StartCoroutine(highlightStepWait(3, 7, 8));
                        break;
                }
            }
        }

        // draws light ray first but receives signals from light & reflective at same time???
        //highlist cnt first, wait, then future
        IEnumerator highlightStepWait(int localPrev, int cnt, int future)
        {
            highlightStep(localPrev, cnt);
            yield return new WaitForSeconds(1 / rayManager.Speed);
            highlightStep(globalPrev, future);
        }

        private void highlightStep(int prevStep, int newStep)
        {
            if (prevStep >= 0)
                steps[prevStep].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            steps[newStep].color = new Color(0.0f, 1.0f, 1.0f, 1.0f);
            globalPrev = newStep;
        }

        private void resetColor()
        {
            foreach(TextMeshProUGUI step in steps) 
                step.color = Color.white;   
        }
    }
}
