using System;
using UnityEngine;

namespace RuntimeHandle
{
    /**
     * Created by Peter @sHTiF Stefcek 20.10.2020
     */
    public class PositionAxis : HandleBase
    {
        protected Vector3 _startPosition;
        protected Vector3 _axis;
        protected Vector3 _perp;
        protected Plane _plane;
        protected Vector3 _interactionOffset;

        public PositionAxis Initialize(RuntimeTransformHandle p_runtimeHandle, Vector3 p_axis, Vector3 p_perp,
            Color p_color)
        {
            _parentTransformHandle = p_runtimeHandle;
            _axis = p_axis;
            _perp = p_perp;
            _defaultColor = p_color;
            
            InitializeMaterial();

            transform.SetParent(p_runtimeHandle.transform, false);

            GameObject o = new GameObject();
            o.transform.SetParent(transform, false);
            MeshRenderer mr = o.AddComponent<MeshRenderer>();
            mr.material = _material;
            MeshFilter mf = o.AddComponent<MeshFilter>();
            mf.mesh = MeshUtils.CreateCone(2f, .04f, .04f, 8, 1);
            MeshCollider mc = o.AddComponent<MeshCollider>();
            mc.sharedMesh = MeshUtils.CreateCone(2f, .1f, .1f, 8, 1);
            o.transform.localRotation = Quaternion.FromToRotation(Vector3.up, p_axis);

            o = new GameObject();
            o.transform.SetParent(transform, false);
            mr = o.AddComponent<MeshRenderer>();
            mr.material = _material;
            mf = o.AddComponent<MeshFilter>();
            mf.mesh = MeshUtils.CreateCone(.4f, .2f, .0f, 8, 1);
            mc = o.AddComponent<MeshCollider>();
            o.transform.localRotation = Quaternion.FromToRotation(Vector3.up, _axis);
            o.transform.localPosition = p_axis * 2;

            return this;
        }

        public override void Interact(Vector3 p_previousPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float d = 0.0f;
            _plane.Raycast(ray, out d);

            Vector3 hitPoint = ray.GetPoint(d);

            Vector3 rperp = _parentTransformHandle.space == HandleSpace.LOCAL
                ? _parentTransformHandle.target.rotation * _perp
                : _perp;
            Vector3 axis = _parentTransformHandle.space == HandleSpace.LOCAL
                ? _parentTransformHandle.target.rotation * _axis
                : _axis;
            Vector3 projected = Vector3.ProjectOnPlane(axis, rperp);

            float amount = Vector3.Dot(hitPoint + _interactionOffset - _startPosition, projected);
            Vector3 offset = axis * amount;

            Vector3 snapping = _parentTransformHandle.positionSnap;
            float snap = Vector3.Scale(snapping, axis).magnitude;
            if (snap != 0 && _parentTransformHandle.snappingType == HandleSnappingType.RELATIVE)
            {
                if (snapping.x != 0) offset.x = Mathf.Round(offset.x / snapping.x) * snapping.x;
                if (snapping.y != 0) offset.y = Mathf.Round(offset.y / snapping.y) * snapping.y;
                if (snapping.z != 0) offset.z = Mathf.Round(offset.z / snapping.z) * snapping.z;
            }

            Vector3 position = _startPosition + offset;

            if (snap != 0 && _parentTransformHandle.snappingType == HandleSnappingType.ABSOLUTE)
            {
                if (snapping.x != 0) position.x = Mathf.Round(position.x / snapping.x) * snapping.x;
                if (snapping.y != 0) position.y = Mathf.Round(position.y / snapping.y) * snapping.y;
                if (snapping.x != 0) position.z = Mathf.Round(position.z / snapping.z) * snapping.z;
            }

            _parentTransformHandle.target.position = position;

            base.Interact(p_previousPosition);
        }
        
        public override void StartInteraction(Vector3 p_hitPoint)
        {
            Vector3 rperp = _parentTransformHandle.space == HandleSpace.LOCAL
                ? _parentTransformHandle.target.rotation * _perp
                : _perp;

            // If the camera is in line (angle = 90) with the plane normal to rperp we should take the other vector
            // perpendicular to the axis and transform using the plane normal to that.
            Vector3 cameraForward = _parentTransformHandle.handleCamera.transform.forward;
            float angle = Vector3.Angle(rperp, cameraForward);
            if (Mathf.Abs(angle - 90.0f) < 15.0f)
            {
                // Take the other vector perpendicular to the axis. For example if _axis is the x-axis and _perp is the
                // y-axis we can now take the z-axis for the new _perp.
                _perp = Vector3.Cross(_axis, _perp).normalized;
                rperp = _parentTransformHandle.space == HandleSpace.LOCAL
                    ? _parentTransformHandle.target.rotation * _perp
                    : _perp;
            }

            _plane = new Plane(rperp, _parentTransformHandle.target.position);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float d = 0.0f;
            _plane.Raycast(ray, out d);

            Vector3 hitPoint = ray.GetPoint(d);
            _startPosition = _parentTransformHandle.target.position;
            _interactionOffset = _startPosition - hitPoint;
        }

        private void Update()
        {
            // Determine the angle between the camera forward and the axis.
            Vector3 cameraForward = _parentTransformHandle.handleCamera.transform.forward;
            Vector3 axis = _parentTransformHandle.space == HandleSpace.LOCAL
                ? _parentTransformHandle.target.rotation * _axis
                : _axis;
            float angle = Vector3.Angle(axis, cameraForward);

            // Hide the axis if the angle is shallow enough. This prevents non-smooth motion.
            float threshold = _parentTransformHandle.hideHandleAngle; 
            bool shallow = Mathf.Abs(angle) < threshold || Mathf.Abs(angle - 180.0f) < threshold;
            foreach (Transform child in transform)
                child.gameObject.SetActive(!shallow);
        }
    }
}