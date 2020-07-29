using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapFrameDisplay : MonoBehaviour, IImageLoadedHandler
{
    public Image image;

    private ImageLoader _imageLoader;

    public void Start()
    {
        _imageLoader = this.gameObject.AddComponent<ImageLoader>();
    }

    public void DisplayImage(string path)
    {
        //Texture2D texture = Utility.LoadImageFile(path);
        //image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

        _imageLoader.StartLoading(path, this);
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        image.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));
    }
}
