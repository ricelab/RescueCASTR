using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public interface IImageLoadedHandler
{
    void ImageLoaded(Texture2D imageTexture, object optionalParameter);
}


public class ImageLoaderCache
{
    private struct ImageLoaderCacheEntry
    {
        public string path;
        public Texture2D imageTexture;
    }

    public int maxCacheSize;

    private Queue<ImageLoaderCacheEntry> _cacheQueue;

    public ImageLoaderCache(int size)
    {
        maxCacheSize = size;

        _cacheQueue = new Queue<ImageLoaderCacheEntry>();
    }

    public Texture2D TryLoadImageFromCache(string path)
    {
        foreach (ImageLoaderCacheEntry imageLoaderCacheEntry in _cacheQueue)
        {
            if (imageLoaderCacheEntry.path == path)
            {
                return imageLoaderCacheEntry.imageTexture;
            }
        }

        return null;
    }

    public void AddImageToCache(string path, Texture2D imageTexture)
    {
        ImageLoaderCacheEntry imageLoaderCacheEntry = new ImageLoaderCacheEntry();
        imageLoaderCacheEntry.path = path;
        imageLoaderCacheEntry.imageTexture = imageTexture;

        if (TryLoadImageFromCache(path) == null)
        {
            _cacheQueue.Enqueue(imageLoaderCacheEntry);
            while (_cacheQueue.Count > maxCacheSize)
            {
                _cacheQueue.Dequeue();
            }
        }
    }
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
    private ImageLoaderCache _imageLoaderCache = null;
    private object _optionalParameter = null;

    private Coroutine _coroutine;

    public bool StartLoading(string url, IImageLoadedHandler imageLoaderHandler, ImageLoaderCache imageLoaderCache = null, object optionalParameter = null)
    {
        if (loadStatus != LoadStatus.Loading)
        {
            _url = url;
            _imageLoadedHandler = imageLoaderHandler;
            _imageLoaderCache = imageLoaderCache;
            _optionalParameter = optionalParameter;

            imageTexture = _imageLoaderCache.TryLoadImageFromCache(_url);
            if (imageTexture != null)
            {
                _imageLoadedHandler.ImageLoaded(imageTexture, _optionalParameter);
            }
            else
            {
                _coroutine = StartCoroutine(LoadingEnumerator());
            }

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
            Debug.LogError("Error loading from " + _url + " Error: " + error);

            loadStatus = LoadStatus.Fail;
        }
        else
        {
            imageTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            loadStatus = LoadStatus.Success;
            _imageLoaderCache.AddImageToCache(_url, imageTexture);
            _imageLoadedHandler.ImageLoaded(imageTexture, _optionalParameter);
        }
    }
}
