using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public interface IImageLoadedHandler
{
    void ImageLoaded(Texture2D imageTexture, object optionalParameter);
}

public class ImageLoaderCache : MonoBehaviour
{
    public Dictionary<string, Texture2D> cacheDictionary;
}

public class ImageLoader : MonoBehaviour
{
    public enum LoadStatus
    {
        NotStarted,
        Loading,
        Success,
        Fail
    };

    public LoadStatus loadStatus = LoadStatus.NotStarted;
    public string error = null;
    public Texture2D imageTexture;

    private string _url = null;
    private IImageLoadedHandler _imageLoadedHandler = null;
    private object _optionalParameter = null;

    private Coroutine _coroutine;

    public bool StartLoading(string newUrl, IImageLoadedHandler newImageLoaderHandler, object newOptionalParameter = null)
    {
        if (loadStatus != LoadStatus.Loading)
        {
            _url = newUrl;
            _imageLoadedHandler = newImageLoaderHandler;
            _optionalParameter = newOptionalParameter;
            _coroutine = StartCoroutine(LoadingEnumerator());

            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator LoadingEnumerator()
    {
        loadStatus = LoadStatus.Loading;

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(_url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            error = www.error;
            loadStatus = LoadStatus.Fail;
        }
        else
        {
            imageTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            loadStatus = LoadStatus.Success;
            _imageLoadedHandler.ImageLoaded(imageTexture, _optionalParameter);
        }
    }
}
