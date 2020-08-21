using UnityEngine;
using UnityEngine.EventSystems;

public class MessageMapIcon : MessageIcon, IPointerEnterHandler, IPointerExitHandler
{
    public void Update()
    {
        if (_message != null)
        {
            Camera sceneCamera = _message.fieldTeam.mainController.sceneCameraObj.GetComponent<Camera>();
            RectTransform canvasRect = _message.fieldTeam.mainController.sceneUiObj.GetComponent<RectTransform>();
            Vector2 viewportPos = sceneCamera.WorldToViewportPoint(_message.fieldTeam.mainController.map.ConvertLocationToMapPosition(_message.location));
            Vector2 worldObjScreenPos = new Vector2(
                ((viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
                ((viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f))
            );
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = worldObjScreenPos;

            if (_message.highlightMapIcon)
            {
                this.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                this.transform.SetAsLastSibling();
            }
            else
            {
                this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        _message.highlightTimelineIcon = true;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        _message.highlightTimelineIcon = false;
    }
}
