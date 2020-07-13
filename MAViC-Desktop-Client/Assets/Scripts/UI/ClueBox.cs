using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClueBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Clue clue;

    public void Start()
    {
        this.transform.Find("Background").GetComponent<Image>().color = new Color(0.51f, 0.65f, 0.75f, 1.0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.transform.Find("Background").GetComponent<Image>().color = new Color(0.98f, 0.88f, 0.74f, 1.0f);

        clue.fieldTeam.HighlightPathAtTime(clue.simulatedTime.dateTime);

        clue.fieldTeam.HighlightTimeOnTimeline(clue.simulatedTime);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.transform.Find("Background").GetComponent<Image>().color = new Color(0.51f, 0.65f, 0.75f, 1.0f);
        clue.fieldTeam.UnhighlightPath();
        clue.fieldTeam.UnhighlightTimeline();
    }
}
