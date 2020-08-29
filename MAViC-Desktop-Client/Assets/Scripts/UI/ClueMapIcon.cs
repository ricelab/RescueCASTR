using UnityEngine;
using UnityEngine.EventSystems;

public class ClueMapIcon : ClueIcon, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject iconImgObj;
    public GameObject iconArrowObj;

    public void Update()
    {
        if (_clue != null)
        {
            Camera sceneCamera = _clue.fieldTeam.mainController.sceneCameraObj.GetComponent<Camera>();
            RectTransform canvasRect = _clue.fieldTeam.mainController.sceneUiObj.GetComponent<RectTransform>();
            Vector3 viewportPos = sceneCamera.WorldToViewportPoint(_clue.fieldTeam.mainController.map.ConvertLocationToMapPosition(_clue.location));
            Vector2 worldObjScreenPos = new Vector2(
                ((viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
                ((viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f))
            );
            this.gameObject.GetComponent<RectTransform>().anchoredPosition = worldObjScreenPos;
            if (viewportPos.z < 0.0f)
            {
                iconImgObj.SetActive(false);
                iconArrowObj.SetActive(false);
            }
            else
            {
                iconImgObj.SetActive(true);
                iconArrowObj.SetActive(true);
            }

            if (_clue.highlightMapIcon)
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

        _clue.highlightTimelineIcon = true;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        _clue.highlightTimelineIcon = false;
    }
}
