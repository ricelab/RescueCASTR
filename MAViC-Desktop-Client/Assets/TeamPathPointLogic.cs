using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamPathPointLogic : MonoBehaviour
{
    public Location location;
    public DateTime time;
    public int pointNumber;
    public FieldTeam fieldTeam;

    private GameObject _mapFrameDisplay;
    private MapFrameDisplayLogic _mapFrameDisplayLogic;

    void OnMouseEnter()
    {
        Debug.Log("MouseEnter: " + pointNumber);

        //this.GetComponent<MeshRenderer>().enabled = true;

        string imagePath = fieldTeam.GetPhotoThumbnailPathFromTime(time);

        GameObject sceneUi = fieldTeam.fieldTeamsLogic.sceneUi;

        _mapFrameDisplay = Instantiate(fieldTeam.mapFrameDisplayPrefab, sceneUi.transform);
        _mapFrameDisplayLogic = _mapFrameDisplay.GetComponent<MapFrameDisplayLogic>();
        _mapFrameDisplayLogic.DisplayImage(imagePath);

        Camera sceneCamera = fieldTeam.fieldTeamsLogic.sceneCamera.GetComponent<Camera>();
        RectTransform canvasRect = sceneUi.GetComponent<RectTransform>();
        Vector2 viewportPos = sceneCamera.WorldToViewportPoint(this.transform.position);
        Vector2 worldObjScreenPos = new Vector2(
            ((viewportPos.x * canvasRect.sizeDelta.x * 0.75f) - (canvasRect.sizeDelta.x * 0.5f)),
            ((viewportPos.y * canvasRect.sizeDelta.y * 0.8f) - (canvasRect.sizeDelta.y * 0.5f) + (canvasRect.sizeDelta.y * 0.2f))
        );
        _mapFrameDisplay.GetComponent<RectTransform>().anchoredPosition = worldObjScreenPos;
    }

    void OnMouseExit()
    {
        Debug.Log("MouseExit: " + pointNumber);

        this.GetComponent<MeshRenderer>().enabled = false;

        GameObject.Destroy(_mapFrameDisplay);
    }
}
