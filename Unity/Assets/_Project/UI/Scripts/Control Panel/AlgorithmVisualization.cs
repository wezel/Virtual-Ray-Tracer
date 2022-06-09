using UnityEngine;
using _Project.Ray_Tracer.Scripts;
using _Project.Ray_Tracer.Scripts.RT_Ray;
using UnityEngine.EventSystems;

namespace _Project.UI.Scripts.Control_Panel
{
    /// <summary>
    /// A UI class that provides access to the properties of an <see cref="RTLight"/>. Any changes made to the shown
    /// properties will be applied to the light.
    /// </summary>
    public class AlgorithmVisualization : MonoBehaviour
    {
        private RayObject ray;

        [SerializeField]
        private BoolEdit one_single_button;

        public void Show(RayObject Ray)
        {
            Debug.Log("Showing panel (???)");
            gameObject.SetActive(true);
            this.ray = Ray;
        }

        public void Hide()
        {
            Debug.Log("Hiding panel (???)");
            gameObject.SetActive(false);
        }

        public void Awake()
        {
            one_single_button.OnValueChanged += (value) => { Debug.Log("hello :)"); };
        }
    }
}
