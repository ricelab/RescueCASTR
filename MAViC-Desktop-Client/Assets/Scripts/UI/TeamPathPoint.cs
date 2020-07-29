using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;

public class TeamPathPoint : MonoBehaviour
{
    public Location location;
    public DateTime actualTime;
    public int pointNumber;
    public FieldTeam fieldTeam;

    private GameObject _mapFrameDisplayObj;
    private MapFrameDisplay _mapFrameDisplay;

    private bool _isHighlighted = false;

    void Update()
    {
        float scaleFactor = 2.0f;
        if (fieldTeam.mainController.sceneCameraControls.cameraViewingMode == CameraControls.CameraViewingMode._2D)
        {
            scaleFactor = scaleFactor / 50.0f * fieldTeam.mainController.sceneCamera.orthographicSize;
        }
        else // if (fieldTeam.mainController.sceneCameraControls.cameraViewingMode == CameraControls.CameraViewingMode._3D)
        {
            scaleFactor = scaleFactor / 50.0f *
                (fieldTeam.mainController.sceneCameraObj.transform.position.y - fieldTeam.mainController.sceneCameraControls.minimumY);
        }

        this.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        if (_isHighlighted)
        {
            this.transform.localScale *= 2.0f;
        }
    }

    void OnMouseEnter()
    {
        if (fieldTeam.ConvertActualTimeToSimulatedTime(actualTime) <= fieldTeam.mainController.currentSimulatedTime &&
            fieldTeam.fieldTeamAppearStatus == FieldTeam.FieldTeamAppearStatus.Showing)
        {
            //this.HighlightPathPoint();

            string imagePath = fieldTeam.GetPhotoThumbnailPathFromActualTime(actualTime);

            GameObject wholeScreenUiObj = fieldTeam.mainController.wholeScreenUiObj;

            _mapFrameDisplayObj = Instantiate(fieldTeam.mapFrameDisplayPrefab, wholeScreenUiObj.transform);
            _mapFrameDisplayObj.transform.Find("Background").GetComponent<Image>().color = fieldTeam.teamColor;
            _mapFrameDisplayObj.transform.Find("Arrow").GetComponent<Image>().color = fieldTeam.teamColor;
            _mapFrameDisplayObj.transform.Find("Time").GetComponent<Text>().text = fieldTeam.ConvertActualTimeToSimulatedTime(actualTime).ToString("MM/dd/yyyy HH:mm:ss");
            _mapFrameDisplay = _mapFrameDisplayObj.GetComponent<MapFrameDisplay>();
            _mapFrameDisplay.fieldTeam = fieldTeam;
            _mapFrameDisplay.DisplayImage(imagePath);

            Camera sceneCamera = fieldTeam.mainController.sceneCameraObj.GetComponent<Camera>();
            RectTransform canvasRect = wholeScreenUiObj.GetComponent<RectTransform>();
            Vector2 viewportPos = sceneCamera.WorldToViewportPoint(this.transform.position);
            Vector2 worldObjScreenPos = new Vector2(
                ((viewportPos.x * canvasRect.sizeDelta.x * 0.75f) - (canvasRect.sizeDelta.x * 0.5f)),
                ((viewportPos.y * canvasRect.sizeDelta.y * 0.8f) - (canvasRect.sizeDelta.y * 0.5f) + (canvasRect.sizeDelta.y * 0.2f))
            );
            _mapFrameDisplayObj.GetComponent<RectTransform>().anchoredPosition = worldObjScreenPos;

            fieldTeam.HighlightActualTimeOnTimeline(actualTime);
        }
    }

    void OnMouseExit()
    {
        if (fieldTeam.ConvertActualTimeToSimulatedTime(actualTime) <= fieldTeam.mainController.currentSimulatedTime &&
            fieldTeam.fieldTeamAppearStatus == FieldTeam.FieldTeamAppearStatus.Showing)
        {
            //this.UnhighlightPathPoint();

            GameObject.Destroy(_mapFrameDisplayObj);

            fieldTeam.UnhighlightTimeline();
        }
    }

    public void HighlightPathPoint()
    {
        if (!_isHighlighted)
        {
            //this.transform.localScale *= 2.0f;
            this.GetComponent<MeshRenderer>().enabled = true;
            _isHighlighted = true;
        }
    }

    public void UnhighlightPathPoint()
    {
        if (_isHighlighted)
        {
            this.GetComponent<MeshRenderer>().enabled = false;
            //this.transform.localScale /= 2.0f;
            _isHighlighted = false;
        }
    }
}
