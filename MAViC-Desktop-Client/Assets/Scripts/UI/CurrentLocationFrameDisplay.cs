﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CurrentLocationFrameDisplay : MonoBehaviour
{
    public Image image;
    public Text teamNameText;

    public void SetTeamName(string teamName)
    {
        teamNameText.text = teamName;
    }

    public void DisplayImage(string path)
    {
        Texture2D texture = Utility.LoadImageFile(path);
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }
}