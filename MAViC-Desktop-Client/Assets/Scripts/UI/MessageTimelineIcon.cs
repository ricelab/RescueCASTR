using UnityEngine;

public class MessageTimelineIcon : MonoBehaviour
{
    public Message message;

    public void Update()
    {
        if (message != null)
        {
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = message.fieldTeam.teamTimeline.TimelinePositionToUIPosition(message.simulatedTime.dateTime);
        }
    }
}
