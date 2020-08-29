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
        Clues,
        Communications
    };


    public MainController mainController;

    //public ScrollRect scrollRect;

    public GameObject mainMenuPageObj;
    public GameObject fieldTeamDetailsPageObj;
    public GameObject messagesPageObj;
    public GameObject cluesPageObj;
    public GameObject communicationsPageObj;

    public GameObject ftdPageTeamColorIconObj;
    public GameObject ftdPageTeamNameTextObj;

    public GameObject messagesPageTeamColorIconObj;
    public GameObject messagesPageTeamNameTextObj;

    public GameObject cluesPageTeamColorIconObj;
    public GameObject cluesPageTeamNameTextObj;

    public GameObject communicationsPageTeamColorIconObj;
    public GameObject communicationsPageTeamNameTextObj;

    public GameObject liveFootageObj;

    public MessagesPage messagesPage;
    public CluesPage cluesPage;
    public CommunicationsPage communicationsPage;

    public FieldTeam selectedFieldTeam = null;

    public CurrentlyActivePage currentlyActivePage = CurrentlyActivePage.MainMenu;


    private ImageLoader _imageLoader;

    private string _lastDisplayedThumbnailImagePath;


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

        DisplayFieldTeamLiveImage(
            selectedFieldTeam.GetPhotoPathFromSimulatedTime(selectedFieldTeam.simulatedTimeLastOnline),
            selectedFieldTeam.GetPhotoThumbnailPathFromSimulatedTime(selectedFieldTeam.simulatedTimeLastOnline),
            selectedFieldTeam.GetGrayscalePhotoThumbnailPathFromSimulatedTime(selectedFieldTeam.simulatedTimeLastOnline)
            );

        currentlyActivePage = CurrentlyActivePage.FieldTeamDetails;
    }

    public void ShowMessages()
    {
        messagesPageTeamColorIconObj.GetComponent<Image>().color = selectedFieldTeam.teamColor;
        messagesPageTeamNameTextObj.GetComponent<Text>().text = selectedFieldTeam.teamName + " Messages";

        fieldTeamDetailsPageObj.SetActive(false);
        messagesPageObj.SetActive(true);

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

        // Load field team's clues
        foreach (Clue clue in selectedFieldTeam.revealedClues)
        {
            cluesPage.AddClueBox(clue);
        }

        currentlyActivePage = CurrentlyActivePage.Clues;
    }

    public void ShowCommunications()
    {
        communicationsPageTeamColorIconObj.GetComponent<Image>().color = selectedFieldTeam.teamColor;
        communicationsPageTeamNameTextObj.GetComponent<Text>().text = selectedFieldTeam.teamName + " Clues and Messages";

        fieldTeamDetailsPageObj.SetActive(false);
        communicationsPageObj.SetActive(true);

        // Load field team's clues
        foreach (Communication communication in selectedFieldTeam.revealedCommunications)
        {
            communicationsPage.AddCommunicationBox(communication);
        }

        currentlyActivePage = CurrentlyActivePage.Communications;
    }

    public override void OnBackButtonClick(GameObject fromPage, GameObject toPage)
    {
        mainController.ShowAllFieldTeams(false);
        selectedFieldTeam = null;

        currentlyActivePage = CurrentlyActivePage.MainMenu;

        Image liveFootageImage = liveFootageObj.GetComponent<Image>();
        liveFootageImage.sprite = null;

        _lastDisplayedThumbnailImagePath = null;
    }

    public void DisplayFieldTeamLiveImage(string fullImagePath, string thumbnailPath, string grayscaleThumbnailPath)
    {
        if (_lastDisplayedThumbnailImagePath == null || _lastDisplayedThumbnailImagePath != thumbnailPath)
        {
            Image liveFootageImage = liveFootageObj.GetComponent<Image>();

            if (selectedFieldTeam.isInRadioDeadZone)
            {
                _imageLoader.StartLoading(grayscaleThumbnailPath, this, mainController.footageThumbnailsCache);
            }
            else
            {
                Texture2D texture = Utility.LoadImageFile(thumbnailPath);
                liveFootageImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

                //_imageLoader.StartLoading(thumbnailPath, this, mainController.footageThumbnailsCache);
            }

            if (mainController.footageFullscreenView != null && mainController.footageFullscreenViewShowingLive)
            {
                mainController.footageFullscreenView.DisplayFullscreenImage(fullImagePath, thumbnailPath);
            }

            _lastDisplayedThumbnailImagePath = thumbnailPath;
        }
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        Image liveFootageImage = liveFootageObj.GetComponent<Image>();
        liveFootageImage.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));
    }
}
