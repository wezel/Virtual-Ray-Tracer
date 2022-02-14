using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Control_Panel
{
    public class FolderButton : Button
    {
        private Image background;
        private Color originalColor;
        private Color concealedColor;

        public void Highlight()
        {
            transform.SetAsLastSibling();
            background.color = originalColor;
        }

        public void Conceal()
        {
            transform.SetAsFirstSibling();
            background.color = concealedColor;
        }
    
        // Start is called before the first frame update
        private new void Awake()
        { 
            base.Start(); 
            background = GetComponent<Image>(); 
            originalColor = background.color;
            concealedColor = Color.Lerp(originalColor, Color.white, .1f); // To lighten by 10%
        }
    }
}
