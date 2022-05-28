using _Project.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadgeNotification : MonoBehaviour
{
    [SerializeField]
    BadgeShowcase badgePrefab;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private bool showingNotification = false;

    private List<Badge> badges;

    /// <summary>
    /// Check whether new badges are earned every frame.
    /// If so, show them.
    /// </summary>
    void Update()
    {
        if (showingNotification)
            return;

        foreach (Badge b in badges)
        {
            if (b.ShowNotification())
            {
                b.Completed = true;
                badgePrefab.UpdateUI(b);
                animator.SetTrigger("Show");
                return;
            }
        }
            
    }

    /// <summary>
    /// Initialize variables.
    /// </summary>
    private void Start()
    {
        badges = GlobalManager.Get().Badges;
    }
}
