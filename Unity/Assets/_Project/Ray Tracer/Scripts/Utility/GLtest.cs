
using UnityEngine;

namespace _Project.Ray_Tracer.Scripts.Utility
{
    public class GLtest : MonoBehaviour
    {
        // Draws a line from "startVertex" var to the curent mouse position.
        public Material mat;
        Vector3 startVertex;
        Vector3 mousePos;

        void Start()
        {
            startVertex = Vector3.zero;
        }

        void Update()
        {
            mousePos = Input.mousePosition;
            // Press space to update startVertex
            if (Input.GetKeyDown(KeyCode.Space))
            {
                startVertex = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);
            }
        }

        void OnPostRender()
        {
            if (!mat)
            {
                Debug.LogError("Please Assign a material on the inspector");
                return;
            }
            
            
            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadOrtho();

            GL.Begin(GL.LINES);
            GL.Color(Color.red);
            GL.Vertex(startVertex);
            GL.Vertex(new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0));
            GL.End();

            GL.PopMatrix();
        }
    }
}