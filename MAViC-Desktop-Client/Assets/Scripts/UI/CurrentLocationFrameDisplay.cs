using UnityEngine;
using UnityEngine.UI;

public class CurrentLocationFrameDisplay : MonoBehaviour, IImageLoadedHandler
{
    public FieldTeam fieldTeam;

    public Image image;
    public Text teamNameText;

    private bool _isStarted = false;
    //private ImageLoader _imageLoader;

    private string _lastDisplayedImagePath;

    public void Start()
    {
        if (!_isStarted)
        {
            //_imageLoader = this.gameObject.AddComponent<ImageLoader>();

            _isStarted = true;
        }
    }

    public void SetTeamName(string teamName)
    {
        teamNameText.text = teamName;
    }

    public void DisplayImage(string path)
    {
        if (_lastDisplayedImagePath == null || _lastDisplayedImagePath != path)
        {
            Texture2D texture = Utility.LoadImageFile(path);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

            //Start();
            //_imageLoader.StartLoading(path, this, fieldTeam.mainController.footageThumbnailsCache);

            _lastDisplayedImagePath = path;
        }
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        image.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));
    }

    public void ShowThumbnail()
    {
        this.transform.Find("Background").gameObject.SetActive(true);
        this.transform.Find("Image").gameObject.SetActive(true);
        this.transform.Find("Arrow").gameObject.SetActive(true);
    }

    public void HideThumbnail()
    {
        this.transform.Find("Background").gameObject.SetActive(false);
        this.transform.Find("Image").gameObject.SetActive(false);
        this.transform.Find("Arrow").gameObject.SetActive(false);
    }
}
