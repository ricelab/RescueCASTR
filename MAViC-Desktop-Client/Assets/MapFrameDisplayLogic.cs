using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapFrameDisplayLogic : MonoBehaviour
{
    public Image image;
    
    // Start is called before the first frame update
    void Start()
    {
        ///
    }

    // Update is called once per frame
    void Update()
    {
        ///
    }

    public void DisplayImage(string path)
    {
        Texture2D texture = LoadImageFile(path);
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }

    private Texture2D LoadImageFile(string path)
    {
        Texture2D texture = null;
        byte[] fileData;

        if (File.Exists(path))
        {
            fileData = File.ReadAllBytes(path);
            texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
        }

        return texture;
    }
}
