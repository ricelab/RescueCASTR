using UnityEngine;

public class MessageMapIcon : MonoBehaviour
{
    public Message message;

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
}
