using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class TeamTimeline : MonoBehaviour
{
    public FieldTeam fieldTeam;

    public GameObject timelineHighlightPrefab;
    public GameObject timelineNeedlePrefab;

    private Color _lastTeamColor;
    private string _lastTeamName;

    private GameObject _line;

    private GameObject _mapFrameDisplayObj;
    private MapFrameDisplay _mapFrameDisplay;

    private GraphicRaycaster _raycaster;
    private PointerEventData _pointerEventData;
    private EventSystem _eventSystem;

    private bool _mapFrameDisplayShowing = false;

    private GameObject _wholeScreenUiObj;
    private RectTransform _wholeScreenUiCanvasRect;
    private GameObject _timelineUiObj;
    private RectTransform _timelineUiCanvasRect;

    private GameObject _timelineHighlight;
    private bool _timelineHighlightShowing = false;
    private GameObject _timelineNeedle;
    private bool _timelineNeedleShowing = false;

    private float _beginPos = 0.0f;
    private float _endPos = 0.0f;

    private float _scaleX = 0.0f;
    private float _n = 0.0f;
    private float _pivotX = 0.0f;

    private bool _hoveringOverLine = false;
    private bool _hoveringOverLineOnLastFrame = false;

    private DateTime _endTimeOfTimeline;

    void Start()
    {
        _line = this.transform.Find("Line").gameObject;

        // For cursor hovering/clicking on timeline
        _raycaster = this.GetComponentInParent<GraphicRaycaster>();
        _eventSystem = GetComponent<EventSystem>();

        _wholeScreenUiObj = fieldTeam.mainController.wholeScreenUiObj;
        _wholeScreenUiCanvasRect = _wholeScreenUiObj.GetComponent<RectTransform>();
        _timelineUiObj = fieldTeam.mainController.timelineUiObj;
        _timelineUiCanvasRect = _timelineUiObj.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (fieldTeam)
        {
            /* Update icon colour if team's colour changed */

            if (fieldTeam.teamColor != _lastTeamColor)
            {
                _lastTeamColor = fieldTeam.teamColor;
                Image img = _line.GetComponent<Image>();
                img.color = fieldTeam.teamColor;
            }


            /* Update name if team's name changed */

            if (fieldTeam.teamName != _lastTeamName)
            {
                _lastTeamName = fieldTeam.teamName;
                Text txt = this.transform.Find("Label").GetComponent<Text>();
                txt.text = this.fieldTeam.teamName;
            }


            /* Draw/update timeline markings */

            if (fieldTeam.mainController.currentSimulatedTime.dateTime < fieldTeam.simulatedEndTime.dateTime)
                _endTimeOfTimeline = fieldTeam.mainController.currentSimulatedTime.dateTime;
            else
                _endTimeOfTimeline = fieldTeam.simulatedEndTime.dateTime;

            RectTransform lineTransform = _line.GetComponent<RectTransform>();

            _scaleX =
                (float)(_endTimeOfTimeline.Ticks - fieldTeam.simulatedStartTime.dateTime.Ticks) /
                (float)(fieldTeam.mainController.currentSimulatedTime.dateTime.Ticks - fieldTeam.mainController.earliestSimulatedStartTime.dateTime.Ticks);

            _n =
                (float)(fieldTeam.simulatedStartTime.dateTime.Ticks - fieldTeam.mainController.earliestSimulatedStartTime.dateTime.Ticks) /
                (float)(fieldTeam.mainController.currentSimulatedTime.dateTime.Ticks - fieldTeam.mainController.earliestSimulatedStartTime.dateTime.Ticks);
            if (_scaleX == 1.0f)
                _pivotX = 0.0f;
            else
                _pivotX = _n + _n / (1.0f - _scaleX) * _scaleX;

            lineTransform.pivot = new Vector2(_pivotX, lineTransform.pivot.y);
            lineTransform.localScale = new Vector3(_scaleX, lineTransform.localScale.y, lineTransform.localScale.z);

            _beginPos = _n * (_timelineUiCanvasRect.sizeDelta.x - 25.0f) + 5.0f;
            _endPos = (_n + _scaleX) * (_timelineUiCanvasRect.sizeDelta.x - 25.0f) + 5.0f;


            /* Display team footage thumbnail preview if cursor hovering over timeline */

            // Set up the new Pointer Event
            _pointerEventData = new PointerEventData(_eventSystem);
            // Set the Pointer Event Position to that of the cursor position
            _pointerEventData.position = Input.mousePosition;

            // Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            // Raycast using the Graphics Raycaster and cursor position
            _raycaster.Raycast(_pointerEventData, results);

            RaycastResult result = new RaycastResult();
            bool prevHoveringOverLine = _hoveringOverLine;
            _hoveringOverLine = false;
            foreach (RaycastResult res in results)
            {
                if (res.gameObject == _line) // cursor is hovering over line
                {
                    _hoveringOverLine = true;
                    result = res;
                    break;
                }
            }

            if (fieldTeam.mainController.sideUi.selectedFieldTeam == null)
            {
                if (prevHoveringOverLine == false && _hoveringOverLine == true)
                {
                    fieldTeam.ShowThisFieldTeamOnly();
                }
                else if (prevHoveringOverLine == true && _hoveringOverLine == false)
                {
                    fieldTeam.ShowAllFieldTeams();
                }
            }

            if (_hoveringOverLine)
            {
                _hoveringOverLineOnLastFrame = true;

                Vector2 referencePosition = new Vector2(
                    result.screenPosition.x / _wholeScreenUiObj.GetComponent<Canvas>().pixelRect.size.x * _wholeScreenUiObj.GetComponent<CanvasScaler>().referenceResolution.x,
                    result.screenPosition.y / _wholeScreenUiObj.GetComponent<Canvas>().pixelRect.size.y * _wholeScreenUiObj.GetComponent<CanvasScaler>().referenceResolution.y
                );

                float placeHighlighted = (referencePosition.x - _beginPos) / (_endPos - _beginPos);
                if (placeHighlighted < 0)
                    placeHighlighted = 0;
                else if (placeHighlighted > 1)
                    placeHighlighted = 1;

                long ticks = fieldTeam.simulatedStartTime.dateTime.Ticks +
                    (long)(placeHighlighted * (_endTimeOfTimeline.Ticks - fieldTeam.simulatedStartTime.dateTime.Ticks));

                DateTime simulatedTimeHighlighted = new DateTime(ticks);

                string imagePath = fieldTeam.GetPhotoThumbnailPathFromSimulatedTime(simulatedTimeHighlighted);

                if (!_mapFrameDisplayShowing)
                {
                    _mapFrameDisplayObj = Instantiate(fieldTeam.mapFrameDisplayPrefab, _wholeScreenUiObj.transform);
                    _mapFrameDisplayObj.transform.Find("Background").GetComponent<Image>().color = fieldTeam.teamColor;
                    _mapFrameDisplayObj.transform.Find("Arrow").GetComponent<Image>().color = fieldTeam.teamColor;
                    _mapFrameDisplay = _mapFrameDisplayObj.GetComponent<MapFrameDisplay>();
                    _mapFrameDisplay.fieldTeam = fieldTeam;
                    _mapFrameDisplayShowing = true;
                }

                _mapFrameDisplay.DisplayImage(imagePath);
                _mapFrameDisplayObj.transform.Find("Time").GetComponent<Text>().text = simulatedTimeHighlighted.ToString("MM/dd/yyyy HH:mm:ss");

                Vector2 pos = new Vector2(
                    referencePosition.x - 0.5f * _wholeScreenUiCanvasRect.sizeDelta.x,
                    referencePosition.y - 0.5f * _wholeScreenUiCanvasRect.sizeDelta.y);
                float halfMapFrameWidth = _mapFrameDisplayObj.transform.Find("Background").GetComponent<RectTransform>().rect.width * 0.5f;

                RectTransform arrowTransform = _mapFrameDisplayObj.transform.Find("Arrow").GetComponent<RectTransform>();
                if (pos.x - halfMapFrameWidth < -0.5f * _wholeScreenUiCanvasRect.sizeDelta.x)
                {
                    float originalPos = pos.x;
                    pos.x = -0.5f * _wholeScreenUiCanvasRect.sizeDelta.x + halfMapFrameWidth;

                    arrowTransform.localPosition = new Vector3(originalPos - pos.x, arrowTransform.localPosition.y, arrowTransform.localPosition.z);

                    //float arrowWidth = arrowTransform.rect.width;
                    //if (arrowTransform.localPosition.x - 0.5f * arrowWidth < -halfMapFrameWidth)
                    //{
                    //    float newWidth = arrowWidth - (halfMapFrameWidth - (arrowTransform.localPosition.x - 0.5f * arrowWidth));
                    //    arrowTransform.localScale = new Vector3(newWidth, arrowTransform.localScale.y, arrowTransform.localScale.z);
                    //}
                    //else
                    //{
                    //    arrowTransform.localScale = new Vector3(25.0f, arrowTransform.localScale.y, arrowTransform.localScale.z);
                    //}
                }
                else
                {
                    arrowTransform.localPosition = new Vector3(0.0f, arrowTransform.localPosition.y, arrowTransform.localPosition.z);
                }

                _mapFrameDisplayObj.GetComponent<RectTransform>().anchoredPosition = pos;

                fieldTeam.UnhighlightPath();
                fieldTeam.HighlightPathAtSimulatedTime(simulatedTimeHighlighted);
            }
            else if (_hoveringOverLineOnLastFrame)
            {
                GameObject.Destroy(_mapFrameDisplayObj);
                _mapFrameDisplayShowing = false;

                fieldTeam.UnhighlightPath();
                _hoveringOverLineOnLastFrame = false;
            }
        }
    }

    public void HighlightTimeOnTimeline(DateTime timeHighlighted)
    {
        if (!_timelineHighlightShowing)
        {
            _timelineHighlight = Instantiate(timelineHighlightPrefab, _timelineUiObj.transform);
            _timelineHighlightShowing = true;
        }

        float placeToHighlight = (float)(timeHighlighted.Ticks - fieldTeam.simulatedStartTime.dateTime.Ticks) / (float)(_endTimeOfTimeline.Ticks - fieldTeam.simulatedStartTime.dateTime.Ticks);
        if (placeToHighlight < 0.0f)
            placeToHighlight = 0.0f;
        else if (placeToHighlight > 1.0f)
            placeToHighlight = 1.0f;

        Camera timelineCamera = fieldTeam.mainController.timelineCameraObj.GetComponent<Camera>();
        RectTransform canvasRect = _timelineUiObj.GetComponent<RectTransform>();
        Vector2 viewportPos = timelineCamera.WorldToViewportPoint(_line.transform.position);
        Vector2 worldObjTimelineViewPos = new Vector2(
            viewportPos.x * canvasRect.sizeDelta.x,
            viewportPos.y * canvasRect.sizeDelta.y + 5.0f
        );

        Vector2 pos = new Vector2(_beginPos + placeToHighlight * (_endPos - _beginPos) - 0.5f * canvasRect.sizeDelta.x, worldObjTimelineViewPos.y - 0.5f * canvasRect.sizeDelta.y);

        _timelineHighlight.GetComponent<RectTransform>().anchoredPosition = pos;

        MoveNeedleToTime(timeHighlighted);
    }

    public void UnhighlightTimeline()
    {
        GameObject.Destroy(_timelineHighlight);
        _timelineHighlightShowing = false;
        RemoveNeedle();
    }

    private void MoveNeedleToTime(DateTime timeHighlighted)
    {
        if (!_timelineNeedleShowing)
        {
            _timelineNeedle = Instantiate(timelineNeedlePrefab, _timelineUiObj.transform);
            _timelineNeedleShowing = true;
        }

        float placeToHighlight = (float)(timeHighlighted.Ticks - fieldTeam.simulatedStartTime.dateTime.Ticks) / (float)(_endTimeOfTimeline.Ticks - fieldTeam.simulatedStartTime.dateTime.Ticks);
        if (placeToHighlight < 0.0f)
            placeToHighlight = 0.0f;
        else if (placeToHighlight > 1.0f)
            placeToHighlight = 1.0f;

        Camera timelineCamera = fieldTeam.mainController.timelineCameraObj.GetComponent<Camera>();
        RectTransform canvasRect = _timelineUiObj.GetComponent<RectTransform>();
        Vector2 viewportPos = timelineCamera.WorldToViewportPoint(_line.transform.position);
        Vector2 worldObjScreenPos = new Vector2(
            viewportPos.x * canvasRect.sizeDelta.x,
            _timelineNeedle.GetComponent<RectTransform>().rect.height / 2.0f
        );

        Vector2 pos = new Vector2(_beginPos + placeToHighlight * (_endPos - _beginPos) - 0.5f * canvasRect.sizeDelta.x, worldObjScreenPos.y - 0.5f * canvasRect.sizeDelta.y);

        _timelineNeedle.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    private void RemoveNeedle()
    {
        GameObject.Destroy(_timelineNeedle);
        _timelineNeedleShowing = false;
    }
}
