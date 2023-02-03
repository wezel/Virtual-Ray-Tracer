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
        Objects,
        EasterEgg
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
    public int ActionAmount
    {
        get 
        {        
            switch (type)
            {
                case BadgeType.Points:
                    return GlobalManager.TutorialPoints;
                case BadgeType.Playtime:
                    return (int)Time.realtimeSinceStartup / 60;
                case BadgeType.Objects:
                    return GlobalManager.ObjectsCreated;
                case BadgeType.EasterEgg:
                    return GlobalManager.EasterEggFound;
            }
            return 0;
        }
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
    /// Whether this badge has been completed.
    /// </summary>
    public bool Completed
    {
        get { return completed || ActionAmount >= ActionTotal; }
        set { completed = value; }
    }

    /// <summary>
    /// Whether to show a notification for earning the badge.
    /// </summary>
    /// <returns>Whether to show a notification for earning the badge</returns>
    public bool ShowNotification()
    {
        return !completed && ActionAmount >= ActionTotal;
    }
}
