using _Project.Ray_Tracer.Scripts;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using _Project.Ray_Tracer.Scripts.Utility;
using System.Collections;
using UnityEngine.UI;

//TODO: fix error when tabbing out => make it so gameobj doesnt always get turned off 
namespace _Project.UI.Scripts.Control_Panel
{
    public class VisualizationProperties : MonoBehaviour
    {
        //text fields
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

        //pause/play button fileds
        [SerializeField]
        private Sprite pauseSprite;
        [SerializeField]
        private Sprite playSprite;
        [SerializeField]
        private Button pauseButton;
        [SerializeField]
        private bool paused = false;

        private List<TextMeshProUGUI> steps;
        private int globalPrev;
        private bool hasBeenShown = false;
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
        }

        private void Start()
        {
            rayManager = RayManager.Get();
            paused = rayManager.Paused;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            //reset color - needed when we exit visprops and then start it again.
            resetColor();
            hasBeenShown = true;
            rayManager.drawingNewRay += RMDrawingNewRay;
            if (rayManager.DrawingRay != null)
            {
                this.ray = rayManager.DrawingRay;
                syncToCurrent();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            //add and remove listeners to not get corountine error 
            if (hasBeenShown)
                rayManager.drawingNewRay -= RMDrawingNewRay;
        }

        public void Pause()
        {
            paused = !paused;
            rayManager.Paused = paused;
            setButtonSprite(paused);
        }

        private void setButtonSprite(bool paused)
        {
            if (paused)
                pauseButton.image.sprite = playSprite;
            else
                pauseButton.image.sprite = pauseSprite;
        }

        //gets notification from ray manager that a new ray is being drawn
        private void RMDrawingNewRay(object sender, TreeNode<RTRay> rayBeingDrawn)
        {
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

        //highlist cnt first, wait, then future
        IEnumerator highlightStepWait(int localPrev, int cnt, int future)
        {
            float waitTime = ray.Data.Length / rayManager.Speed;
            highlightStep(localPrev, cnt);
            yield return new WaitForSeconds(1/rayManager.Speed);
            highlightStep(globalPrev, future);
        }

        private void highlightStep(int prevStep, int newStep)
        {
            //first get ray color
            Color rayColor = rayManager.GetRayTypeMaterial(ray.Data.Type).color;

            if (prevStep >= 0)
                steps[prevStep].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            steps[newStep].color = rayColor;
            globalPrev = newStep;
        }

        private void resetColor()
        {
            foreach(TextMeshProUGUI step in steps) 
                step.color = Color.white;   
        }
    }
}
