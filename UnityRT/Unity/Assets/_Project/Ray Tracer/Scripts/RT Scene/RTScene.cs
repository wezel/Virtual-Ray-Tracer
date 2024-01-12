using System.Collections.Generic;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Camera;
using _Project.Ray_Tracer.Scripts.RT_Scene.RT_Light;
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.RT_Scene
{
    /// <summary>
    /// A simple class that stores all components of a ray tracer scene. A ray tracer scene consists of an
    /// <see cref="RTCamera"/>, a list of <see cref="RTLight"/>s and a list of <see cref="RTMesh"/>es. All of these
    /// objects refer to "real" objects in the Unity scene. This class simply collects those references for easy
    /// access.
    /// </summary>
    public class RTScene
    {
        public delegate void SceneChanged();
        /// <summary>
        /// An event invoked whenever a object in this scene is changed.
        /// </summary>
        public event SceneChanged OnSceneChanged;

        private RTCamera camera;
        /// <summary>
        /// This ray tracer scene's camera. There can only be one camera in a scene.
        /// </summary>
        public RTCamera Camera
        {
            get { return camera; }
            set
            {
                if (camera != null)
                    camera.OnCameraChanged -= SceneObjectChanged;

                camera = value;
                camera.OnCameraChanged += SceneObjectChanged;
                OnSceneChanged?.Invoke();
            }
        }

        /// <summary>
        /// This ray tracers scene's list of lights.
        /// </summary>
        public List<RTLight> Lights { get; }

        /// <summary>
        /// This ray tracers scene's list of meshes.
        /// </summary>
        public List<RTMesh> Meshes { get; }

        public RTScene(RTCamera camera) : this(camera, new List<RTLight>(), new List<RTMesh>()) { }

        public RTScene(RTCamera camera, List<RTLight> lights, List<RTMesh> meshes)
        {
            Camera = camera;
            
            Lights = lights;
            foreach (var light in lights)
                light.OnLightChanged += SceneObjectChanged;
            
            Meshes = meshes;
            foreach (var mesh in meshes)
                mesh.OnMeshChanged += SceneObjectChanged;
        }

        /// <summary>
        /// Add a light to this scene.
        /// </summary>
        /// <param name="light"> The <see cref="RTLight"/> object to add. </param>
        public void AddLight(RTLight light)
        {
            Lights.Add(light);
            light.OnLightChanged += SceneObjectChanged;
            OnSceneChanged?.Invoke();
        }

        /// <summary>
        /// Remove a light from this scene.
        /// </summary>
        /// <param name="light"> The <see cref="RTLight"/> object to remove. </param>
        public void RemoveLight(RTLight light)
        {
            Lights.Remove(light);
            light.OnLightChanged -= SceneObjectChanged;
            OnSceneChanged?.Invoke();
        }

        /// <summary>
        /// Add a mesh to this scene.
        /// </summary>
        /// <param name="mesh"> The <see cref="RTMesh"/> object to add. </param>
        public void AddMesh(RTMesh mesh)
        {
            Meshes.Add(mesh);
            mesh.OnMeshChanged += SceneObjectChanged;
            OnSceneChanged?.Invoke();
        }

        /// <summary>
        /// Remove a mesh from this scene.
        /// </summary>
        /// <param name="mesh"> The <see cref="RTMesh"/> object to remove. </param>
        public void RemoveMesh(RTMesh mesh)
        {
            Meshes.Remove(mesh);
            mesh.OnMeshChanged -= SceneObjectChanged;
            OnSceneChanged?.Invoke();
        }
        
        /// <summary>
        /// Destroy all game objects associated with this scene.
        /// </summary>
        public void Destroy()
        {
            if (camera != null)
            {
                camera.OnCameraChanged -= SceneObjectChanged;
                Object.Destroy(camera.gameObject);
            }
            camera = null;

            foreach (var light in Lights)
            {
                if (light != null)
                {
                    light.OnLightChanged -= SceneObjectChanged;
                    Object.Destroy(light.gameObject);
                }
            }
            Lights.Clear();

            foreach (var mesh in Meshes)
            {
                if (mesh != null)
                {
                    mesh.OnMeshChanged -= SceneObjectChanged;
                    Object.Destroy(mesh.gameObject);
                }
            }
            Meshes.Clear();
        }

        private void SceneObjectChanged()
        {
            OnSceneChanged?.Invoke();
        }
    }
}
