using UnityEngine;
using UnityEngine.UI;

public class ClueFullscreenView : FullscreenView, IImageLoadedHandler
{
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

    public override MainController GetMainController()
    {
        return _clue.fieldTeam.mainController;
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        clueImage.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));
    }
}
