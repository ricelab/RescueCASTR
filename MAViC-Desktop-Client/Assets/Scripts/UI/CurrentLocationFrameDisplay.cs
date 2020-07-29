using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CurrentLocationFrameDisplay : MonoBehaviour, IImageLoadedHandler
{
    public Image image;
    public Text teamNameText;

    private ImageLoader _imageLoader;

    public void Start()
    {
        _imageLoader = this.gameObject.AddComponent<ImageLoader>();
    }

    public void SetTeamName(string teamName)
    {
        teamNameText.text = teamName;
    }

    public void DisplayImage(string path)
    {
        //Texture2D texture = Utility.LoadImageFile(path);
        //image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

        if (_imageLoader != null)
            _imageLoader.StartLoading(path, this);
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
