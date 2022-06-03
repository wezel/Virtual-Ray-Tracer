using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using _Project.Scripts;
using UnityEngine;

namespace _Project.UI.Scripts.Animation_Tools
{
    /// <summary>
    /// This class is specifically made to animate the camera in the opening scene. It allows the camera to automatically
    /// rotate around a given target with a specified speed. A min and max angle can be specified between which the camera
    /// will bounce back and forth. At each bounce a new object is selected to be shown.
    /// 
    /// </summary>
    public class StartSceneAnimation : MonoBehaviour
    {
        [SerializeField] private CameraController cameraController;

        [SerializeField] 
        private List<RTMesh> objects;
        [SerializeField]
        private List<RTLight> lights;

        private int currentObject;
        private int currentLight;

        private Transform cameraTransform;
    
        private bool positive = true;
    
        [SerializeField]
        private float angle;
    
        [SerializeField]
        private Transform target;
        [SerializeField]
        private float rotationSpeed;
        [SerializeField]
        private float maxAngle;
        [SerializeField]
        private float minAngle;
        [SerializeField, Range(60, 300)]
        private int rayTypeChange;
        [SerializeField, Range(60, 300)]
        private int lightChange;

        private int rayChangeCnt = 0;
        private int lightChangeCnt = 0;
        private bool changedAttenuation = true;

        private int rayTypesEnabled = 0;

        private float distance = 0.0f;
        

        /// <summary>
        /// Prepare the Class to do its job the right way.
        /// </summary>
        public void Start()
        {
            cameraTransform = cameraController.transform;
            angle = cameraTransform.eulerAngles.y;
            minAngle = angle - minAngle;
            maxAngle = angle + maxAngle;
            objects[0].gameObject.SetActive(true);
            currentObject = objects.Count - 1;
            currentLight = 0;

            // Store the distance to the target and camera rotation.
            distance = Vector3.Distance(cameraTransform.position, target.position);

            // Calculate the initial position based on the distance.
            cameraTransform.position = target.position - (cameraTransform.rotation * Vector3.forward * distance);
        }

        /// <summary>
        /// Rotate the scene by the given speed in the right direction.
        /// </summary>
        private void RotateCamera()
        {
            angle += positive ? rotationSpeed : -rotationSpeed;
        
            // Set desired camera rotation.
            Quaternion desiredRotation = Quaternion.Euler(cameraTransform.eulerAngles.x, angle, 0);

            // Linearly interpolate camera rotation to desired rotation. I have no idea why, but just setting the
            // rotation to desired does not work (its not normalization). Lerp seems to produce a quaternion with the
            // signs of the coordinates flipped.
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, desiredRotation, 1.0f);
            
            cameraTransform.position = target.position - cameraTransform.rotation * Vector3.forward * distance;
        }

        /// <summary>
        /// Deactivate the current object and activate the next object on the list. If the end of the list is reached
        /// loop back to the start of the list. 
        /// </summary>
        private void SwitchScene()
        {
            RTScene scene = RTSceneManager.Get().Scene;
            objects[currentObject].gameObject.SetActive(false);
            scene.RemoveMesh(objects[currentObject]);
            currentObject++;
            if (currentObject == objects.Count) currentObject = 0;
            objects[currentObject].gameObject.SetActive(true);
            scene.AddMesh(objects[currentObject]);
        }

        private void ChangeRayType()
        {
            rayTypesEnabled = (rayTypesEnabled + 1) % 8;
            RayManager rayManager = RayManager.Get();
            rayManager.RayTransparencyEnabled = (1 << 0 & rayTypesEnabled) != 0;
            rayManager.RayDynamicRadiusEnabled = (1 << 1 & rayTypesEnabled) != 0;
            rayManager.RayColorContributionEnabled = (1 << 2 & rayTypesEnabled) != 0;
        }

        private void ChangeLight()
        {
            if (!changedAttenuation && Random.value > 0.7f)
            {
                lights[currentLight].LightDistanceAttenuation = !lights[currentLight].LightDistanceAttenuation;
                changedAttenuation = true;  // Make sure it doesn't keep changing 
            }
            else
            {
                RTScene scene = RTSceneManager.Get().Scene;
                lights[currentLight].gameObject.SetActive(false);
                scene.RemoveLight(lights[currentLight]);
                currentLight = (currentLight + 1) % lights.Count;
                lights[currentLight].gameObject.SetActive(true);
                scene.AddLight(lights[currentLight]);
                changedAttenuation = false;
            }
        }


        /// <summary>
        /// Rotate the camera and if an endpoint is reach pick the next object to be shown.
        /// </summary>
        private void FixedUpdate()
        {
            RotateCamera();
            if ((rayChangeCnt = (rayChangeCnt + 1) % rayTypeChange) == 0) ChangeRayType();
            if ((lightChangeCnt = (lightChangeCnt + 1) % lightChange) == 0) ChangeLight();
            if (!(angle >= maxAngle) && !(angle <= minAngle)) return;

            // To not change multiple things quickly after each other, clamp the counts
            rayChangeCnt = Mathf.Clamp(rayChangeCnt, 0, rayTypeChange - 50);
            lightChangeCnt = Mathf.Clamp(lightChangeCnt, 0, lightChange - 50);

            positive = !positive;
            SwitchScene();
        }
    }
}
