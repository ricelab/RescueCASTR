using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TeamIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public FieldTeam fieldTeam;

    private Color _lastTeamColor;
    private string _lastTeamName;

    // Update is called once per frame
    void Update()
    {
        if (fieldTeam)
        {
            // Update icon colour if team's colour changed
            if (fieldTeam.teamColor != _lastTeamColor)
            {
                _lastTeamColor = fieldTeam.teamColor;
                Image img = this.transform.Find("Image").GetComponent<Image>();
                img.color = fieldTeam.teamColor;
            }

            // Update name if team's name changed
            if (fieldTeam.teamName != _lastTeamName)
            {
                _lastTeamName = fieldTeam.teamName;
                Text txt = this.transform.Find("Text").GetComponent<Text>();
                txt.text = this.fieldTeam.teamName;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.GetComponent<Image>().color = new Color(0.87f, 0.87f, 0.87f, 1.0f);
        fieldTeam.ShowThisFieldTeamOnly();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        fieldTeam.ShowAllFieldTeams();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        fieldTeam.ShowThisFieldTeamOnly();
        fieldTeam.mainController.sideUi.ShowTeamDetails(fieldTeam);
    }
}
