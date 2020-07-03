using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class TeamTimelineLogic : MonoBehaviour
{
    public FieldTeam fieldTeam;

    public GameObject timelineHighlightPrefab;
    public GameObject timelineNeedlePrefab;

    private Color _lastTeamColor;
    private string _lastTeamName;

    private GameObject _line;

    private GameObject _mapFrameDisplay;
    private MapFrameDisplayLogic _mapFrameDisplayLogic;

    private GraphicRaycaster _raycaster;
    private PointerEventData _pointerEventData;
    private EventSystem _eventSystem;

    private bool _mapFrameDisplayShowing = false;

    private GameObject _sceneUi;

    private GameObject _timelineHighlight;
    private bool _timelineHighlightShowing = false;
    private GameObject _timelineNeedle;
    private bool _timelineNeedleShowing = false;

    private float _beginPos = 0.0f;
    private float _endPos = 0.0f;

    private float _scaleX = 0.0f;
    private float _n = 0.0f;
    private float _pivotX = 0.0f;

    void Start()
    {
        _line = this.transform.Find("Line").gameObject;

        // For cursor hovering/clicking on timeline
        _raycaster = this.GetComponentInParent<GraphicRaycaster>();
        _eventSystem = GetComponent<EventSystem>();

        _sceneUi = fieldTeam.fieldTeamsLogic.sceneUi;
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

            RectTransform lineTransform = _line.GetComponent<RectTransform>();
            
            _scaleX =
                (float)(fieldTeam.endTime.dateTime.Ticks - fieldTeam.startTime.dateTime.Ticks) /
                (float)(fieldTeam.fieldTeamsLogic.latestEndTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks);

            _n =
                (float)(fieldTeam.startTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks) /
                (float)(fieldTeam.fieldTeamsLogic.latestEndTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks);
            if (_scaleX == 1.0f)
                _pivotX = 0.0f;
            else
                _pivotX = _n + _n / (1.0f - _scaleX) * _scaleX;

            lineTransform.pivot = new Vector2(_pivotX, lineTransform.pivot.y);
            lineTransform.localScale = new Vector3(_scaleX, lineTransform.localScale.y, lineTransform.localScale.z);

            _beginPos = _n * (0.75f * (float)Screen.width - 25.0f) + 5.0f;
            _endPos = (_n + _scaleX) * (0.75f * (float)Screen.width - 25.0f) + 5.0f;


            /* Display team footage thumbnail preview if cursor hovering over timeline */

            // Set up the new Pointer Event
            _pointerEventData = new PointerEventData(_eventSystem);
            // Set the Pointer Event Position to that of the cursor position
            _pointerEventData.position = Input.mousePosition;

            // Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            // Raycast using the Graphics Raycaster and cursor position
            _raycaster.Raycast(_pointerEventData, results);

            bool hoveringOverLine = false;
            foreach (RaycastResult result in results)
            {
                if (result.gameObject == _line)
                {
                    hoveringOverLine = true;

                    float placeHighlighted = (result.screenPosition.x - _beginPos) / (_endPos - _beginPos);
                    if (placeHighlighted < 0)
                        placeHighlighted = 0;
                    else if (placeHighlighted > 1)
                        placeHighlighted = 1;

                    long ticks = fieldTeam.startTime.dateTime.Ticks +
                        (long)(placeHighlighted * (fieldTeam.endTime.dateTime.Ticks - fieldTeam.startTime.dateTime.Ticks));

                    DateTime timeHighlighted = new DateTime(ticks);

                    string imagePath = fieldTeam.GetPhotoThumbnailPathFromTime(timeHighlighted);
                    
                    if (!_mapFrameDisplayShowing)
                    {
                        _mapFrameDisplay = Instantiate(fieldTeam.mapFrameDisplayPrefab, _sceneUi.transform);
                        _mapFrameDisplay.transform.Find("Background").GetComponent<Image>().color = fieldTeam.teamColor;
                        _mapFrameDisplay.transform.Find("Arrow").GetComponent<Image>().color = fieldTeam.teamColor;
                        _mapFrameDisplayLogic = _mapFrameDisplay.GetComponent<MapFrameDisplayLogic>();
                        _mapFrameDisplayShowing = true;
                    }
                    
                    _mapFrameDisplayLogic.DisplayImage(imagePath);
                    
                    Vector2 pos = new Vector2(
                        result.screenPosition.x - 0.5f * Screen.width, result.screenPosition.y - 0.5f * Screen.height);
                    float halfMapFrameWidth = _mapFrameDisplay.transform.Find("Background").GetComponent<RectTransform>().rect.width * 0.5f;

                    RectTransform arrowTransform = _mapFrameDisplay.transform.Find("Arrow").GetComponent<RectTransform>();
                    if (pos.x - halfMapFrameWidth < -0.5f * Screen.width)
                    {
                        float originalPos = pos.x;
                        pos.x = -0.5f * Screen.width + halfMapFrameWidth;

                        arrowTransform.localPosition = new Vector3(originalPos - pos.x, arrowTransform.localPosition.y, arrowTransform.localPosition.z);

                        //float arrowWidth = arrowTransform.rect.width;
                        //if (arrowTransform.localPosition.x - 0.5f * arrowWidth < -halfMapFrameWidth)
                        //{
                        //    Debug.Log("here");
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
                    
                    _mapFrameDisplay.GetComponent<RectTransform>().anchoredPosition = pos;

                    fieldTeam.UnhighlightPath();
                    fieldTeam.HighlightPathAtTime(timeHighlighted);

                    break;
                }
            }

            if (!hoveringOverLine)
            {
                GameObject.Destroy(_mapFrameDisplay);
                _mapFrameDisplayShowing = false;

                fieldTeam.UnhighlightPath();
            }
        }
    }

    public void HighlightTimeOnTimeline(DateTime timeHighlighted)
    {
        if (!_timelineHighlightShowing)
        {
            _timelineHighlight = Instantiate(timelineHighlightPrefab, _sceneUi.transform);
            _timelineHighlightShowing = true;
        }

        float placeToHighlight = (float)(timeHighlighted.Ticks - fieldTeam.startTime.dateTime.Ticks) / (float)(fieldTeam.endTime.dateTime.Ticks - fieldTeam.startTime.dateTime.Ticks);
        if (placeToHighlight < 0.0f)
            placeToHighlight = 0.0f;
        else if (placeToHighlight > 1.0f)
            placeToHighlight = 1.0f;

        Camera timelineCamera = fieldTeam.fieldTeamsLogic.timelineCamera.GetComponent<Camera>();
        RectTransform canvasRect = fieldTeam.fieldTeamsLogic.sceneUi.GetComponent<RectTransform>();
        Vector2 viewportPos = timelineCamera.WorldToViewportPoint(_line.transform.position);
        Vector2 worldObjScreenPos = new Vector2(
            (viewportPos.x * canvasRect.sizeDelta.x * 0.75f) - (canvasRect.sizeDelta.x * 0.5f),
            viewportPos.y * canvasRect.sizeDelta.y * 0.2f + 5.0f
        );

        Vector2 pos = new Vector2(_beginPos + placeToHighlight * (_endPos - _beginPos) - 0.5f * Screen.width, worldObjScreenPos.y - 0.5f * Screen.height);

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
            _timelineNeedle = Instantiate(timelineNeedlePrefab, _sceneUi.transform);
            _timelineNeedleShowing = true;
        }

        float placeToHighlight = (float)(timeHighlighted.Ticks - fieldTeam.startTime.dateTime.Ticks) / (float)(fieldTeam.endTime.dateTime.Ticks - fieldTeam.startTime.dateTime.Ticks);
        if (placeToHighlight < 0.0f)
            placeToHighlight = 0.0f;
        else if (placeToHighlight > 1.0f)
            placeToHighlight = 1.0f;

        Camera timelineCamera = fieldTeam.fieldTeamsLogic.timelineCamera.GetComponent<Camera>();
        RectTransform canvasRect = fieldTeam.fieldTeamsLogic.sceneUi.GetComponent<RectTransform>();
        Vector2 viewportPos = timelineCamera.WorldToViewportPoint(_line.transform.position);
        Vector2 worldObjScreenPos = new Vector2(
            ((viewportPos.x * canvasRect.sizeDelta.x * 0.75f) - (canvasRect.sizeDelta.x * 0.5f)),
            _timelineNeedle.GetComponent<RectTransform>().rect.height / 2.0f
        );

        Vector2 pos = new Vector2(_beginPos + placeToHighlight * (_endPos - _beginPos) - 0.5f * Screen.width, worldObjScreenPos.y - 0.5f * Screen.height);

        _timelineNeedle.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    private void RemoveNeedle()
    {
        GameObject.Destroy(_timelineNeedle);
        _timelineNeedleShowing = false;
    }
}
