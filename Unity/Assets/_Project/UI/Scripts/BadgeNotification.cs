using _Project.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BadgeNotification : MonoBehaviour
{
    [Serializable]
    public class Event : UnityEvent { }
    public Event OnNotification;

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
                OnNotification?.Invoke();
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
