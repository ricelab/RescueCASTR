using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MessageMapIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject expandablePanelObj;
    public Text messageText;

    public Message message
    {
        get
        {
            return _message;
        }
        set
        {
            _message = value;
            messageText.text = _message.messageContent;
        }
    }

    private Message _message;

    public void Update()
    {
        if (message != null)
        {
            Camera sceneCamera = message.fieldTeam.mainController.sceneCameraObj.GetComponent<Camera>();
            RectTransform canvasRect = message.fieldTeam.mainController.sceneUiObj.GetComponent<RectTransform>();
            Vector2 viewportPos = sceneCamera.WorldToViewportPoint(message.fieldTeam.mainController.map.ConvertLocationToMapPosition(message.location));
            Vector2 worldObjScreenPos = new Vector2(
                ((viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
                ((viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f))
            );
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = worldObjScreenPos;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        expandablePanelObj.SetActive(true);
        this.transform.SetAsLastSibling();
        _message.fieldTeam.mainController.mouseHoveringOverIcon = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        expandablePanelObj.SetActive(false);
        _message.fieldTeam.mainController.mouseHoveringOverIcon = false;
    }
}
