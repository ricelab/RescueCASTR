using UnityEngine;

public class ClueTimelineIcon : MonoBehaviour
{
    public Clue clue;

    public void Update()
    {
        if (clue != null)
        {
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = clue.fieldTeam.teamTimeline.TimelinePositionToUIPosition(clue.simulatedTime.dateTime);
        }
    }
}
