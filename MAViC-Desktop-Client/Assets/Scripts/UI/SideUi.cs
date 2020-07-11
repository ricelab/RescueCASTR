using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SideUi : MonoBehaviour
{
    public MainController mainController;

    public GameObject mainMenuContentPanel;
    public GameObject fieldTeamDetailsContentPanel;

    public GameObject teamColorIconObj;
    public GameObject teamNameTextObj;

    public GameObject liveFootageObj;

    public FieldTeam selectedFieldTeam;

    public void ShowTeamDetails(FieldTeam ft)
    {
        selectedFieldTeam = ft;

        teamColorIconObj.GetComponent<Image>().color = selectedFieldTeam.teamColor;
        teamNameTextObj.GetComponent<Text>().text = selectedFieldTeam.teamName;

        mainMenuContentPanel.SetActive(false);
        fieldTeamDetailsContentPanel.SetActive(true);

        mainController.sideUi.DisplayFieldTeamLiveImage(selectedFieldTeam.GetPhotoThumbnailPathFromTime(mainController.currentTime));
    }

    public void DisplayFieldTeamLiveImage(string path)
    {
        Image liveFootageImage = liveFootageObj.GetComponent<Image>();

        Texture2D texture = Utility.LoadImageFile(path);
        liveFootageImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }
}
