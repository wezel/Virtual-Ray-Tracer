using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BadgeShowcase : MonoBehaviour
{
    [SerializeField]
    private Sprite playTimeSprite, pointsSprite, objectsSprite; 

    [SerializeField]
    private Image badgeIcon, background;

    [SerializeField]
    private TextMeshProUGUI badgeName, badgeDescription;

    [SerializeField]
    private Color Uncompleted, Completed, backgroundCompleted, backgroundUncompleted;

    /// <summary>
    /// Initialize the badge's UI.
    /// </summary>
    /// <param name="badge"></param>
    public void UpdateUI(Badge badge)
    {
        // Update badge icon
        switch (badge.Type)
        {
            case Badge.BadgeType.Playtime:
                badgeIcon.sprite = playTimeSprite;
                break;
            case Badge.BadgeType.Points:
            case Badge.BadgeType.EasterEgg:
                badgeIcon.sprite = pointsSprite;
                break;
            case Badge.BadgeType.Objects:
                badgeIcon.sprite = objectsSprite;
                break;
            
        }

        // Update badge text
        badgeName.text = badge.BadgeName;
        badgeDescription.text = badge.BadgeDescription;

        // Make the badge look (un)completed
        badgeIcon.color = badge.Completed ? Completed : Uncompleted;
        badgeName.color = badge.Completed ? Completed : Uncompleted;
        badgeDescription.color = badge.Completed ? Completed : Uncompleted;
        background.color = badge.Completed ? backgroundCompleted : backgroundUncompleted;
    }
}
