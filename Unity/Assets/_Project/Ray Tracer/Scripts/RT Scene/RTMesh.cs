using UnityEngine;
using UnityEngine.Rendering;

namespace _Project.Ray_Tracer.Scripts.RT_Scene
{
    /// <summary>
    /// Represents a mesh in the ray tracer scene. Requires that the attached game object has a mesh and a material
    /// based on the RayTracerShader. Should be considered something like a tag to indicate to the scene manager that
    /// this mesh should be sent to the ray tracer. Almost all actual information for the ray tracer is stored in the
    /// transform, mesh and material components.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)), RequireComponent(typeof(Outline))]
    public class RTMesh : MonoBehaviour
    {
        private static Shader StandardShader = null;
        private static Shader TransparentShader = null;

        private static readonly int ambient = Shader.PropertyToID("_Ambient");
        private static readonly int diffuse = Shader.PropertyToID("_Diffuse");
        private static readonly int specular = Shader.PropertyToID("_Specular");
        private static readonly int shininess = Shader.PropertyToID("_Shininess");
        private static readonly int refractiveIndex = Shader.PropertyToID("_RefractiveIndex");
        private static readonly int metallic = Shader.PropertyToID("_Metallic");
        private static readonly int smoothness = Shader.PropertyToID("_Smoothness");
        private static readonly int backupAmbient = Shader.PropertyToID("_BackupAmbient");



        public delegate void MeshChanged();
        /// <summary>
        /// An event invoked whenever a property of this mesh is changed.
        /// </summary>
        public event MeshChanged OnMeshChanged;

        /// <summary>
        /// The underlying <see cref="UnityEngine.Material"/> used by the mesh. Its shader should be either
        /// LitSS or RayTracerShaderTransparent.
        /// </summary>
        public Material Material { get; private set; }

        /// <summary>
        /// The outline used to highlight the mesh when it is selected.
        /// </summary>
        public Outline Outline { get; private set; }

        /// <summary>
        /// The position of the mesh.
        /// </summary>
        public Vector3 Position
        {
            get => transform.position;
            set
            {
                transform.position = value;
                OnMeshChanged?.Invoke();
            }
        }

        /// <summary>
        /// The rotation of the mesh.
        /// </summary>
        public Vector3 Rotation
        {
            get => transform.eulerAngles;
            set
            {
                transform.eulerAngles = value;
                OnMeshChanged?.Invoke();
            }
        }

        /// <summary>
        /// The scale of the mesh.
        /// </summary>
        public Vector3 Scale
        {
            get => transform.localScale;
            set
            {
                transform.localScale = value;
                OnMeshChanged?.Invoke();
            }
        }

        /// <summary>
        /// The color of the mesh.
        /// </summary>
        public Color Color
        {
            get => Material.color;
            set
            {
                // Calculate ambient color
                if(this.Type == ObjectType.Transparent || this.Type == ObjectType.Mirror) {
                    return;
                }
                var ambientValue = Material.GetFloat(ambient);
                var oldAmbientValue = Material.GetFloat(backupAmbient);
                var ambientColor = value;
                ambientColor.r = ambientValue*value.r - oldAmbientValue*value.r;
                ambientColor.g = ambientValue*value.g - oldAmbientValue*value.g;
                ambientColor.b = ambientValue*value.b - oldAmbientValue*value.b;

                Material.SetFloat(backupAmbient, ambientValue);

                // Change Base Map color
                Material.color = value + ambientColor;
                
                // Create darker variant of base map for emissive color
                var emission = value;
                emission.r = value.r * (ambientValue * (float) 1 + (float) 0.01);
                emission.g = value.g * (ambientValue * (float) 1 + (float) 0.01);
                emission.b = value.b * (ambientValue * (float) 1 + (float) 0.01);

                // Get the Renderer component
                var meshRenderer = this.GetComponent<Renderer>();

                // Call SetColor with color name and set color to corresponding value
                meshRenderer.material.SetColor("_EmissiveColor", emission);

                OnMeshChanged?.Invoke();
            }
        }

        /// <summary>
        /// The ambient component of the mesh's material.
        /// </summary>
        public float Ambient
        {
            get => Material.GetFloat(ambient);
            set
            {
                Material.SetFloat(ambient, value);

                // Get the Renderer component
                var meshRenderer = this.GetComponent<Renderer>();

                // Call SetColor using the shader property name "_BaseEmissiveColor" and adjust color with ambient
                // meshRenderer.material.SetColor("_BaseColor", emission);
                this.Color = this.Color;
                
                OnMeshChanged?.Invoke();
            }
        }

        /// <summary>
        /// The diffuse component of the mesh's material.
        /// </summary>
        public float Diffuse
        {
            get => Material.GetFloat(diffuse);
            set
            {
                Material.SetFloat(diffuse, value);
                Material.SetFloat(metallic, 1-value);
                OnMeshChanged?.Invoke();
            }
        }

        /// <summary>
        /// The specular component of the mesh's material.
        /// </summary>
        public float Specular
        {
            get => Material.GetFloat(specular);
            set
            {
                Material.SetFloat(specular, value);
                Material.SetFloat(smoothness, value);
                OnMeshChanged?.Invoke();
            }
        }

        /// <summary>
        /// The shininess of the mesh's material.
        /// </summary>
        public float Shininess
        {
            get => Material.GetFloat(shininess);
            set
            {
                Material.SetFloat(shininess, value);
                OnMeshChanged?.Invoke();
            }
        }

        /// <summary>
        /// The refractive index of the mesh's material.
        /// </summary>
        public float RefractiveIndex
        {
            get => Material.GetFloat(refractiveIndex);
            set
            {
                Material.SetFloat(refractiveIndex, value);
                OnMeshChanged?.Invoke();
            }
        }

        public enum ObjectType
        {
            Opaque,
            Transparent,
            Mirror
        }

        /// <summary>
        /// Whether the mesh is transparent a mirror or just opaque.
        /// </summary>
        [SerializeField] 
        public ObjectType Type;

        [SerializeField]
        private bool shadeSmooth = true;
        /// <summary>
        /// Whether the ray tracer smooths the normals of the mesh. Does not affect the visuals of the mesh in the
        /// Unity scene.
        /// </summary>
        public bool ShadeSmooth
        {
            get => shadeSmooth;
            private set 
            { 
                shadeSmooth = value;
                OnMeshChanged?.Invoke();
            }
        }

        public void ChangeObjectType(ObjectType type)
        {
            if (Type == type) return;

            switch (type)
            {
                case ObjectType.Transparent:
                    Material.shader = TransparentShader;
                    Material.SetFloat(ambient, 0);
                    Material.SetFloat(diffuse, 0);
                    Material.SetFloat(specular, 0);
                    Material.SetFloat(shininess, 128);
                    Material.SetFloat(refractiveIndex, 1.5f);
                    Color color = Material.color;
                    color.a = 120 / 256f;
                    Material.color = color;
                    break;
                case ObjectType.Opaque:
                    Material.shader = StandardShader;
                    Material.SetFloat(ambient, 0.2f);
                    Material.SetFloat(diffuse, 1f);
                    Material.SetFloat(specular, 0);
                    Material.SetFloat(shininess, 1);
                    break;
                case ObjectType.Mirror:
                    Material.shader = StandardShader;
                    Material.SetFloat(ambient, 0);
                    Material.SetFloat(diffuse, 0);
                    Material.SetFloat(specular, 1);
                    Material.SetFloat(shininess, 128);
                    break;
            }

            Type = type;
            OnMeshChanged?.Invoke();
        }

        private void Awake()
        {
            if (StandardShader == null)
                StandardShader = Shader.Find("Custom/LitSS");
            if (TransparentShader == null)
                TransparentShader = Shader.Find("Custom/RayTracerShaderTransparent");

            Initialize();
            this.Ambient = this.Ambient;
            this.Color = this.Color;
        }

        /// <summary>
        /// Initialize this ray tracer mesh based on its attached components. What material settings are used is
        /// determined by inspecting the attached <see cref="MeshRenderer"/>. If this component is missing or if
        /// something else goes wrong an error is printed.
        /// </summary>
        private void Initialize()
        {
            GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.TwoSided;
            // Find the material used by this object and verify that it uses the correct shader.
            Material = GetComponent<MeshRenderer>().material;
            
            Type = Material.name.Replace("(Instance)","").Trim()  switch
            {
                "Glass" => ObjectType.Transparent,
                "Mirror" => ObjectType.Mirror,
                _ => ObjectType.Opaque
            };
            
            if (Material == null)
                Debug.LogError("Could not find material of " + gameObject.name + "!");
            if (Type == ObjectType.Transparent && Material.shader != TransparentShader)
                Debug.LogError("Material of " + gameObject.name + " uses a non transparent shader or a shader not" +
                    " supported by the ray tracer!");
            if (Type != ObjectType.Transparent && Material.shader != StandardShader)
                Debug.LogError("Material of " + gameObject.name + " uses a transparent shader or a shader not" +
                    " supported by the ray tracer!");

            // Find the outline component attached to this object.
            Outline = GetComponent<Outline>();
            if (Outline == null)
                Debug.LogWarning("Could not find outline of " + gameObject.name + "!");
            else
                Outline.enabled = false; // Outline should be disabled by default.
        }
    }
}
