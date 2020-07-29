using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClueBox : MonoBehaviour, IImageLoadedHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Clue clue;
    public Image image;
    public Text clueText;
    public Text clueTimeText;

    private ImageLoader _imageLoader;

    public void Start()
    {
        _imageLoader = this.gameObject.AddComponent<ImageLoader>();

        this.transform.Find("Background").GetComponent<Image>().color = new Color(0.51f, 0.65f, 0.75f, 1.0f);
    }

    public void LoadImage(string path)
    {
        _imageLoader.StartLoading(path, this);
    }

    public void ImageLoaded(Texture2D imageTexture, object optionalParameter)
    {
        image.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0, 0));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.transform.Find("Background").GetComponent<Image>().color = new Color(0.98f, 0.88f, 0.74f, 1.0f);

        clue.fieldTeam.HighlightPathAtSimulatedTime(clue.simulatedTime.dateTime);

        clue.fieldTeam.HighlightSimulatedTimeOnTimeline(clue.simulatedTime);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.transform.Find("Background").GetComponent<Image>().color = new Color(0.51f, 0.65f, 0.75f, 1.0f);
        clue.fieldTeam.UnhighlightPath();
        clue.fieldTeam.UnhighlightTimeline();
    }
}
