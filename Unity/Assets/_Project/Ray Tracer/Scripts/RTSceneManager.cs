using System;
using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RT_Scene;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Point_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Area_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera;
using _Project.Ray_Tracer.Scripts.Utility;
using _Project.UI.Scripts;
using _Project.UI.Scripts.Control_Panel;
using RuntimeHandle;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Spot_Light;
using _Project.Scripts;

namespace _Project.Ray_Tracer.Scripts
{
    /// <summary>
    /// Manages a ray tracer scene. On <see cref="Awake"/> it will find all objects in the Unity scene with
    /// <see cref="RTCamera"/>, <see cref="RTMesh"/> and <see cref="RTPointLight"/> components and construct an
    /// <see cref="RTScene"/> from them.
    /// </summary>
    public class RTSceneManager : MonoBehaviour
    {
        /// <summary>
        /// The scene this scene manager manages.
        /// </summary>
        public RTScene Scene { get; private set; }

        /// <summary>
        /// The most recent low resolution preview image produced by the ray tracer.
        /// </summary>
        public RTImage Image { get; private set; }

        [Header("Scene Objects")]
        [SerializeField] private RTCamera cameraPrefab;
        [SerializeField] private RTPointLight pointLightPrefab;
        [SerializeField] private RTSpotLight spotLightPrefab;
        [SerializeField] private RTAreaLight areaLightPrefab;
        [SerializeField] private RTMesh goatPrefab;
        [SerializeField] private RTMesh spherePrefab;
        [SerializeField] private RTMesh cubePrefab;
        [SerializeField] private RTMesh capsulePrefab;
        [SerializeField] private RTMesh cylinderPrefab;
        [SerializeField] private RTMesh prismPrefab;
        [SerializeField] private RTMesh barrelPrefab;
        [SerializeField] private RTMesh wineglassPrefab;
        [SerializeField] private RTMesh meshPrefab;

        [Header("UI")]
        [SerializeField] private Color SelectionColor;
        [SerializeField] private ControlPanel ControlPanel;
        [SerializeField] private TMP_Dropdown HandleTypeDropdown;
        [SerializeField] private TMP_Dropdown HandleSpaceDropdown;

        [Header("Gizmos")]
        [SerializeField] private RuntimeTransformHandle transformHandle;
        
        [SerializeField] private float translationSnap = 0.0f;
        [SerializeField] private float rotationSnap = 0.0f;
        [SerializeField] private float scaleSnap = 0.0f;

        [Header("Objects")]
        [SerializeField] private bool deleteAllowed = false;
        [SerializeField, Range(0, 12)] private int pointSpotLightLimit = 12;
        [SerializeField, Range(0,  5)] private int areaLightLimit = 5;
        public bool DeleteAllowed { get => deleteAllowed; }
        public int PointSpotLightLimit { get => pointSpotLightLimit; }
        public int AreaLightLimit { get => areaLightLimit; }

        [Serializable]
        public class Event : UnityEvent { };
        public Event OnTranslationMode, OnRotationMode, OnScaleMode, OnLocalSpace, OnGlobalSpace, OnDeselect, OnObjectDeleted;

        private static RTSceneManager instance = null;
        private Selection selection = new Selection();
        private Transform previousTransform;

        private HandleSpace handleSpace = HandleSpace.WORLD;
        
        /// <summary>
        /// The object type. 
        /// The value represents the amount of points to unlock the object in sandbox mode.
        /// </summary>
        public enum ObjectType
        {
            Sphere = 8000,
            Cube = 8500,
            Cylinder = 9000,
            Barrel = 9500,
            Capsule = 10000,
            Prism = 10500,
            Goat = 11000,
            Wineglass = 11500,
            PointLight = 12000,
            SpotLight = 13000,
            AreaLight = 14000
        }

        /// <summary>
        /// A simple data class that represents a selected object. The object may be an <see cref="RTCamera"/>, an
        /// <see cref="RTLight"/> or an <see cref="RTMesh"/>.
        /// </summary>
        class Selection
        {
            /// <summary>
            /// The type of the selected object. May be <see cref="RTCamera"/>, <see cref="RTLight"/> or
            /// <see cref="RTMesh"/>.
            /// </summary>
            public Type Type { get; private set; }

            /// <summary>
            /// The <see cref="UnityEngine.Transform"/> of the selected object.
            /// </summary>
            public Transform Transform;

            /// <summary>
            /// The <see cref="RTCamera"/> component attached to the selected object. Will be <c>null</c> if there is
            /// none.
            /// </summary>
            public RTCamera Camera
            {
                get
                {
                    if (Type == typeof(RTCamera)) return (RTCamera)selected;
                    return null;
                }
                set
                {
                    if (value == null) return;
                    selected = value;

                    Type = typeof(RTCamera);
                    Transform = value.transform;
                    Empty = false;
                }
            }

            /// <summary>
            /// The <see cref="RTPointLight"/> component attached to the selected object. Will be <c>null</c> if there is
            /// none.
            /// </summary>
            public RTLight Light
            {
                get
                {
                    if (Type == typeof(RTPointLight)) return (RTPointLight)selected;
                    if (Type == typeof(RTSpotLight)) return (RTSpotLight)selected;
                    if (Type == typeof(RTAreaLight)) return (RTAreaLight)selected;
                    return null;
                }
                set
                {
                    if (value == null) return;
                    selected = value;

                    if (value.Type == RTLight.RTLightType.Point)
                        Type = typeof(RTPointLight);
                    else
                        Type = value.Type == RTLight.RTLightType.Spot ? typeof(RTSpotLight) : typeof(RTAreaLight);
                    Transform = value.transform;
                    Empty = false;
                }
            }

            /// <summary>
            /// The <see cref="RTMesh"/> component attached to the selected object. Will be <c>null</c> if there is
            /// none.
            /// </summary>
            public RTMesh Mesh 
            {
                get
                {
                    if (Type == typeof(RTMesh)) return (RTMesh)selected;
                    return null;
                }
                set
                {
                    if (value == null) return;
                    selected = value;

                    Type = typeof(RTMesh);
                    Transform = value.transform;
                    Empty = false;
                }
            }
            
            /// <summary>
            /// A selection is empty if an object is selected that has no <see cref="RTCamera"/>, <see cref="RTLight"/>
            /// or <see cref="RTMesh"/> component attached. <see cref="Transform"/> does not have to be <c>null</c> for
            /// a selection to be considered empty.
            /// </summary>
            public bool Empty { get; private set; }

            private Component selected;

            public Selection()
            {
                Type = null;
                Transform = null;
                Empty = true;
                selected = null;
            }
        }

        /// <summary>
        /// Get the current SceneManager instance.
        /// </summary>
        /// <returns> The current SceneManager instance. </returns>
        public static RTSceneManager Get()
        {
            return instance;
        }

        /// <summary>
        /// Make <paramref name="newSelection"/> the selected object. Nothing is done if <paramref name="newSelection"/> is
        /// already the selected object or if we try to select an object that is has no <see cref="RTCamera"/>,
        /// <see cref="RTLight"/> or <see cref="RTMesh"/> component.
        /// </summary>
        /// <param name="newSelection"> The new selected object. </param>
        public void Select(Transform newSelection)
        {

            // Do nothing if what we selected is already the selected object.
            if (selection.Transform == newSelection)
            {
                selection.Mesh?.OnMeshSelected?.Invoke();
                selection.Camera?.OnCameraSelected?.Invoke();
                selection.Light?.OnLightSelected?.Invoke();

                return;
            }

            // Do nothing if we selected something other than a camera, light or mesh.
            Selection candidate = DetermineSelection(newSelection);
            if (candidate.Empty)
                return;

            Deselect();
            selection = candidate;

            // Enable UI for the new selected object and outline it.
            if (selection.Type == typeof(RTCamera))
            {
                ControlPanel.ShowCameraProperties(selection.Camera);
                selection.Camera.Color = SelectionColor;
                selection.Camera.OnCameraSelected?.Invoke();
            }
            else if (selection.Type.BaseType == typeof(RTLight))
            {
                ControlPanel.ShowLightProperties(selection.Light);
                selection.Light.Higlight(SelectionColor);
                previousTransform = newSelection;
                selection.Light.OnLightSelected?.Invoke();
            }
            else if (selection.Type == typeof(RTMesh))
            {
                ControlPanel.ShowMeshProperties(selection.Mesh);
                selection.Mesh.Outline.OutlineColor = SelectionColor;
                selection.Mesh.Outline.enabled = true;
                previousTransform = newSelection;
                selection.Mesh.OnMeshSelected?.Invoke();
            }

            transformHandle.target = selection.Transform;
            SetHandleType(transformHandle.type);
            transformHandle.gameObject.SetActive(true);
            UIManager.Get().AddEscapable(DeselectAndInvoke);
        }
        
        public bool HasSelection()
        {
            return !selection.Empty;
        }
        
        /// <summary>
        /// Deselect the currently selected object. Nothing is done if no object is selected.
        /// </summary>
        public void Deselect()
        {
            ControlPanel.ShowRayTracerProperties();
            
            if (selection.Empty)
                return;

            // Deactivate the outline around the selected object.
            if (selection.Type == typeof(RTCamera))
                selection.Camera.ResetColor();
            else if (selection.Type.BaseType == typeof(RTLight))
                selection.Light.ResetHighlight();
            else if (selection.Type == typeof(RTMesh))
                selection.Mesh.Outline.enabled = false;

            selection = new Selection();

            transformHandle.gameObject.SetActive(false);
            UIManager.Get().RemoveEscapable(DeselectAndInvoke);
        }

        public void DeselectAndInvoke()
        {
            Deselect();
            OnDeselect?.Invoke();
        }

        /// <summary>
        /// Delete the currently selected object. Cameras cannot be deleted because there must always be on camera in a
        /// scene.
        /// </summary>
        public void DeleteSelected()
        {
            // Make sure we can delete the object
            if (!deleteAllowed) return;
            if (selection.Empty) return;
            if (selection.Type == typeof(RTCamera)) return; // Camera can't be deleted, there must always be a camera.

            // Deactive the object, so the raytracer doesn't see it anymore
            selection.Transform.gameObject.SetActive(false);

            // Delete the object in our database
            if (selection.Type.BaseType == typeof(RTLight))
                Scene.RemoveLight(selection.Light);
            else if (selection.Type == typeof(RTMesh))
                Scene.RemoveMesh(selection.Mesh);
            
            // Create a local reference
            GameObject gameObject = selection.Transform.gameObject;
            
            // Remove all connections
            previousTransform = null;
            Deselect();
            
            // Destroy
#if UNITY_EDITOR
            DestroyImmediate(gameObject);
#else
            Destroy(gameObject);
#endif
            // Invoke listeners
            OnObjectDeleted?.Invoke();
        }

        public void CreateObject(ObjectType type)
        {
            RTMesh mesh;
            switch (type)
            {
                case ObjectType.PointLight:
                    if (Scene.PointLights.Count + Scene.SpotLights.Count >= pointSpotLightLimit)
                    {
                        Debug.LogError("PointSpotLightLimit reached!");
                        return;
                    }
                    RTPointLight pointLight = Instantiate(pointLightPrefab);
                    pointLight.UpdateLightData();
                    Scene.AddLight(pointLight);
                    Select(pointLight.transform);
                    return;
                case ObjectType.SpotLight:
                    if (Scene.PointLights.Count + Scene.SpotLights.Count >= pointSpotLightLimit)
                    {
                        Debug.LogError("PointSpotLightLimit reached!");
                        return;
                    }
                    RTSpotLight spotLight = Instantiate(spotLightPrefab);
                    spotLight.UpdateLightData();
                    Scene.AddLight(spotLight);
                    Select(spotLight.transform);
                    return;
                case ObjectType.AreaLight:
                    if (Scene.AreaLights.Count >= areaLightLimit + 1)
                    {
                        Debug.LogError("AreaLightLimit reached!");
                        return;
                    }
                    RTAreaLight areaLight = Instantiate(areaLightPrefab);
                    areaLight.UpdateLightData();
                    Scene.AddLight(areaLight);
                    Select(areaLight.transform);
                    return;
                case ObjectType.Sphere:
                    mesh = Instantiate(spherePrefab);
                    break;
                case ObjectType.Cube:
                    mesh = Instantiate(cubePrefab);
                    break;
                case ObjectType.Cylinder:
                    mesh = Instantiate(cylinderPrefab);
                    break;
                case ObjectType.Capsule:
                    mesh = Instantiate(capsulePrefab);
                    break;
                case ObjectType.Goat:
                     mesh = Instantiate(goatPrefab);
                    break;
                case ObjectType.Prism:
                    mesh = Instantiate(prismPrefab);
                    break;
                case ObjectType.Barrel:
                    mesh = Instantiate(barrelPrefab);
                    break;
                case ObjectType.Wineglass:
                    mesh = Instantiate(wineglassPrefab);
                    break;
                default:
                    return;
            }
            Scene.AddMesh(mesh);
            Select(mesh.transform);
        }

        /// <summary>
        /// Update the low resolution render preview image.
        /// </summary>
        /// <param name="colors"> The new pixel colors for the image. </param>
        public void UpdateImage(Color[] colors)
        {
            Image.Reset(Scene.Camera.ScreenWidth, Scene.Camera.ScreenHeight, colors);
        }

        /// <summary>
        /// Determine the selected camera, light or mesh based on <paramref name="selection"/>.
        /// </summary>
        /// <param name="selection"> 
        /// The selected object to get the attached <see cref="RTCamera"/>, <see cref="RTLight"/> or <see cref="RTMesh"/>
        /// component for. 
        /// </param>
        /// <returns> A <see cref="Selection"/> object containing the selected camera, light or mesh. </returns>
        private Selection DetermineSelection(Transform selection)
        {
            Selection result = new Selection();

            if (selection == null)
                return result;

            result.Transform = selection;
            
            // Try to get camera, light and mesh components from the selected object.
            result.Camera = selection.GetComponent<RTCamera>();
            if (!result.Empty) return result;
           
            result.Light = selection.GetComponent<RTPointLight>();
            if (!result.Empty) return result;

            result.Light = selection.GetComponent<RTSpotLight>();
            if (!result.Empty) return result;

            result.Light = selection.GetComponent<RTAreaLight>();
            if (!result.Empty) return result;

            result.Mesh = selection.GetComponent<RTMesh>();
            if (!result.Empty) return result;
            
            return result;
        }
        
        private void SetHandleType(HandleType type)
        {
            // Cameras should not be scaled and lights should not be scaled or rotated. We default to translation.
            bool selectedCamera = selection.Type == typeof(RTCamera);
            bool selectedPointLight = selection.Type == typeof(RTPointLight);
            bool selectedSpotLight = selection.Type == typeof(RTSpotLight);
            bool selectedAreaLight = selection.Type == typeof(RTAreaLight);
            if (type == HandleType.ROTATION && selectedPointLight)
                type = HandleType.POSITION;
            if (type == HandleType.SCALE && (selectedCamera || selectedPointLight || selectedSpotLight))
                type = HandleType.POSITION;

            // Update the dropdown text if necessary. Changing triggers a callback.
            if (HandleTypeDropdown.value != (int)type)
                HandleTypeDropdown.value = (int)type;

            // Scaling only happens in local space, so selecting the space makes no sense when scaling.
            if (type == HandleType.SCALE)
                HandleSpaceDropdown.interactable = false;
            else
                HandleSpaceDropdown.interactable = true;

            // Invoke transformation type changed listeners
            if (type == HandleType.POSITION)
                OnTranslationMode?.Invoke();
            else if (type == HandleType.ROTATION)
                OnRotationMode?.Invoke();
            else
                OnScaleMode?.Invoke();

            // Spotlight may not rotate in z; Arealight may not scale in z
            if ((selectedSpotLight && type == HandleType.ROTATION) ||
                (selectedAreaLight && type == HandleType.SCALE))
                transformHandle.axes = HandleAxes.XY;
            else
                transformHandle.axes = HandleAxes.XYZ;   // reset in case it's set to XY

            transformHandle.type = type;
        }

        private void SetHandleSpace(HandleSpace space)
        {
            handleSpace = space;
            transformHandle.space = space;

            if (space == HandleSpace.LOCAL)
                OnLocalSpace?.Invoke();
            else 
                OnGlobalSpace?.Invoke();

            if (HandleSpaceDropdown.value != (int)space)
                HandleSpaceDropdown.value = (int)space;
        }

        private void OnEvent(ControlPanel.SignalType signal)
        {
            switch (signal)
            {
                case ControlPanel.SignalType.RayTracer:
                    Deselect();
                    break;
                case ControlPanel.SignalType.Camera:
                    Select(Scene.Camera.transform);
                    break;
                case ControlPanel.SignalType.Object:
                    // Lights can be disabled, check if previousTransform's gameObject is active
                    if (previousTransform != null && previousTransform.gameObject.activeInHierarchy)
                        Select(previousTransform);
                    else
                    {
                        Deselect();
                        ControlPanel.ShowEmptyProperties();
                        GlobalManager.EasterEggFound = 1;
                    }
                    break;
            }
        }

        public void SetShadows(bool value)
        {
            LightShadows shadowType = value ? LightShadows.Hard : LightShadows.None;
            foreach (var sceneLight in Scene.PointLights) 
                sceneLight.Shadows = shadowType;
            foreach (var sceneLight in Scene.SpotLights)
                sceneLight.Shadows = shadowType;

            shadowType = value ? LightShadows.Soft : LightShadows.None;
            foreach (var sceneLight in Scene.AreaLights)
                sceneLight.Shadows = shadowType;
        }

        private void Awake()
        {
            instance = this;
            Image = new RTImage(1, 1);

            transformHandle.gameObject.SetActive(false);

            HandleTypeDropdown.onValueChanged.AddListener(type => SetHandleType((HandleType)type));
            HandleSpaceDropdown.onValueChanged.AddListener(space => SetHandleSpace((HandleSpace)space));

            // Find the first camera and all lights and meshes in the Unity scene.
            RTCamera camera = FindObjectOfType<RTCamera>();
            List<RTPointLight> pointLights = new List<RTPointLight>(FindObjectsOfType<RTPointLight>());
            List<RTSpotLight> spotLights = new List<RTSpotLight>(FindObjectsOfType<RTSpotLight>());
            List<RTAreaLight> areaLights = new List<RTAreaLight>(FindObjectsOfType<RTAreaLight>());
            List<RTMesh> meshes = new List<RTMesh>(FindObjectsOfType<RTMesh>());

            // Construct the ray tracer scene with the found objects.
            Scene = new RTScene(camera, pointLights, spotLights, areaLights, meshes);
        }

        private void Start()
        {
            ControlPanel.Subscribe(OnEvent);
        }

        
        // Check whether we are clicking on a transformation handle.
        private void OnLeftClick()
        {
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // If we hit a handle we do nothing
            if (Physics.Raycast(ray, Mathf.Infinity, LayerMask.GetMask("Gizmos"))) return;

            // If we don't hit a handle we try to select the first object we did hit.
            int mask = LayerMask.GetMask("Ray Tracer Objects", "Camera and Lights");
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, mask))
            {
                Select(hit.transform);
                return;
            }

            // If nothing was hit deselect all.
            DeselectAndInvoke();
        }
        
        private void Update()
        {
            // Check if we clicked on anything (unless we are hovered over or in UI, or orbiting the camera).
            bool inUI = EventSystem.current.IsPointerOverGameObject();
            if (Input.GetMouseButtonDown(0) && !inUI)
                OnLeftClick();
            
            // Handle transformation type hot keys.
            if (Input.GetKeyDown(KeyCode.T))
                SetHandleType(HandleType.POSITION);
            if (Input.GetKeyDown(KeyCode.R))
                SetHandleType(HandleType.ROTATION);
            if (Input.GetKeyDown(KeyCode.S))
                SetHandleType(HandleType.SCALE);

            // Handle space hot keys.
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (handleSpace == HandleSpace.WORLD)
                    SetHandleSpace(HandleSpace.LOCAL);
                else
                    SetHandleSpace(HandleSpace.WORLD);
            }

            // Handle snapping hot keys.
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                transformHandle.positionSnap = new Vector3(translationSnap, translationSnap, translationSnap);
                transformHandle.rotationSnap = rotationSnap;
                transformHandle.scaleSnap = new Vector3(scaleSnap, scaleSnap, scaleSnap);
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                transformHandle.positionSnap = new Vector3(0.0f, 0.0f, 0.0f);
                transformHandle.rotationSnap = 0.0f;
                transformHandle.scaleSnap = new Vector3(0.0f, 0.0f, 0.0f);
            }
            
            // Delete object key.
            if (Input.GetKeyDown(KeyCode.Delete))
                DeleteSelected();
        }
    }
}
