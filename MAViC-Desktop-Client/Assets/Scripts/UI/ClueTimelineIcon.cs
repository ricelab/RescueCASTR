using UnityEngine;
using UnityEngine.EventSystems;

public class ClueTimelineIcon : ClueIcon, IPointerEnterHandler, IPointerExitHandler
{
    public void Update()
    {
        if (clue != null)
        {
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = clue.fieldTeam.teamTimeline.TimelinePositionToUIPosition(clue.simulatedTime.dateTime);
        }

        if (_clue.highlightTimelineIcon)
        {
            this.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            this.transform.SetAsLastSibling();
        }
        else
        {
            this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        _clue.highlightMapIcon = true;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        _clue.highlightMapIcon = false;
    }
}
