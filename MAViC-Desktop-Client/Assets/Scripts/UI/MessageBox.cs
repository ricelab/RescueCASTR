using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Message message;
    public Text messageText;
    public Text messageTimeText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.transform.Find("Background").GetComponent<Image>().color = new Color(0.98f, 0.88f, 0.74f, 1.0f);
        message.fieldTeam.HighlightPathAtSimulatedTime(message.simulatedTime.dateTime);
        message.fieldTeam.HighlightSimulatedTimeOnTimeline(message.simulatedTime);
        message.highlightMapIcon = true;
        message.highlightTimelineIcon = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Color bgColor;

        if (message.messageDirection == MessageDirection.FromCommandToTeam)
            bgColor = new Color(0.13f, 1.0f, 0.28f, 1.0f);
        else // if (message.messageDirection == MessageDirection.FromTeamToCommand)
            bgColor = new Color(0.902f, 0.902f, 0.902f, 1.0f);

        this.transform.Find("Background").GetComponent<Image>().color = bgColor;
        message.fieldTeam.UnhighlightPath();
        message.fieldTeam.UnhighlightTimeline();
        message.highlightMapIcon = false;
        message.highlightTimelineIcon = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        message.fieldTeam.mainController.fullscreenViewObj =
                GameObject.Instantiate(message.fieldTeam.mainController.messageFullscreenViewPrefab, message.fieldTeam.mainController.wholeScreenUiObj.transform);
        message.fieldTeam.mainController.messageFullscreenView = message.fieldTeam.mainController.fullscreenViewObj.GetComponent<MessageFullscreenView>();
        message.fieldTeam.mainController.messageFullscreenView.message = message;
    }
}
