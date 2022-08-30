using System.Collections;
using _Project.Ray_Tracer.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel
{
    public class VisualizationProperties : MonoBehaviour
    {

        private UIManager uiManager;

        public void Show()
        {
            gameObject.SetActive(true);
            uiManager = UIManager.Get();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
