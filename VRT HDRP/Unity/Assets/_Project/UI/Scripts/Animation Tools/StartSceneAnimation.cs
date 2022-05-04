using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RT_Scene;
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
        
        private int currentObject;
    
        private Transform cameraTransform;
    
        private bool positive = true;
    
        private float angle;
    
        [SerializeField]
        private Transform target;
        [SerializeField]
        private float rotationSpeed;
        [SerializeField]
        private float maxAngle;
        [SerializeField]
        private float minAngle;

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
            scene.AddMesh(objects[currentObject]);
            objects[currentObject].gameObject.SetActive(true);
        }
    
        /// <summary>
        /// Rotate the camera and if an endpoint is reach pick the next object to be shown.
        /// </summary>
        private void FixedUpdate()
        {
            RotateCamera();
            if (!(angle >= maxAngle) && !(angle <= minAngle)) return;
        
            positive = !positive;
            SwitchScene();
        }
    }
}
