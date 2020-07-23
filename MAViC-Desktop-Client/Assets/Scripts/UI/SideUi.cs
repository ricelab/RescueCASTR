using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SideUi : ABackButtonClickHandler // ABackButtonClickHandler inherits MonoBehaviour, and is defined in BackButton.cs
{
    public enum CurrentlyActivePage
    {
        MainMenu,
        FieldTeamDetails,
        Messages,
        Clues
    };


    public MainController mainController;

    public ScrollRect scrollRect;

    public GameObject mainMenuContentPanel;
    public GameObject fieldTeamDetailsContentPanel;
    public GameObject messagesContentPanel;
    public GameObject cluesContentPanel;

    public GameObject ftdPageTeamColorIconObj;
    public GameObject ftdPageTeamNameTextObj;

    public GameObject messagesPageTeamColorIconObj;
    public GameObject messagesPageTeamNameTextObj;

    public GameObject cluesPageTeamColorIconObj;
    public GameObject cluesPageTeamNameTextObj;

    public GameObject liveFootageObj;

    public MessagesPage messagesPage;
    public CluesPage cluesPage;

    public FieldTeam selectedFieldTeam = null;

    public CurrentlyActivePage currentlyActivePage = CurrentlyActivePage.MainMenu;


    public void ShowTeamDetails(FieldTeam ft)
    {
        selectedFieldTeam = ft;

        selectedFieldTeam.ShowThisFieldTeamOnly();

        ftdPageTeamColorIconObj.GetComponent<Image>().color = selectedFieldTeam.teamColor;
        ftdPageTeamNameTextObj.GetComponent<Text>().text = selectedFieldTeam.teamName;

        mainMenuContentPanel.SetActive(false);
        fieldTeamDetailsContentPanel.SetActive(true);

        scrollRect.content = fieldTeamDetailsContentPanel.GetComponent<RectTransform>();

        DisplayFieldTeamLiveImage(selectedFieldTeam.GetPhotoThumbnailPathFromSimulatedTime(mainController.currentSimulatedTime));

        currentlyActivePage = CurrentlyActivePage.FieldTeamDetails;
    }

    public void ShowMessages()
    {
        messagesPageTeamColorIconObj.GetComponent<Image>().color = selectedFieldTeam.teamColor;
        messagesPageTeamNameTextObj.GetComponent<Text>().text = selectedFieldTeam.teamName + " Messages";

        fieldTeamDetailsContentPanel.SetActive(false);
        messagesContentPanel.SetActive(true);

        scrollRect.content = messagesContentPanel.GetComponent<RectTransform>();

        // Load field team's message history
        foreach (Message message in selectedFieldTeam.revealedMessages)
        {
            messagesPage.AddMessageBox(message);
        }

        currentlyActivePage = CurrentlyActivePage.Messages;
    }

    public void ShowClues()
    {
        cluesPageTeamColorIconObj.GetComponent<Image>().color = selectedFieldTeam.teamColor;
        cluesPageTeamNameTextObj.GetComponent<Text>().text = selectedFieldTeam.teamName + " Clues";

        fieldTeamDetailsContentPanel.SetActive(false);
        cluesContentPanel.SetActive(true);

        scrollRect.content = cluesContentPanel.GetComponent<RectTransform>();

        // Load field team's clues
        foreach (Clue clue in selectedFieldTeam.revealedClues)
        {
            cluesPage.AddClueBox(clue);
        }

        currentlyActivePage = CurrentlyActivePage.Clues;
    }

    public override void OnBackButtonClick(GameObject fromPage, GameObject toPage)
    {
        mainController.ShowAllFieldTeams();
        selectedFieldTeam = null;

        scrollRect.content = mainMenuContentPanel.GetComponent<RectTransform>();
        currentlyActivePage = CurrentlyActivePage.MainMenu;
    }

    public void DisplayFieldTeamLiveImage(string path)
    {
        Image liveFootageImage = liveFootageObj.GetComponent<Image>();

        Texture2D texture = Utility.LoadImageFile(path);
        liveFootageImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }
}
