using TMPro;
using UnityEngine;

namespace _Project.UI.Scripts
{
    public class FPSCounter : MonoBehaviour
    {
    
        [SerializeField] 
        private TextMeshProUGUI counter;
    
        [SerializeField]
        private float hudRefreshRate = 1f;
    
        private float timer;
    
        public void Update()
        {
            if (!(Time.unscaledTime > timer)) return;
        
            counter.text =  (1f / Time.unscaledDeltaTime).ToString("F1");
            timer = Time.unscaledTime + hudRefreshRate;
        }
    }
}
