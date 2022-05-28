using _Project.UI.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class BadgesPanel : MonoBehaviour
{
    //[SerializeField]
    //private BadgePrefab badgePrefab;

    [SerializeField]
    private Button exitButton;

    /// <summary>
    /// Show the badges panel.
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        UIManager.Get().AddEscapable(Hide);
    }

    /// <summary>
    /// Hide the badges panel.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        UIManager.Get().RemoveEscapable(Hide);
    }

    /// <summary>
    /// Toggle the badges panel. If the badges panel is hidden it will now be shown and vice versa.
    /// </summary>
    public void Toggle()
    {
        if (gameObject.activeSelf)
            Hide();
        else
            Show();
    }

    private void Update()
    {
        Debug.Log("UPDATE");
    }

    private void Awake()
    {
        exitButton.onClick.AddListener(Hide);

        /*
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 1; i < sceneCount; i++)
        {
            Button levelButton = Instantiate(levelsPrefab, content.transform);
            levelButton.name = i.ToString();

            levelButton.interactable = Tutorial.TutorialManager.CanLevelBeLoaded(i);
            levelButton.GetComponentInChildren<TextMeshProUGUI>().text = i + ". " + System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            levelButton.onClick.AddListener(() => OnButtonClicked(levelButton));
            levelButtons.Add(levelButton);
        }
        */
    }
}
