using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapFrameDisplay : MonoBehaviour
{
    public Image image;

    public void DisplayImage(string path)
    {
        Texture2D texture = Utility.LoadImageFile(path);
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }
}
