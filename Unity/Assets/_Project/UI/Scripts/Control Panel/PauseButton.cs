using UnityEngine;
using UnityEngine.UI;
public class PauseButton : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TogglePause);
    }
    public void TogglePause()
    {
        Time.timeScale = Mathf.Approximately(Time.timeScale, 0.0f) ? 1.0f : 0.0f;
    }
}