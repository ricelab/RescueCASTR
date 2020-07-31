using UnityEngine;
using UnityEngine.EventSystems;

public class PLS_LKP_Pin : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject labelObj;

    public void OnPointerEnter(PointerEventData eventData)
    {
        labelObj.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        labelObj.SetActive(false);
    }
}
