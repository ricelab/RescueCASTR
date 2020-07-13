using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SideUi : ABackButtonClickHandler // ABackButtonClickHandler inherits MonoBehaviour, and is defined in BackButton.cs
{
    public MainController mainController;

    public ScrollRect scrollRect;

    public GameObject mainMenuContentPanel;
    public GameObject fieldTeamDetailsContentPanel;

    public GameObject teamColorIconObj;
    public GameObject teamNameTextObj;

    public GameObject liveFootageObj;

    public FieldTeam selectedFieldTeam = null;

    public void ShowTeamDetails(FieldTeam ft)
    {
        selectedFieldTeam = ft;

        teamColorIconObj.GetComponent<Image>().color = selectedFieldTeam.teamColor;
        teamNameTextObj.GetComponent<Text>().text = selectedFieldTeam.teamName;

        mainMenuContentPanel.SetActive(false);
        fieldTeamDetailsContentPanel.SetActive(true);

        scrollRect.content = fieldTeamDetailsContentPanel.GetComponent<RectTransform>();

        DisplayFieldTeamLiveImage(selectedFieldTeam.GetPhotoThumbnailPathFromTime(mainController.currentTime));
    }

    public void DisplayFieldTeamLiveImage(string path)
    {
        Image liveFootageImage = liveFootageObj.GetComponent<Image>();

        Texture2D texture = Utility.LoadImageFile(path);
        liveFootageImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }

    public override void OnBackButtonClick(GameObject fromPage, GameObject toPage)
    {
        selectedFieldTeam.ShowAllFieldTeams();
        selectedFieldTeam = null;

        scrollRect.content = mainMenuContentPanel.GetComponent<RectTransform>();
    }
}
