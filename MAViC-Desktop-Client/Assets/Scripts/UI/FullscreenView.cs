using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenView : MonoBehaviour, IImageLoadedHandler
{
    public MainController mainController;

    public Image image;

    private ImageLoader _fullImageLoader;
    private ImageLoader _thumbnailImageLoader;
    private bool _fullImageIsLoaded = false;
    private bool _isStarted = false;

    public void Start()
    {
        if (!_isStarted)
        {
            _fullImageLoader = this.gameObject.AddComponent<ImageLoader>();
            _thumbnailImageLoader = this.gameObject.AddComponent<ImageLoader>();

            _isStarted = true;
        }
    }

    public void DisplayFullscreenImage(string fullImagePath, string thumbnailPath = null)
    {
        Start(); // starts everything up if not yet started

        _fullImageIsLoaded = false;
        if (thumbnailPath != null)
        {
            _thumbnailImageLoader.StartLoading(thumbnailPath, this, mainController.footageThumbnailsCache, false /* !isFullImage */);
        }
        _fullImageLoader.StartLoading(fullImagePath, this, mainController.footagePhotosCache, true /* isFullImage */);
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        bool isFullImage = (bool)optionalParameter;

        if (isFullImage /* full image is loaded */ || !_fullImageIsLoaded /* thumbnail is loaded, but full image is not yet loaded */)
        {
            image.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));

            if (isFullImage)
            {
                _fullImageIsLoaded = true;
            }
        }
    }
}
