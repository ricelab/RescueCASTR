﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;

public class TeamPathPointLogic : MonoBehaviour
{
    public Location location;
    public DateTime time;
    public int pointNumber;
    public FieldTeam fieldTeam;

    private GameObject _mapFrameDisplay;
    private MapFrameDisplayLogic _mapFrameDisplayLogic;

    private bool _isHighlighted = false;

    void Start()
    {
        this.transform.localScale *= 2.5f;
    }

    void OnMouseEnter()
    {
        //Debug.Log("MouseEnter: " + pointNumber);

        //this.HighlightPathPoint();

        string imagePath = fieldTeam.GetPhotoThumbnailPathFromActualTime(time);

        GameObject sceneUi = fieldTeam.fieldTeamsLogic.sceneUi;

        _mapFrameDisplay = Instantiate(fieldTeam.mapFrameDisplayPrefab, sceneUi.transform);
        _mapFrameDisplay.transform.Find("Background").GetComponent<Image>().color = fieldTeam.teamColor;
        _mapFrameDisplay.transform.Find("Arrow").GetComponent<Image>().color = fieldTeam.teamColor;
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

        fieldTeam.HighlightActualTimeOnTimeline(time);
    }

    void OnMouseExit()
    {
        //Debug.Log("MouseExit: " + pointNumber);

        //this.UnhighlightPathPoint();

        GameObject.Destroy(_mapFrameDisplay);

        fieldTeam.UnhighlightTimeline();
    }

    public void HighlightPathPoint()
    {
        if (!_isHighlighted)
        {
            this.transform.localScale *= 2.0f;
            this.GetComponent<MeshRenderer>().enabled = true;
            _isHighlighted = true;
        }
    }

    public void UnhighlightPathPoint()
    {
        if (_isHighlighted)
        {
            this.GetComponent<MeshRenderer>().enabled = false;
            this.transform.localScale /= 2.0f;
            _isHighlighted = false;
        }
    }
}
