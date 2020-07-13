using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPXparser;
using System.IO;
using System;
using System.Linq;
using UnityEngine.UI;

public class FieldTeam : MonoBehaviour
{
    #region Public Properties

    public enum FieldTeamAppearStatus
    {
        Showing,
        Dimmed,
        Hidden
    };

    public FieldTeamAppearStatus fieldTeamAppearStatus
    {
        get
        {
            return _fieldTeamAppearStatus;
        }
        set
        {
            _fieldTeamAppearStatus = value;

            if (_fieldTeamIsInstantiated)
            {
                LineRenderer pathLineRenderer = _teamPathLineObj.GetComponent<LineRenderer>();
                MeshRenderer currentLocationMeshRenderer = _currentLocationIndicator.GetComponent<MeshRenderer>();

                switch (value)
                {
                    case FieldTeamAppearStatus.Showing:
                        pathLineRenderer.enabled = true;
                        pathLineRenderer.startColor = new Color(
                            pathLineRenderer.startColor.r, pathLineRenderer.startColor.g, pathLineRenderer.startColor.b, 1.0f);
                        pathLineRenderer.endColor = new Color(
                            pathLineRenderer.endColor.r, pathLineRenderer.endColor.g, pathLineRenderer.endColor.b, 1.0f);

                        if (!isComplete)
                        {
                            currentLocationMeshRenderer.enabled = true;
                            currentLocationMeshRenderer.material.color = new Color(
                                currentLocationMeshRenderer.material.color.r,
                                currentLocationMeshRenderer.material.color.g,
                                currentLocationMeshRenderer.material.color.b,
                                1.0f);

                            DisplayOrUpdateCurrentLocationFrameDisplay();
                        }
                        else
                        {
                            HideCurrentLocationFrameDisplay();
                        }

                        break;

                    case FieldTeamAppearStatus.Dimmed:
                        pathLineRenderer.enabled = true;
                        pathLineRenderer.startColor = new Color(
                            pathLineRenderer.startColor.r, pathLineRenderer.startColor.g, pathLineRenderer.startColor.b, 0.15f);
                        pathLineRenderer.endColor = new Color(
                            pathLineRenderer.endColor.r, pathLineRenderer.endColor.g, pathLineRenderer.endColor.b, 0.15f);

                        if (!isComplete)
                        {
                            currentLocationMeshRenderer.enabled = false;
                            //currentLocationMeshRenderer.material.color = new Color(
                            //    currentLocationMeshRenderer.material.color.r,
                            //    currentLocationMeshRenderer.material.color.g,
                            //    currentLocationMeshRenderer.material.color.b,
                            //    0.25f);
                        }
                        HideCurrentLocationFrameDisplay();

                        break;

                    case FieldTeamAppearStatus.Hidden:
                        pathLineRenderer.enabled = false;
                        currentLocationMeshRenderer.enabled = false;
                        HideCurrentLocationFrameDisplay();

                        break;
                }
            }
        }
    }

    public MainController mainController;

    public GameObject teamIconPrefab;
    public GameObject teamTimelinePrefab;

    public GameObject mapFrameDisplayPrefab;
    public GameObject currentLocationFrameDisplayPrefab;

    public string teamName;
    public Color teamColor;
    public string recordingDirectoryPath;

    public UDateTime startTime;

    public UDateTime endTime
    {
        get
        {
            if (_endTime == null)
            {
                if (!_initialFileReadDone)
                {
                    PerformInitialFileRead();
                }

                DateTime dateTime = new DateTime(startTime.dateTime.Ticks + _actualEndTime.dateTime.Ticks - _actualStartTime.dateTime.Ticks);
                _endTime = dateTime;
            }
            return _endTime;
        }
    }

    public bool isComplete => UpdateRatioComplete() < 1.0 ? false : true;

    public Location currentLocation => _revealedMapLocations.Last();
    public Vector3 currentScenePosition => _revealedMapPositions.Last();

    #endregion


    #region Private Properties

    private FieldTeamAppearStatus _fieldTeamAppearStatus = FieldTeamAppearStatus.Showing;

    private bool _fieldTeamIsStarted = false;
    private bool _initialFileReadDone = false;
    private bool _fieldTeamIsInstantiated = false;

    private UDateTime _endTime = null;

    private UDateTime _actualStartTime;
    private UDateTime _actualEndTime;

    private float _ratioComplete = 0.0f;

    private GameObject _teamIconObj;
    private TeamIcon _teamIcon;

    private GameObject _teamTimelineObj;
    private TeamTimeline _teamTimeline;

    private string _gpsRecordingFilePath;
    private string _timelapsePhotoDirectoryPath;
    private string _timelapsePhotoThumbnailDirectoryPath;

    private GameObject _mapObj;
    private Map _map;

    private string[] _photoFileNames;
    private DateTime[] _photoTimes;

    private DateTime[] _gpsWaypointTimes;

    private List<Location> _mapLocations;
    private List<Location> _revealedMapLocations;

    private List<Vector3> _mapPositions;
    private List<Vector3> _revealedMapPositions;

    private List<GameObject> _teamPathPointObjs;
    private GameObject _teamPathLineObj;

    private int _latestAvailableWaypointIndex = 0;

    private GameObject _currentLocationIndicator;
    private GameObject _currentLocationFrameDisplayObj;
    private CurrentLocationFrameDisplay _currentLocationFrameDisplay;
    private bool _currentLocationFrameDisplayIsShowing = false;

    private Color _lastTeamColor;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        if (!_fieldTeamIsStarted)
        {
            if (!_initialFileReadDone)
            {
                PerformInitialFileRead();
            }

            mainController = this.gameObject.transform.parent.GetComponent<MainController>();
            _mapObj = mainController.mapObj;
            _map = _mapObj.GetComponent<Map>();

            _fieldTeamIsStarted = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_fieldTeamIsInstantiated)
        {
            // Update path colour if team's colour changed
            if (teamColor != _lastTeamColor)
            {
                _lastTeamColor = teamColor;

                LineRenderer lineRenderer = _teamPathLineObj.GetComponent<LineRenderer>();
                lineRenderer.startColor = new Color(0.5f * teamColor.r, 0.5f * teamColor.g, 0.5f * teamColor.b, teamColor.a);
                lineRenderer.endColor = teamColor;
            }

            // Add more points to revealed waypoints (if any)
            bool updateLatestPoint = false;
            for (int i = _latestAvailableWaypointIndex + 1; i < _teamPathPointObjs.Count; i++)
            {
                TeamPathPoint teamPathPoint = _teamPathPointObjs[i].GetComponent<TeamPathPoint>();

                if (ConvertActualTimeToTime(teamPathPoint.actualTime) < mainController.currentTime)
                {
                    updateLatestPoint = true;
                    _revealedMapPositions.Add(_mapPositions[i]);
                    _revealedMapLocations.Add(_mapLocations[i]);
                    _latestAvailableWaypointIndex = i;
                }
                else
                {
                    break;
                }
            }
            if (updateLatestPoint)
            {
                LineRenderer lineRenderer = _teamPathLineObj.GetComponent<LineRenderer>();
                lineRenderer.positionCount = _revealedMapPositions.Count;
                lineRenderer.SetPositions(_revealedMapPositions.ToArray());

                _currentLocationIndicator.transform.position = currentScenePosition;
            }

            UpdateRatioComplete();

            // If the team has just completed, move its icon to the 'completed' section
            if (isComplete && _teamIcon.transform.parent != mainController.completedTeamsPanel.transform)
            {
                _teamIconObj.transform.parent = mainController.completedTeamsPanel.transform;
                _teamIconObj.transform.SetSiblingIndex(0);
                _currentLocationIndicator.GetComponent<MeshRenderer>().enabled = false;
                HideCurrentLocationFrameDisplay();
            }

            if (_currentLocationFrameDisplayIsShowing)
            {
                DisplayOrUpdateCurrentLocationFrameDisplay();
            }
        }
    }

    public void FieldTeamInstantiate()
    {
        Start();
        
        _teamIconObj = Instantiate(teamIconPrefab,
            isComplete ? mainController.completedTeamsPanel.transform : mainController.currentlyDeployedTeamsPanel.transform);
        _teamIconObj.transform.SetSiblingIndex(0);
        _teamIcon = _teamIconObj.GetComponent<TeamIcon>();
        _teamIcon.fieldTeam = this;

        _teamTimelineObj = Instantiate(teamTimelinePrefab, mainController.timelineContentPanel.transform);
        _teamTimelineObj.transform.SetSiblingIndex(0);
        _teamTimeline = _teamTimelineObj.GetComponent<TeamTimeline>();
        _teamTimeline.fieldTeam = this;

        
        List<Track> tracks = Track.ReadTracksFromFile(_gpsRecordingFilePath);
        if (tracks.Count > 0)
        {
            /* Load photo frames */

            DirectoryInfo d = new DirectoryInfo(_timelapsePhotoDirectoryPath);
            FileInfo[] Files = d.GetFiles("*.JPG");

            _photoFileNames = new string[Files.Length];
            _photoTimes = new DateTime[Files.Length];

            int i = 0;
            foreach (FileInfo file in Files)
            {
                _photoFileNames[i] = file.Name;
                _photoTimes[i] = DateTime.ParseExact(Path.GetFileNameWithoutExtension(file.Name), "yyyy_MM_dd_HH_mm_ss", null);

                i++;
            }


            /* Load waypoints from GPS recording */

            // Only looking at the first track of the file (should only have one track)
            Track track = tracks[0];

            _mapLocations = new List<Location>(track.Waypoints.Count);
            _mapPositions = new List<Vector3>(track.Waypoints.Count);

            _teamPathPointObjs = new List<GameObject>(track.Waypoints.Count);
            _gpsWaypointTimes = new DateTime[track.Waypoints.Count];

            i = 0;
            foreach (Waypoint waypoint in track.Waypoints)
            {
                Location location = new Location();
                location.Latitude = waypoint.Latitude;
                location.Longitude = waypoint.Longitude;
                location.Altitude = waypoint.Elevation + 100;
                location.Accuracy = 0;
                location.AltitudeAccuracy = 0;
                location.Heading = 0;
                location.Speed = 0;

                _teamPathPointObjs.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
                _teamPathPointObjs.Last().transform.parent = _mapObj.transform;
                _teamPathPointObjs.Last().transform.position = _map.ConvertLocationToMapPosition(location);
                _teamPathPointObjs.Last().GetComponent<MeshRenderer>().enabled = false;

                TeamPathPoint teamPathPoint = _teamPathPointObjs.Last().AddComponent<TeamPathPoint>();
                teamPathPoint.location = location;
                teamPathPoint.actualTime = waypoint.Time;
                teamPathPoint.pointNumber = i;
                teamPathPoint.fieldTeam = this;

                if (ConvertActualTimeToTime(teamPathPoint.actualTime) < mainController.currentTime)
                {
                    _latestAvailableWaypointIndex = i;
                }

                _mapLocations.Add(location);
                _mapPositions.Add(_map.ConvertLocationToMapPosition(location));

                _gpsWaypointTimes[i] = waypoint.Time;

                i++;
            }

            // Get subset of points that are visible now (at the simulated current time)
            _revealedMapLocations = new List<Location>(_mapLocations.Take(_latestAvailableWaypointIndex + 1));
            _revealedMapPositions = new List<Vector3>(_mapPositions.Take(_latestAvailableWaypointIndex + 1));

            // Instantiate line
            _teamPathLineObj = new GameObject();
            _teamPathLineObj.transform.parent = _mapObj.transform;
            _teamPathLineObj.transform.SetSiblingIndex(0);
            LineRenderer lineRenderer = _teamPathLineObj.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));

            // Set positions (vertices) to display only the path that has been revealed at the current time
            lineRenderer.positionCount = _revealedMapPositions.Count;
            lineRenderer.SetPositions(_revealedMapPositions.ToArray());
            lineRenderer.startColor = new Color(teamColor.r, teamColor.g, teamColor.b, 0.5f);
            lineRenderer.endColor = teamColor;
            lineRenderer.startWidth = lineRenderer.endWidth = 1.0f;
            lineRenderer.numCornerVertices = 10;
            lineRenderer.numCapVertices = 5;

            // Instantiate current location indicator
            _currentLocationIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _currentLocationIndicator.transform.parent = _mapObj.transform;
            _currentLocationIndicator.transform.position = currentScenePosition;
            _currentLocationIndicator.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);
            _currentLocationIndicator.GetComponent<MeshRenderer>().material.color = teamColor;
            _currentLocationIndicator.GetComponent<MeshRenderer>().enabled = !isComplete;

            // Display current location frame display
            if (!isComplete)
            {
                DisplayOrUpdateCurrentLocationFrameDisplay();
            }
        }

        _fieldTeamIsInstantiated = true;
    }

    public void HighlightPathAtTime(DateTime time)
    {
        HighlightPathAtActualTime(ConvertTimeToActualTime(time));
    }

    public void HighlightPathAtActualTime(DateTime time)
    {
        int i = BinarySearchForClosestValue(_gpsWaypointTimes, time);
        _teamPathPointObjs[i].GetComponent<TeamPathPoint>().HighlightPathPoint();
    }

    public void UnhighlightPath()
    {
        foreach (GameObject teamPathPointObjs in _teamPathPointObjs)
        {
            TeamPathPoint teamPathPoint = teamPathPointObjs.GetComponent<TeamPathPoint>();
            teamPathPoint.UnhighlightPathPoint();
        }
    }

    public void HighlightActualTimeOnTimeline(DateTime timeHighlighted)
    {
        HighlightTimeOnTimeline(ConvertActualTimeToTime(timeHighlighted));
    }

    public void HighlightTimeOnTimeline(DateTime timeHighlighted)
    {
        _teamTimeline.HighlightTimeOnTimeline(timeHighlighted);
    }

    public void UnhighlightTimeline()
    {
        _teamTimeline.UnhighlightTimeline();
    }

    public string GetPhotoPathFromTime(DateTime time)
    {
        return GetPhotoPathFromActualTime(ConvertTimeToActualTime(time));
    }

    public string GetPhotoThumbnailPathFromTime(DateTime time)
    {
        return GetPhotoThumbnailPathFromActualTime(ConvertTimeToActualTime(time));
    }

    public string GetPhotoPathFromActualTime(DateTime time)
    {
        int i = BinarySearchForClosestValue(_photoTimes.ToArray(), time);
        return _timelapsePhotoDirectoryPath + _photoFileNames[i];
    }

    public string GetPhotoThumbnailPathFromActualTime(DateTime time)
    {
        int i = BinarySearchForClosestValue(_photoTimes.ToArray(), time);
        return _timelapsePhotoThumbnailDirectoryPath + _photoFileNames[i];
    }

    public void ShowThisFieldTeamOnly()
    {
        foreach (Transform t in mainController.transform)
        {
            FieldTeam ft = t.gameObject.GetComponent<FieldTeam>();
            if (ft != null && ft.isActiveAndEnabled)
            {
                ft.fieldTeamAppearStatus = FieldTeamAppearStatus.Dimmed;
            }
        }
        fieldTeamAppearStatus = FieldTeamAppearStatus.Showing;
    }

    public void ShowAllFieldTeams()
    {
        foreach (Transform t in mainController.transform)
        {
            FieldTeam ft = t.gameObject.GetComponent<FieldTeam>();
            if (ft != null && ft.isActiveAndEnabled)
            {
                ft.fieldTeamAppearStatus = FieldTeamAppearStatus.Showing;
            }
        }
    }

    /// <summary>
    /// To determine whether the whole path is in scene camera view
    /// </summary>
    /// <param name="margin">When margin is 0, then it returns true when the line is exactly at the camera edge.</param>
    /// <returns></returns>
    public bool IsPathInCameraView(float margin = 0.0f)
    {
        Camera camera = mainController.sceneCamera.GetComponent<Camera>();
        foreach (GameObject g in _teamPathPointObjs)
        {
            Vector3 viewportPoint = camera.WorldToViewportPoint(g.transform.position);
            if(viewportPoint.x > 1 - margin || viewportPoint.x < 0 + margin || viewportPoint.y > 1 - margin || viewportPoint.y < 0 + margin)
            {
                return false;
            }
        }

        return true;
    }

    public DateTime ConvertTimeToActualTime(DateTime time)
    {
        long ticksFromStart = time.Ticks - startTime.dateTime.Ticks;
        return new DateTime(_actualStartTime.dateTime.Ticks + ticksFromStart);
    }

    public DateTime ConvertActualTimeToTime(DateTime actualTime)
    {
        long ticksFromStart = actualTime.Ticks - _actualStartTime.dateTime.Ticks;
        return new DateTime(startTime.dateTime.Ticks + ticksFromStart);
    }

    private void PerformInitialFileRead()
    {
        if (!recordingDirectoryPath.EndsWith("/"))
        {
            recordingDirectoryPath += "/";
        }

        _gpsRecordingFilePath = recordingDirectoryPath + "gps-record.gpx";
        _timelapsePhotoDirectoryPath = recordingDirectoryPath + "photos/";
        _timelapsePhotoThumbnailDirectoryPath = recordingDirectoryPath + "photo-thumbnails/";

        List<Track> tracks = Track.ReadTracksFromFile(_gpsRecordingFilePath);
        _actualStartTime = tracks[0].Waypoints.First().Time;
        _actualEndTime = tracks[0].Waypoints.Last().Time;

        _initialFileReadDone = true;
    }

    private float UpdateRatioComplete()
    {
        if (_fieldTeamIsStarted)
        {
            if (mainController.currentTime.dateTime >= _endTime.dateTime)
            {
                _ratioComplete = 1.0f;
            }
            else if (mainController.currentTime.dateTime <= startTime.dateTime)
            {
                _ratioComplete = 0.0f;
            }
            else
            {
                _ratioComplete = (float)(mainController.currentTime.dateTime.Ticks - startTime.dateTime.Ticks) /
                    (float)(_endTime.dateTime.Ticks - startTime.dateTime.Ticks);
            }
        }

        return _ratioComplete;
    }

    private void DisplayOrUpdateCurrentLocationFrameDisplay()
    {
        if (!_currentLocationFrameDisplayIsShowing)
        {
            _currentLocationFrameDisplayObj = GameObject.Instantiate(currentLocationFrameDisplayPrefab, mainController.sceneUiObj.transform);
            _currentLocationFrameDisplayObj.transform.SetAsFirstSibling();
            _currentLocationFrameDisplayObj.transform.Find("Background").GetComponent<Image>().color = teamColor;
            _currentLocationFrameDisplayObj.transform.Find("Arrow").GetComponent<Image>().color = teamColor;

            _currentLocationFrameDisplay = _currentLocationFrameDisplayObj.GetComponent<CurrentLocationFrameDisplay>();
            _currentLocationFrameDisplay.SetTeamName(teamName);

            _currentLocationFrameDisplayIsShowing = true;
        }

        Camera sceneCamera = mainController.sceneCamera.GetComponent<Camera>();
        RectTransform canvasRect = mainController.sceneUiObj.GetComponent<RectTransform>();
        Vector2 viewportPos = sceneCamera.WorldToViewportPoint(currentScenePosition);
        Vector2 worldObjScreenPos = new Vector2(
            ((viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
            ((viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f))
        );
        _currentLocationFrameDisplayObj.GetComponent<RectTransform>().anchoredPosition = worldObjScreenPos;

        _currentLocationFrameDisplay.DisplayImage(GetPhotoThumbnailPathFromTime(mainController.currentTime));

        // Display on side UI
        if (mainController.sideUi.selectedFieldTeam == this)
        {
            mainController.sideUi.DisplayFieldTeamLiveImage(GetPhotoThumbnailPathFromTime(mainController.currentTime));
        }
    }

    private void HideCurrentLocationFrameDisplay()
    {
        if (_currentLocationFrameDisplayIsShowing)
        {
            GameObject.Destroy(_currentLocationFrameDisplayObj);
            _currentLocationFrameDisplayIsShowing = false;
        }
    }

    private int BinarySearchForClosestValue(DateTime[] a, DateTime item)
    {
        int first = 0;
        int last = a.Length - 1;
        int mid;
        do
        {
            mid = first + (last - first) / 2;
            if (item.CompareTo(a[mid]) > 0)
            {
                first = mid + 1;
            }
            else
            {
                last = mid - 1;
            }
            if (a[mid].CompareTo(item) == 0)
            {
                return mid;
            }
        } while (first <= last);
        
        return mid;
    }
}
