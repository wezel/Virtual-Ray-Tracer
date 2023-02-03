using _Project.Scripts;
using _Project.UI.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BadgesPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject content;

    [SerializeField]
    private BadgeShowcase badgePrefab;

    [SerializeField]
    private Button exitButton;

    private List<BadgeShowcase> badgePrefabs = new List<BadgeShowcase>();

    /// <summary>
    /// Show the badges panel.
    /// </summary>
    public void Show()
    {
        // Update the badges
        List<Badge> badges = GlobalManager.Get().Badges;
        for (int i = 0; i < badgePrefabs.Count; i++)
            badgePrefabs[i].UpdateUI(badges[i]);

        if (GlobalManager.EasterEggFound != 0) badgePrefabs[badgePrefabs.Count - 1].gameObject.SetActive(true);
        
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

    private void Awake()
    {
        exitButton.onClick.AddListener(Hide);

        foreach (Badge badge in GlobalManager.Get().Badges)
        {
            BadgeShowcase prefab = Instantiate(badgePrefab, content.transform);
            prefab.UpdateUI(badge);
            badgePrefabs.Add(prefab);
        }

        // hide easter egg TODO implement this nicer
        badgePrefabs[badgePrefabs.Count - 1].gameObject.SetActive(false);
    }
}
