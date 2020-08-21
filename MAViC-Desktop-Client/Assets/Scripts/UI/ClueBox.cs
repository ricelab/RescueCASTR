using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClueBox : MonoBehaviour, IImageLoadedHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Clue clue;
    public Image image;
    public Text clueText;
    public Text clueTimeText;

    private bool _isStarted = false;
    private ImageLoader _imageLoader;

    public void Start()
    {
        if (!_isStarted)
        {
            _imageLoader = this.gameObject.AddComponent<ImageLoader>();
            this.transform.Find("Background").GetComponent<Image>().color = new Color(0.51f, 0.65f, 0.75f, 1.0f);

            _isStarted = true;
        }
    }

    public void LoadImage(string path)
    {
        Start();
        _imageLoader.StartLoading(path, this, clue.fieldTeam.mainController.cluesPhotosCache);
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        image.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.transform.Find("Background").GetComponent<Image>().color = new Color(0.98f, 0.88f, 0.74f, 1.0f);
        clue.fieldTeam.HighlightPathAtSimulatedTime(clue.simulatedTime.dateTime);
        clue.fieldTeam.HighlightSimulatedTimeOnTimeline(clue.simulatedTime);
        clue.highlightMapIcon = true;
        clue.highlightTimelineIcon = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.transform.Find("Background").GetComponent<Image>().color = new Color(0.51f, 0.65f, 0.75f, 1.0f);
        clue.fieldTeam.UnhighlightPath();
        clue.fieldTeam.UnhighlightTimeline();
        clue.highlightMapIcon = false;
        clue.highlightTimelineIcon = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clue.fieldTeam.mainController.fullscreenViewObj =
                GameObject.Instantiate(clue.fieldTeam.mainController.clueFullscreenViewPrefab, clue.fieldTeam.mainController.wholeScreenUiObj.transform);
        clue.fieldTeam.mainController.clueFullscreenView = clue.fieldTeam.mainController.fullscreenViewObj.GetComponent<ClueFullscreenView>();
        clue.fieldTeam.mainController.clueFullscreenView.clue = clue;
    }
}
