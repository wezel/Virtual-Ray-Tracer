using UnityEngine;
using UnityEngine.UI;

namespace _Project.UI.Scripts.Toolbar
{
    public class MenuBar : MonoBehaviour
    {

        [SerializeField]
        private Button mainMenuButton;

        [SerializeField]
        private Button helpMenuButton;
        
        void Start()
        {
            mainMenuButton.onClick.AddListener(Overlay.Get().MainMenu.Toggle);
            helpMenuButton.onClick.AddListener(UIBase.Get().HelpPanel.Toggle);
        }
    }
}
