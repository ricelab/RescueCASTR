using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TeamIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public FieldTeam fieldTeam;
    public Image footage;

    private Color _lastTeamColor;
    private string _lastTeamName;

    public void Update()
    {
        if (fieldTeam)
        {
            // Update icon colour if team's colour changed
            if (fieldTeam.teamColor != _lastTeamColor)
            {
                _lastTeamColor = fieldTeam.teamColor;
                if (fieldTeam.isComplete)
                {
                    Image img = this.transform.Find("Image").GetComponent<Image>();
                    img.color = fieldTeam.teamColor;
                }
                else
                {
                    this.GetComponent<Image>().color =
                        new Color(0.75f * fieldTeam.teamColor.r, 0.75f * fieldTeam.teamColor.g, 0.75f * fieldTeam.teamColor.b, fieldTeam.teamColor.a);
                }
            }

            // Update name if team's name changed
            if (fieldTeam.teamName != _lastTeamName)
            {
                _lastTeamName = fieldTeam.teamName;
                Text txt = this.transform.Find("Text").GetComponent<Text>();
                txt.text = this.fieldTeam.teamName;
            }

            // Update footage thumbnail (if applicable)
            if (!fieldTeam.isComplete)
            {
                DisplayImage(fieldTeam.GetPhotoThumbnailPathFromSimulatedTime(fieldTeam.mainController.currentSimulatedTime));
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (fieldTeam.isComplete)
        {
            this.GetComponent<Image>().color = new Color(0.87f, 0.87f, 0.87f, 1.0f);
        }
        else
        {
            this.GetComponent<Image>().color =
                        new Color(0.25f * fieldTeam.teamColor.r, 0.25f * fieldTeam.teamColor.g, 0.25f * fieldTeam.teamColor.b, fieldTeam.teamColor.a);
        }

        fieldTeam.ShowThisFieldTeamOnly(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (fieldTeam.isComplete)
        {
            this.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
        else
        {
            this.GetComponent<Image>().color =
                        new Color(0.75f * fieldTeam.teamColor.r, 0.75f * fieldTeam.teamColor.g, 0.75f * fieldTeam.teamColor.b, fieldTeam.teamColor.a);
        }

        fieldTeam.ShowAllFieldTeams(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (fieldTeam.isComplete)
        {
            this.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
        else
        {
            this.GetComponent<Image>().color =
                        new Color(0.75f * fieldTeam.teamColor.r, 0.75f * fieldTeam.teamColor.g, 0.75f * fieldTeam.teamColor.b, fieldTeam.teamColor.a);
        }

        fieldTeam.ShowThisFieldTeamOnly(true);
        fieldTeam.mainController.sideUi.ShowTeamDetails(fieldTeam);
    }

    public void DisplayImage(string path)
    {
        Texture2D texture = Utility.LoadImageFile(path);
        footage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }
}
