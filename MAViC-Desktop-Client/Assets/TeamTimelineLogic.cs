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

    private Color _lastTeamColor;
    private string _lastTeamName;

    private GameObject _line;

    private GameObject _mapFrameDisplay;
    private MapFrameDisplayLogic _mapFrameDisplayLogic;

    private GraphicRaycaster _raycaster;
    private PointerEventData _pointerEventData;
    private EventSystem _eventSystem;

    private bool _mapFrameDisplayShowing = false;

    void Start()
    {
        _line = this.transform.Find("Line").gameObject;

        /* For cursor hovering/clicking on timeline */

        // Fetch the Raycaster from the GameObject (the Canvas)
        _raycaster = this.GetComponentInParent<GraphicRaycaster>();
        // Fetch the Event System from the Scene
        _eventSystem = GetComponent<EventSystem>();
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
            
            float scaleX =
                (float)(fieldTeam.endTime.dateTime.Ticks - fieldTeam.startTime.dateTime.Ticks) /
                (float)(fieldTeam.fieldTeamsLogic.latestEndTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks);

            float n =
                (float)(fieldTeam.startTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks) /
                (float)(fieldTeam.fieldTeamsLogic.latestEndTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks);
            float pivotX = n + n / (1.0f - scaleX) * scaleX;

            lineTransform.pivot = new Vector2(pivotX, lineTransform.pivot.y);
            lineTransform.localScale = new Vector3(scaleX, lineTransform.localScale.y, lineTransform.localScale.z);


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

                    float beginPos = n * (0.75f * (float)Screen.width - 25.0f) + 5.0f;
                    float endPos = (n + scaleX) * (0.75f * (float)Screen.width - 25.0f) + 5.0f;

                    float placeHighlighted = (result.screenPosition.x - beginPos) / (endPos - beginPos);
                    if (placeHighlighted < 0)
                        placeHighlighted = 0;
                    else if (placeHighlighted > 1)
                        placeHighlighted = 1;

                    long ticks = fieldTeam.startTime.dateTime.Ticks +
                        (long)(placeHighlighted * (fieldTeam.endTime.dateTime.Ticks - fieldTeam.startTime.dateTime.Ticks));

                    DateTime timeHighlighted = new DateTime(ticks);

                    string imagePath = fieldTeam.GetPhotoThumbnailPathFromTime(timeHighlighted);

                    GameObject sceneUi = fieldTeam.fieldTeamsLogic.sceneUi;
                    
                    if (!_mapFrameDisplayShowing)
                    {
                        _mapFrameDisplay = Instantiate(fieldTeam.mapFrameDisplayPrefab, sceneUi.transform);
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

    public void HighlightPointOnTimeline()
    {
        ///
    }
}
