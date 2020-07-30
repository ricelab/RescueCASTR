using UnityEngine;
using UnityEngine.EventSystems;

public class FullscreenViewCloseButton : MonoBehaviour, IPointerClickHandler
{
    public GameObject fullScreenViewObj;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject.Destroy(fullScreenViewObj);
    }
}
