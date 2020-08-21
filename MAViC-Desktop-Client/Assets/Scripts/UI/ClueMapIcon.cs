using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClueMapIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IImageLoadedHandler
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

    private Clue _clue;

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

    public void Update()
    {
        if (_clue != null)
        {
            Camera sceneCamera = _clue.fieldTeam.mainController.sceneCameraObj.GetComponent<Camera>();
            RectTransform canvasRect = _clue.fieldTeam.mainController.sceneUiObj.GetComponent<RectTransform>();
            Vector2 viewportPos = sceneCamera.WorldToViewportPoint(_clue.fieldTeam.mainController.map.ConvertLocationToMapPosition(_clue.location));
            Vector2 worldObjScreenPos = new Vector2(
                ((viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
                ((viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f))
            );
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = worldObjScreenPos;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        expandablePanelObj.SetActive(true);
        this.transform.SetAsLastSibling();
        _clue.fieldTeam.mainController.mouseHoveringOverIcon = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        expandablePanelObj.SetActive(false);
        _clue.fieldTeam.mainController.mouseHoveringOverIcon = false;
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        clueImage.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _clue.fieldTeam.mainController.fullscreenViewObj =
                GameObject.Instantiate(_clue.fieldTeam.mainController.clueFullscreenViewPrefab, _clue.fieldTeam.mainController.wholeScreenUiObj.transform);
        _clue.fieldTeam.mainController.clueFullscreenView = _clue.fieldTeam.mainController.fullscreenViewObj.GetComponent<ClueFullscreenView>();
        _clue.fieldTeam.mainController.clueFullscreenView.clue = clue;
    }
}
