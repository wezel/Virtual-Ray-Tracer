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
        [SerializeField]
        private CameraController cameraController;

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
        [SerializeField, Range(0, 1000)]
        private int rayTypeChange;
        [SerializeField, Range(0, 1000)]
        private int lightChange;
        [SerializeField, Range(0, 1000)]
        private int meshChange;

        private int meshChangeCnt;
        private int rayChangeCnt;
        private int lightChangeCnt;
        private bool changedAttenuation;

        /// <summary>
        /// Bitmask for all possible ways of visualizing rays
        /// </summary>
        private int rayTypesEnabled;

        private float distance;
        

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
            lights[0].gameObject.SetActive(true);
            currentObject = 0;
            currentLight = 0;
            meshChangeCnt = 0;
            rayChangeCnt = 0;
            lightChangeCnt = 0;
            changedAttenuation = true;
            rayTypesEnabled = 0;

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
            if (!changedAttenuation && Random.value > 0.7f) // Small chance to flip attenuation. Prevent doing it twice in a row
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
        /// Rotate the camera and change raytype/light/mesh when the amount of ticks is reached.
        /// </summary>
        private void FixedUpdate()
        {
            RotateCamera();
            if ((rayChangeCnt = (rayChangeCnt + 1) % rayTypeChange) == 0)
                ChangeRayType();
            if ((lightChangeCnt = (lightChangeCnt + 1) % lightChange) == 0)
                ChangeLight();
            if ((meshChangeCnt = (meshChangeCnt + 1) % meshChange) == 0)
                SwitchScene();
            if (angle >= maxAngle || angle <= minAngle)
                positive = !positive;
        }
    }
}
