using UnityEngine;
using UnityEngine.EventSystems;

public class MessageTimelineIcon : MessageIcon, IPointerEnterHandler, IPointerExitHandler
{
    public void Update()
    {
        if (_message != null)
        {
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = _message.fieldTeam.teamTimeline.TimelinePositionToUIPosition(_message.simulatedTime.dateTime);

            if (_message.highlightTimelineIcon)
            {
                this.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                this.transform.SetAsLastSibling();
            }
            else
            {
                this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        _message.highlightMapIcon = true;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        _message.highlightMapIcon = false;
    }
}
