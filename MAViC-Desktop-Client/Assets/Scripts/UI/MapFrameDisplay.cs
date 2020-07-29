﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapFrameDisplay : MonoBehaviour, IImageLoadedHandler
{
    public FieldTeam fieldTeam;

    public Image image;

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

    public void DisplayImage(string path)
    {
        //Texture2D texture = Utility.LoadImageFile(path);
        //image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

        Start();
        _imageLoader.StartLoading(path, this, fieldTeam.mainController.footageThumbnailsCache);
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        image.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));
    }
}
