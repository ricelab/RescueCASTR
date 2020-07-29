using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SideUi : ABackButtonClickHandler, // ABackButtonClickHandler inherits MonoBehaviour, and is defined in BackButton.cs
    IImageLoadedHandler
{
    public enum CurrentlyActivePage
    {
        MainMenu,
        FieldTeamDetails,
        Messages,
        Clues
    };


    public MainController mainController;

    //public ScrollRect scrollRect;

    public GameObject mainMenuPageObj;
    public GameObject fieldTeamDetailsPageObj;
    public GameObject messagesPageObj;
    public GameObject cluesPageObj;

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


    private ImageLoader _imageLoader;


    public void Start()
    {
        _imageLoader = this.gameObject.AddComponent<ImageLoader>();
    }

    public void ShowTeamDetails(FieldTeam ft)
    {
        selectedFieldTeam = ft;

        selectedFieldTeam.ShowThisFieldTeamOnly(true);

        ftdPageTeamColorIconObj.GetComponent<Image>().color = selectedFieldTeam.teamColor;
        ftdPageTeamNameTextObj.GetComponent<Text>().text = selectedFieldTeam.teamName;

        mainMenuPageObj.SetActive(false);
        fieldTeamDetailsPageObj.SetActive(true);

        //scrollRect.content = fieldTeamDetailsPageObj.GetComponent<RectTransform>();

        DisplayFieldTeamLiveImage(selectedFieldTeam.GetPhotoPathFromSimulatedTime(mainController.currentSimulatedTime));

        currentlyActivePage = CurrentlyActivePage.FieldTeamDetails;
    }

    public void ShowMessages()
    {
        messagesPageTeamColorIconObj.GetComponent<Image>().color = selectedFieldTeam.teamColor;
        messagesPageTeamNameTextObj.GetComponent<Text>().text = selectedFieldTeam.teamName + " Messages";

        fieldTeamDetailsPageObj.SetActive(false);
        messagesPageObj.SetActive(true);

        //scrollRect.content = messagesPageObj.GetComponent<RectTransform>();

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

        fieldTeamDetailsPageObj.SetActive(false);
        cluesPageObj.SetActive(true);

        //scrollRect.content = cluesPageObj.GetComponent<RectTransform>();

        // Load field team's clues
        foreach (Clue clue in selectedFieldTeam.revealedClues)
        {
            cluesPage.AddClueBox(clue);
        }

        currentlyActivePage = CurrentlyActivePage.Clues;
    }

    public override void OnBackButtonClick(GameObject fromPage, GameObject toPage)
    {
        mainController.ShowAllFieldTeams(false);
        selectedFieldTeam = null;

        //scrollRect.content = mainMenuPageObj.GetComponent<RectTransform>();
        currentlyActivePage = CurrentlyActivePage.MainMenu;

        Image liveFootageImage = liveFootageObj.GetComponent<Image>();
        liveFootageImage.sprite = null;
    }

    public void DisplayFieldTeamLiveImage(string path)
    {
        //Image liveFootageImage = liveFootageObj.GetComponent<Image>();
        //Texture2D texture = Utility.LoadImageFile(path);
        //liveFootageImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

        _imageLoader.StartLoading(path, this, mainController.footageThumbnailsCache);
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        Image liveFootageImage = liveFootageObj.GetComponent<Image>();
        liveFootageImage.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));
    }
}
