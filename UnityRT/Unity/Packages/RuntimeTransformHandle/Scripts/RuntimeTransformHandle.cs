using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RuntimeHandle
{
    /**
     * Created by Peter @sHTiF Stefcek 21.10.2020
     */
    public class RuntimeTransformHandle : MonoBehaviour
    {
        public static float MOUSE_SENSITIVITY = 1;
        
        public HandleAxes axes = HandleAxes.XYZ;
        public HandleSpace space = HandleSpace.LOCAL;
        public HandleType type = HandleType.POSITION;
        public HandleSnappingType snappingType = HandleSnappingType.RELATIVE;

        public Vector3 positionSnap = Vector3.zero;
        public float rotationSnap = 0;
        public Vector3 scaleSnap = Vector3.zero;

        public bool autoScale = false;
        public float autoScaleFactor = 1;
        public Camera handleCamera;

        public float hideHandleAngle = 5.0f;

        private Vector3 _previousMousePosition;
        private HandleBase _previousAxis;
        
        private HandleBase _draggingHandle;

        private HandleType _previousType;
        private HandleAxes _previousAxes;

        private PositionHandle _positionHandle;
        private RotationHandle _rotationHandle;
        private ScaleHandle _scaleHandle;

        public Transform target;

        void Start()
        {
            if (handleCamera == null)
                handleCamera = Camera.main;

            _previousType = type;

            if (target == null)
                target = transform;

            CreateHandles();

            if (autoScale)
                AutoScaleHandle();
        }

        void CreateHandles()
        {
            switch (type)
            {
                case HandleType.POSITION:
                    _positionHandle = gameObject.AddComponent<PositionHandle>().Initialize(this);
                    break;
                case HandleType.ROTATION:
                    _rotationHandle = gameObject.AddComponent<RotationHandle>().Initialize(this);
                    break;
                case HandleType.SCALE:
                    _scaleHandle = gameObject.AddComponent<ScaleHandle>().Initialize(this);
                    break;
            }
        }

        private void OnDisable()
        {
            transform.localScale = Vector3.zero;
        }

        void Clear()
        {
            _draggingHandle = null;
            
            if (_positionHandle) _positionHandle.Destroy();
            if (_rotationHandle) _rotationHandle.Destroy();
            if (_scaleHandle) _scaleHandle.Destroy();
        }
        
        private void AutoScaleHandle()
        {
            // Project the handle position on to the screen and get a second screen point the desired distance from it.
            float screenSize = 50.0f; // Desired vertical screen space size of the handle in pixels.
            Vector3 screenPoint1 = handleCamera.WorldToScreenPoint(transform.position);
            Vector3 screenPoint2 = new Vector3(screenPoint1.x, screenPoint1.y + screenSize, screenPoint1.z);

            // Place the screen points in the world and determine the distance between them. This distance can then be
            // used to scale the handle to have the desired size on the screen.
            Vector3 worldPoint1 = handleCamera.ScreenToWorldPoint(screenPoint1); // Could also be transform.position.
            Vector3 worldPoint2 = handleCamera.ScreenToWorldPoint(screenPoint2);
            float worldSize = Vector3.Distance(worldPoint1, worldPoint2);

            transform.localScale = Vector3.one * autoScaleFactor * worldSize;
        }

        void HandleOverEffect(HandleBase p_axis)
        {
            if (_draggingHandle == null && _previousAxis != null && _previousAxis != p_axis)
            {
                _previousAxis.SetDefaultColor();
            }

            if (p_axis != null && _draggingHandle == null)
            {
                p_axis.SetColor(Color.yellow);
            }

            _previousAxis = p_axis;
        }

        private void GetHandle(ref HandleBase p_handle, ref Vector3 p_hitPoint)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity,LayerMask.GetMask("Gizmos"))) 
                return;

            p_handle = hit.collider.gameObject.GetComponentInParent<HandleBase>();
            if (p_handle == null) return;
            
            p_hitPoint = hit.point;
        }

        static public RuntimeTransformHandle Create(Transform p_target, HandleType p_handleType)
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.localScale = Vector3.zero;
            RuntimeTransformHandle runtimeTransformHandle = gameObject.AddComponent<RuntimeTransformHandle>();
            runtimeTransformHandle.target = p_target;
            runtimeTransformHandle.type = p_handleType;
            return runtimeTransformHandle;
        }

        void Update()
        {
            if (_previousType != type || _previousAxes != axes)
            {
                Clear();
                CreateHandles();
                _previousType = type;
                _previousAxes = axes;
            }

            HandleBase handle = null;
            Vector3 hitPoint = Vector3.zero;
            GetHandle(ref handle, ref hitPoint);
            HandleOverEffect(handle);

            if (Input.GetMouseButton(0) && _draggingHandle != null)
            {
                _draggingHandle.Interact(_previousMousePosition);
            }

            if (Input.GetMouseButtonDown(0) && handle != null)
            {
                _draggingHandle = handle;
                _draggingHandle.StartInteraction(hitPoint);
            }

            if (Input.GetMouseButtonUp(0) && _draggingHandle != null)
            {
                _draggingHandle.EndInteraction();
                _draggingHandle = null;
            }

            _previousMousePosition = Input.mousePosition;

            transform.position = target.transform.position;
            if (space == HandleSpace.LOCAL || type == HandleType.SCALE) 
                transform.rotation = target.transform.rotation;
            else
                transform.rotation = Quaternion.identity;

            if (autoScale)
                AutoScaleHandle();
        }
    }
}