using _Project.Scripts;
using System;
using UnityEngine;

[Serializable]
public class Badge
{
    public enum BadgeType
    {
        Points,
        Playtime,
        Objects
    }

    /// <summary>
    /// The type of this badge.
    /// </summary>
    [SerializeField]
    private BadgeType type;
    public BadgeType Type
    {
        get { return type; }
    }

    /// <summary>
    /// Name of this badge.
    /// </summary>
    [SerializeField]
    string badgeName = "Name";
    public string BadgeName
    {
        get { return badgeName; }
    }

    /// <summary>
    /// Description of this badge.
    /// </summary>
    [SerializeField]
    string badgeDescription = "Description";
    public string BadgeDescription
    {
        get { return badgeDescription; }
    }

    /// <summary>
    /// The amount of actions that have been completed.
    /// </summary>
    int actionAmount = 0;
    public int ActionAmount
    {
        get { return actionAmount; }
    }

    /// <summary>
    /// The amount of actions that have to be completed to earn this badge.
    /// </summary>
    [SerializeField]
    int actionTotal = 0;
    public int ActionTotal
    {
        get { return actionTotal; }
    }

    private bool completed = false;

    /// <summary>
    /// Set this badge to be completed.
    /// </summary>
    public void SetCompleted()
    {
        completed = true;
    }

    /// <summary>
    /// Whether to show a notification for earning the badge.
    /// </summary>
    /// <returns>Whether to show a notification for earning the badge</returns>
    public bool ShowNotification()
    {
        return !completed && actionAmount >= actionTotal;
    }

    /// <summary>
    /// Updates the amount of actions completed
    /// </summary>
    /// <param name="settings"></param>
    /// <returns>Whether the badge was updated</returns>
    public void UpdateBadge()
    {
        if (completed) return;

        switch (type)
        {
            case BadgeType.Points:
                actionAmount = GlobalSettings.TutorialPoints;
                break;
            case BadgeType.Playtime:
                actionAmount = GlobalSettings.MiliSecondsPlayed / 1000 / 60;
                break;
            case BadgeType.Objects:
                actionAmount = GlobalSettings.ObjectsCreated;
                break;
        }
    }
}
