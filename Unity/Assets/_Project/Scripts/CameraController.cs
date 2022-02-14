using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts
{
    /// <summary>
    /// A camera controller that mimics the controls and behavior of the Unity editor camera. Adapted from
    /// http://wiki.unity3d.com/index.php?title=MouseOrbitZoom.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private RectTransform inputBlocker;
        public bool InputBlockerHovered { get; set; }
    
        public Transform Target;
        public float InitialDistance = 5.0f;
        public float MaxDistance = 25.0f;
        public float MinDistance = 0.5f;
        public float YMaxLimit = 90.0f;
        public float YMinLimit = -90.0f;
        public float OrbitSpeed = 4.0f;
        public float PanSpeed = 2.0f;
        public float ZoomSpeed = 1.0f;

        private float xDegrees = 0.0f;
        private float yDegrees = 0.0f;
        private float distance = 0.0f;
    
        private bool zoom = false;
        private bool orbiting = false;
        private bool panning = false;
        private bool mode = false;

        void Start() { Initialize(); }

        void OnEnable() { Initialize(); }

        /// <summary>
        /// Initialize this camera controller. Will rotate the camera to look at the provided <see cref="Target"/>. If no
        /// target is provided a new target will be placed <see cref="InitialDistance"/> in front of the camera.
        /// </summary>
        public void Initialize()
        {
            // If there is no target, create a target at the given initial distance from the camera's current viewpoint.
            if (!Target)
            {
                GameObject go = new GameObject("Camera Target");
                go.transform.position = transform.position + (transform.forward * InitialDistance);
                Target = go.transform;
            }

            // Store the distance to the target and camera rotation.
            distance = Vector3.Distance(transform.position, Target.position);
            xDegrees = transform.rotation.eulerAngles.y;
            yDegrees = transform.rotation.eulerAngles.x;

            // Calculate the initial position based on our rotation and the distance to the target.
            transform.position = Target.position - (transform.rotation * Vector3.forward * distance);
        }

        public void SetCursor()
        {
            if(zoom || panning || orbiting)
                return;
            GlobalSettings.Get().SetCursor(CursorType.ModeCursor);
        }

        public void ResetCursor()
        {
            if(zoom || panning || orbiting)
                return;
        
            GlobalSettings.Get().ResetCursor();
        }

        private void DisableBlocker()
        {
            GlobalSettings globalSettings = GlobalSettings.Get();
            if(mode) {
                globalSettings.SetCursor(CursorType.ModeCursor);
                return;
            }
            globalSettings.ResetCursor();
            inputBlocker.gameObject.SetActive(false);
            InputBlockerHovered = false;
        }

        private void PanningUpdate()
        {
            float xDistance = 0.0f;
            float yDistance = 0.0f;
            
            // Grab the rotation of the camera so we can move in a pseudo local XY space.
            if (Input.GetMouseButton(2))
            {
                xDistance = -Input.GetAxis("Mouse X") * 0.01f;
                yDistance = -Input.GetAxis("Mouse Y") * 0.01f;
            }

            // And we pan with the arrow keys
            if (Input.GetKey(KeyCode.LeftArrow))
                xDistance -= Time.deltaTime * 0.5f;
            if (Input.GetKey(KeyCode.RightArrow))
                xDistance += Time.deltaTime * 0.5f;
            if (Input.GetKey(KeyCode.UpArrow))
                yDistance += Time.deltaTime * 0.5f;
            if (Input.GetKey(KeyCode.DownArrow))
                yDistance -= Time.deltaTime * 0.5f;

            // we pan by way of transforming the target in screen space.
            // Grab the rotation of the camera so we can move in a pseudo local XY space.
            Target.rotation = transform.rotation;
            Target.Translate(Vector3.right * (xDistance * PanSpeed * distance));
            Target.Translate(Vector3.up * (yDistance * PanSpeed * distance));

            if (!(Input.GetMouseButton(2) ||
                  Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
                  Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)))
            {
                panning = false;
                DisableBlocker();
            }
        }

        private void OrbitingUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                xDegrees += Input.GetAxis("Mouse X") * OrbitSpeed;
                yDegrees -= Input.GetAxis("Mouse Y") * OrbitSpeed;
            }
            
            if (Input.GetKey(KeyCode.LeftArrow))
                xDegrees += Time.deltaTime * 20.0f * OrbitSpeed;
            if (Input.GetKey(KeyCode.RightArrow))
                xDegrees -= Time.deltaTime * 20.0f * OrbitSpeed;
            if (Input.GetKey(KeyCode.UpArrow))
                yDegrees += Time.deltaTime * 20.0f * OrbitSpeed;
            if (Input.GetKey(KeyCode.DownArrow))
                yDegrees -= Time.deltaTime * 20.0f * OrbitSpeed;

            // Clamp the vertical axis for the orbit.
            yDegrees = ClampAngle(yDegrees, YMinLimit, YMaxLimit);

            // Set desired camera rotation.
            Quaternion desiredRotation = Quaternion.Euler(yDegrees, xDegrees, 0);

            // Linearly interpolate camera rotation to desired rotation. I have no idea why, but just setting the
            // rotation to desired does not work (its not normalization). Lerp seems to produce a quaternion with the
            // signs of the coordinates flipped.
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, 1.0f);

            if (!(Input.GetMouseButton(0) ||
                  Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
                  Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)))
            {
                orbiting = false;
                DisableBlocker();
            }

        }
    
        private void OnlyOneInputPicker()
        {

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                inputBlocker.gameObject.SetActive(true);
                mode = true;
            }
        
            // If the user is zooming, orbiting or panning we calculate their position

            if (zoom)
            {
                distance += Input.GetAxis("Mouse Y") * ZoomSpeed * 0.125f * Mathf.Abs(distance);

                if (!Input.GetMouseButton(1))
                {
                    zoom = false;
                    DisableBlocker();
                }

                return;
            }

            if (orbiting)
            {
                OrbitingUpdate();
                return;
            }
        
            if (panning)
            {
                PanningUpdate();
                return;
            }

            if (mode && !Input.GetKey(KeyCode.LeftControl))
            {
                inputBlocker.gameObject.SetActive(false);
                GlobalSettings.Get().ResetCursor();
                InputBlockerHovered = false;
                mode = false;
            }

            if (EventSystem.current.IsPointerOverGameObject() && !InputBlockerHovered)
                return;

            // The inputs are below this line
        
            // If scrollWheel is used change zoom. This one is not exclusive.
            distance -= Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed * Mathf.Abs(distance);

            // If the left control is pressed and.... 
            if (Input.GetKey(KeyCode.LeftControl))
            {

                // The right mouse button we activate zoom.
                if (Input.GetMouseButtonDown(1))
                {
                    zoom = true;
                    GlobalSettings.Get().SetCursor(CursorType.ZoomCursor);
                    return;
                }

                // The left mouse button or Arrow keys we activate orbiting.
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftArrow) ||
                    Input.GetKeyDown(KeyCode.RightArrow) ||
                    Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    orbiting = true;
                    GlobalSettings.Get().SetCursor(CursorType.RotateCursor);
                    return;
                }
            }
        
            // If the middle mouse is pressed, or the arrow keys we activate panning.
            if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                inputBlocker.gameObject.SetActive(true);
                panning = true;
                GlobalSettings.Get().SetCursor(CursorType.GrabCursor);
                return;
            }
        }

        void Update()
        {
        
            // If we are over the ui we don't allow the user to start any of these actions.

            OnlyOneInputPicker();


            // TODO Allow for free zoom movement not clamped and not as a function of distance just a fixed step amount.
            // TODO Add function to focus on objects and have zoom like it is now.
            // check if distance is still within it's bounds
            distance = Mathf.Clamp(distance, MinDistance, MaxDistance);

            // Update the position based on our rotation and the distance to the target.
            transform.position = Target.position - (transform.rotation * Vector3.forward * distance);
        
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
    }
}
