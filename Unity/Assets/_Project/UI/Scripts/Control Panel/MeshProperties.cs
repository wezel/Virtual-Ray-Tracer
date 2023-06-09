using _Project.Ray_Tracer.Scripts.RT_Scene;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that provides access to the properties of an <see cref="RTMesh"/>. Any changes made to the shown
    /// properties will be applied to the mesh.
    /// </summary>
    public class MeshProperties : MonoBehaviour
    {
        private RTMesh mesh;

        [SerializeField]
        private Vector3Edit positionEdit;
        [SerializeField]
        private Vector3Edit rotationEdit;
        [SerializeField]
        private Vector3Edit scaleEdit;
        
        [SerializeField]
        private ColorEdit colorEdit;
        [SerializeField]
        private FloatEdit ambientEdit;
        [SerializeField]
        private FloatEdit diffuseEdit;
        [SerializeField]
        private FloatEdit specularEdit;
        [SerializeField]
        private FloatEdit shininessEdit;
        [SerializeField]
        private TMP_Dropdown typeDropdown; 
        [SerializeField]
        private FloatEdit refractiveIndexEdit;

        /// <summary>
        /// Show the mesh properties for <paramref name="mesh"/>. These properties can be changed via the shown UI.
        /// </summary>
        /// <param name="mesh"> The <see cref="RTMesh"/> whose properties will be shown. </param>
        public void Show(RTMesh mesh)
        {
            gameObject.SetActive(true);
            this.mesh = mesh;
            mesh.transform.hasChanged = false;

            positionEdit.Value = mesh.Position;
            rotationEdit.Value = mesh.Rotation;
            scaleEdit.Value = mesh.Scale;

            colorEdit.Color = mesh.Color;
            ambientEdit.Value = mesh.Ambient;
            diffuseEdit.Value = mesh.Metallic;
            specularEdit.Value = mesh.Specular;
            shininessEdit.Value = mesh.Smoothness;
            
            typeDropdown.value = typeDropdown.options.FindIndex(option => option.text == mesh.Type.ToString());
            refractiveIndexEdit.gameObject.SetActive(mesh.Type == RTMesh.ObjectType.Transparent);
            refractiveIndexEdit.Value = mesh.RefractiveIndex;
        }

        /// <summary>
        /// Hide the shown mesh properties.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            mesh = null;
        }

        private void ChangeObjectType(RTMesh.ObjectType type)
        {
            refractiveIndexEdit.gameObject.SetActive(type == RTMesh.ObjectType.Transparent);
            mesh.ChangeObjectType(type);
            Show(mesh);
        }
        
        private void Awake()
        {
            positionEdit.OnValueChanged += (value) => { mesh.Position = value; };
            rotationEdit.OnValueChanged += (value) => { mesh.Rotation = value; };
            scaleEdit.OnValueChanged += (value) => { mesh.Scale = value; };

            colorEdit.OnValueChanged += (value) => { mesh.Color = value; };
            ambientEdit.OnValueChanged += (value) => { mesh.Ambient = value; };
            diffuseEdit.OnValueChanged += (value) => { mesh.Metallic = value; };
            specularEdit.OnValueChanged += (value) => { mesh.Specular = value; };
            shininessEdit.OnValueChanged += (value) => { mesh.Smoothness = value; };

            typeDropdown.onValueChanged.AddListener( type => ChangeObjectType( (RTMesh.ObjectType) type));

            refractiveIndexEdit.OnValueChanged += (value) => { mesh.RefractiveIndex = value; };
        }

        private void Update()
        {
            // Update the UI based on external changes to the mesh transform (e.g. through the transformation gizmos).
            bool inUI = EventSystem.current.currentSelectedGameObject != null; // Only update if we are not in the UI.
            bool draggingEdit = positionEdit.IsDragging() || rotationEdit.IsDragging() || scaleEdit.IsDragging();
            if (gameObject.activeSelf && mesh.transform.hasChanged && !inUI && !draggingEdit)
            {
                positionEdit.Value = mesh.transform.position;
                rotationEdit.Value = mesh.transform.eulerAngles;
                scaleEdit.Value = mesh.transform.localScale;
                mesh.transform.hasChanged = false;
            }
        }
    }
}
