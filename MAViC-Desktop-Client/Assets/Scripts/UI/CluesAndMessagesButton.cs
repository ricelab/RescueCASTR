using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CluesAndMessagesButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SideUi sideUi;

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.GetComponent<Image>().color = new Color(0.87f, 0.87f, 0.87f, 1.0f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        sideUi.ShowCluesAndMessages();
    }
}
