using System.IO;
using UnityEngine;

public static class Utility
{
    public static Texture2D LoadImageFile(string path)
    {
        return Resources.Load<Texture2D>(Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path));
    }
}
