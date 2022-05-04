using TMPro;
using UnityEngine;

namespace _Project.UI.Scripts.Tooltips
{
    public class Version : MonoBehaviour
    {
        void Start()
        {
            GetComponent<TextMeshProUGUI>().text = "V" + Application.version;
        }
    }
}