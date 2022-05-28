using _Project.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadgeNotification : MonoBehaviour
{
    [SerializeField]
    GameObject notification;

    [SerializeField]
    BadgeShowcase badgePrefab;

    private bool showingNotification = false;
    private List<Badge> badges;

    /// <summary>
    /// Show a badge notification to the user.
    /// </summary>
    /// <param name="badge"></param>
    private void ShowNotification(Badge badge)
    {
        showingNotification = true;
        badge.Completed = true;
        badgePrefab.UpdateUI(badge);

        notification.SetActive(true);
        StartCoroutine(HideNotification());
    }

    /// <summary>
    /// Hide the badge notification after 5 seconds.
    /// </summary>
    /// <returns></returns>
    IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(5);

        showingNotification = false;
        notification.SetActive(false);
    }

    /// <summary>
    /// Check whether new badges are earned every frame.
    /// </summary>
    void Update()
    {
        if (showingNotification)
            return;


        foreach (Badge b in badges)
            if (b.ShowNotification())
                ShowNotification(b);
    }

    /// <summary>
    /// Initialize variables.
    /// </summary>
    private void Start()
    {
        badges = GlobalManager.Get().Badges;
    }
}
