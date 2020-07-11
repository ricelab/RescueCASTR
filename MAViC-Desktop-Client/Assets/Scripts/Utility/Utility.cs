using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Utility
{
    public static Texture2D LoadImageFile(string path)
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
