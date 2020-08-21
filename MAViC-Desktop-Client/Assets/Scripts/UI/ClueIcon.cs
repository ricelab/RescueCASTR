using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClueIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IImageLoadedHandler
{
    public GameObject expandablePanelObj;
    public Image clueImage;
    public Text clueText;

    public Clue clue
    {
        get
        {
            return _clue;
        }
        set
        {
            _clue = value;

            clueText.text = _clue.textDescription;

            Start();

            _imageLoader.StartLoading(_clue.fieldTeam.mainController.resourcesUrl /* + _clue.fieldTeam.recordingDirectoryPath */ + "clues-photos/" + _clue.photoFileName,
                this,
                _clue.fieldTeam.mainController.cluesPhotosCache);
        }
    }

    protected Clue _clue;

    private bool _isStarted = false;
    private ImageLoader _imageLoader;

    public void Start()
    {
        if (!_isStarted)
        {
            _imageLoader = this.gameObject.AddComponent<ImageLoader>();
            _isStarted = true;
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        expandablePanelObj.SetActive(true);
        this.transform.SetAsLastSibling();
        _clue.fieldTeam.mainController.mouseHoveringOverIcon = true;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        expandablePanelObj.SetActive(false);
        _clue.fieldTeam.mainController.mouseHoveringOverIcon = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _clue.fieldTeam.mainController.fullscreenViewObj =
                GameObject.Instantiate(_clue.fieldTeam.mainController.clueFullscreenViewPrefab, _clue.fieldTeam.mainController.wholeScreenUiObj.transform);
        _clue.fieldTeam.mainController.clueFullscreenView = _clue.fieldTeam.mainController.fullscreenViewObj.GetComponent<ClueFullscreenView>();
        _clue.fieldTeam.mainController.clueFullscreenView.clue = _clue;
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        clueImage.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));
    }
}
