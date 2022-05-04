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
    

        private float totalTime = 0f;
        private int frames = 0;
    
        public void Update()
        {
            frames++;
            totalTime += Time.unscaledDeltaTime;

            if (totalTime < hudRefreshRate) return;

            counter.text = (frames / totalTime).ToString("F1");
            frames = 0;
            totalTime = 0.0f;
        }

        private void Awake()
        {
            counter.text = (1.0f / Time.unscaledDeltaTime).ToString("F1");
        }
    }
}
