using System.Collections.Generic;
using UnityEngine;
using GPXparser;
using System.IO;
using System;
using System.Linq;
using UnityEngine.UI;

[Serializable]
public class FieldTeamJson
{
    public string name;
    public string color;
    public string path;
    public string simulatedStartTime;
    public RadioDeadZoneJson[] radioDeadZones;
}

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
    public GameObject deployedTeamIconPrefab;
    public GameObject teamTimelinePrefab;

    public GameObject mapFrameDisplayPrefab;
    public GameObject currentLocationFrameDisplayPrefab;

    public GameObject messageMapIconPrefab;
    public GameObject clueMapIconPrefab;

    public GameObject messageTimelineIconPrefab;
    public GameObject clueTimelineIconPrefab;

    public string teamName;
    public Color teamColor;
    public string recordingDirectoryPath;

    public UDateTime simulatedStartTime;

    public UDateTime simulatedEndTime
    {
        get
        {
            if (_simulatedEndTime == null)
            {
                if (!_initialFileReadDone)
                {
                    PerformInitialFileRead();
                }

                DateTime dateTime = new DateTime(simulatedStartTime.dateTime.Ticks + _actualEndTime.dateTime.Ticks - _actualStartTime.dateTime.Ticks);
                _simulatedEndTime = dateTime;
            }
            return _simulatedEndTime;
        }
    }

    public bool isComplete => UpdateRatioComplete() < 1.0 ? false : true;

    public Communication[] prerecordedCommunications;
    public Message[] prerecordedMessages;
    public Clue[] prerecordedClues;

    public List<Communication> revealedCommunications;
    public List<Message> revealedMessages;
    public List<Clue> revealedClues;

    public RadioDeadZone[] radioDeadZones;

    public bool isInRadioDeadZone
    {
        get
        {
            if (!isComplete && radioDeadZones != null)
            {
                foreach (RadioDeadZone radioDeadZone in radioDeadZones)
                {
                    if (mainController.currentSimulatedTime.dateTime >= radioDeadZone.simulatedStartTime.dateTime &&
                        mainController.currentSimulatedTime.dateTime <= radioDeadZone.simulatedEndTime.dateTime)
                    {
                        _lastSimulatedTimeBeforeOffline = radioDeadZone.simulatedStartTime;
                        _lastActualTimeBeforeOffline = radioDeadZone.actualStartTime;

                        return true;
                    }
                }
            }

            return false;
        }
    }

    public UDateTime simulatedTimeLastOnline
    {
        get
        {
            if (isInRadioDeadZone)
            {
                return _lastSimulatedTimeBeforeOffline;
            }
            else
            {
                return isComplete ? simulatedEndTime : mainController.currentSimulatedTime;
            }
        }
    }

    public UDateTime actualTimeLastOnline
    {
        get
        {
            if (isInRadioDeadZone)
            {
                return _lastActualTimeBeforeOffline;
            }
            else
            {
                return isComplete ? _actualEndTime : (UDateTime)ConvertSimulatedTimeToActualTime(mainController.currentSimulatedTime);
            }
        }
    }

    public Location currentLocation => _revealedMapLocations.Last();
    public Vector3 currentScenePosition => _revealedMapPositions.Last();

    public Location predictedCurrentLocation
    {
        get
        {
            if (isInRadioDeadZone && _revealedPredictedRouteMapPositions != null)
            {
                return _revealedPredictedRouteMapLocations.Last();
            }
            else
            {
                return currentLocation;
            }
        }
    }

    public Vector3 predictedCurrentScenePosition
    {
        get
        {
            if (isInRadioDeadZone && _revealedPredictedRouteMapPositions != null)
            {
                return _revealedPredictedRouteMapPositions.Last();
            }
            else
            {
                return currentScenePosition;
            }
        }
    }

    public TeamTimeline teamTimeline;

    public bool showExtraDetails = false;

    #endregion


    #region Private Properties

    private FieldTeamAppearStatus _fieldTeamAppearStatus = FieldTeamAppearStatus.Showing;

    private bool _fieldTeamIsStarted = false;
    private bool _initialFileReadDone = false;
    private bool _fieldTeamIsInstantiated = false;

    private UDateTime _simulatedEndTime = null;

    private UDateTime _actualStartTime;
    private UDateTime _actualEndTime;

    private float _ratioComplete = 0.0f;

    private GameObject _teamIconObj;
    private TeamIcon _teamIcon;

    private GameObject _teamTimelineObj;

    private string _recordingResourcesUrl;

    private string _gpsRecordingFilePath;
    private string _assignedRouteFilePath;
    private string _radioDeadZonesFilePath;
    private string _messagesFilePath;
    private string _cluesFilePath;
    private string _timelapsePhotoDirectoryPath;
    private string _timelapsePhotoThumbnailDirectoryPath;
    private string _timelapsePhotoThumbnailGrayscaleDirectoryPath;
    private string _photosFileNamesListPath;

    private GameObject _mapObj;
    private Map _map;

    private string[] _photoFileNames;
    private DateTime[] _photoTimes;

    private DateTime[] _gpsWaypointTimes;

    private List<Location> _mapLocations;
    private List<Location> _revealedMapLocations;

    private List<Vector3> _mapPositions;
    private List<Vector3> _revealedMapPositions;

    private List<Location> _assignedRouteMapLocations;
    private List<Vector3> _assignedRouteMapPositions;
    private List<Waypoint> _assignedRouteWaypoints;

    private List<Location> _predictedRouteMapLocations;
    private List<Location> _revealedPredictedRouteMapLocations;

    private List<Vector3> _predictedRouteMapPositions;
    private List<Vector3> _revealedPredictedRouteMapPositions;

    private List<Waypoint> _predictedRouteWaypoints;
    private List<Waypoint> _revealedPredictedRouteWaypoints;

    private float _revealedPredictedRouteCumulativeDistance;

    /// <summary>
    /// Average speed, in m/s.
    /// </summary>
    private float _averageSpeed;

    private List<GameObject> _teamPathPointObjs;
    private GameObject _teamPathLineObj;

    private GameObject _teamAssignedRouteLineObj;
    private float _teamAssignedRouteLineTotalDistance;

    private GameObject _teamPredictedRouteLineObj;
    private float _teamPredictedRouteLineTotalDistance;

    private List<GameObject> _clueMapIconObjs;
    private List<GameObject> _messageMapIconObjs;

    private List<GameObject> _clueTimelineIconObjs;
    private List<GameObject> _messageTimelineIconObjs;

    private UDateTime _lastSimulatedTimeBeforeOffline;
    private UDateTime _lastActualTimeBeforeOffline;

    private int _latestAvailableWaypointIndex = 0;

    private int _latestAvailablePredictedRouteWaypointIndex = 0;

    private int _latestAvailableCommunicationIndex = -1;
    private int _latestAvailableMessageIndex = -1;
    private int _latestAvailableClueIndex = -1;

    private GameObject _currentLocationIndicator;
    private GameObject _currentLocationFrameDisplayObj;
    private CurrentLocationFrameDisplay _currentLocationFrameDisplay;
    private bool _currentLocationFrameDisplayIsShowing = false;

    private Color _lastTeamColor;

    private bool _mapPositionsDroppedToTerrain = false;

    #endregion


    #region Public Methods

    public void Start()
    {
        if (!_fieldTeamIsStarted)
        {
            mainController = this.gameObject.transform.parent.GetComponent<MainController>();
            _mapObj = mainController.mapObj;
            _map = mainController.map;

            if (!_initialFileReadDone)
            {
                PerformInitialFileRead();
            }

            _fieldTeamIsStarted = true;
        }
    }

    public void FieldTeamInstantiate()
    {
        Start();

        _teamIconObj = Instantiate(isComplete ? teamIconPrefab : deployedTeamIconPrefab,
            isComplete ? mainController.completedTeamsPanel.transform : mainController.currentlyDeployedTeamsPanel.transform);
        _teamIconObj.transform.SetSiblingIndex(0);
        _teamIcon = _teamIconObj.GetComponent<TeamIcon>();
        _teamIcon.fieldTeam = this;

        _teamTimelineObj = Instantiate(teamTimelinePrefab, mainController.timelineContentPanel.transform);
        _teamTimelineObj.transform.SetSiblingIndex(1);
        teamTimeline = _teamTimelineObj.GetComponent<TeamTimeline>();
        teamTimeline.fieldTeam = this;


        List<Track> tracks = Track.ReadTracksFromFile(_gpsRecordingFilePath);
        if (tracks.Count > 0)
        {
            /* Load photo frames */

            TextAsset photoFileNames = Resources.Load<TextAsset>(_photosFileNamesListPath);
            _photoFileNames = photoFileNames.text.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );
            //_photoFileNames.Take(_photoFileNames.Count() - 1).ToArray();

            _photoTimes = new DateTime[_photoFileNames.Length];

            int i = 0;
            foreach (string photoFileName in _photoFileNames)
            {
                _photoTimes[i] = DateTime.ParseExact(Path.GetFileNameWithoutExtension(photoFileName), "yyyy_MM_dd_HH_mm_ss", null);

                i++;
            }


            /* Load/setup radio dead zones */

            TextAsset radioDeadZonesJsonFile = Resources.Load<TextAsset>(_radioDeadZonesFilePath);
            RadioDeadZoneJson[] radioDeadZoneJsonArray = JsonHelper.FromJson<RadioDeadZoneJson>(JsonHelper.fixJson(radioDeadZonesJsonFile.text));
            radioDeadZones = new RadioDeadZone[radioDeadZoneJsonArray.Length];
            for (int n = 0; n < radioDeadZones.Length; n++)
            {
                radioDeadZones[n] = new RadioDeadZone();

                if (radioDeadZoneJsonArray[n].instantiateBySimulatedTime)
                {
                    radioDeadZones[n].simulatedStartTime = Convert.ToDateTime(radioDeadZoneJsonArray[n].startTime);
                    radioDeadZones[n].simulatedEndTime = Convert.ToDateTime(radioDeadZoneJsonArray[n].endTime);

                    radioDeadZones[n].actualStartTime = ConvertSimulatedTimeToActualTime(radioDeadZones[n].simulatedStartTime);
                    radioDeadZones[n].actualEndTime = ConvertSimulatedTimeToActualTime(radioDeadZones[n].simulatedEndTime);
                }
                else
                {
                    radioDeadZones[n].actualStartTime = Convert.ToDateTime(radioDeadZoneJsonArray[n].startTime);
                    radioDeadZones[n].actualEndTime = Convert.ToDateTime(radioDeadZoneJsonArray[n].endTime);

                    radioDeadZones[n].simulatedStartTime = ConvertActualTimeToSimulatedTime(radioDeadZones[n].actualStartTime);
                    radioDeadZones[n].simulatedEndTime = ConvertActualTimeToSimulatedTime(radioDeadZones[n].actualEndTime);
                }
            }


            /* Load waypoints from GPS recording */

            // Only looking at the first track of the file (should only have one track)
            Track track = tracks[0];

            _mapLocations = new List<Location>(track.Waypoints.Count);
            _mapPositions = new List<Vector3>(track.Waypoints.Count);

            _teamPathPointObjs = new List<GameObject>(track.Waypoints.Count);
            _gpsWaypointTimes = new DateTime[track.Waypoints.Count];

            _averageSpeed = (float)track.Statistics.AverageSpeedInMotion * 5.0f / 38.0f;

            i = 0;
            foreach (Waypoint waypoint in track.Waypoints)
            {
                Location location = new Location();
                location.latitude = waypoint.Latitude;
                location.longitude = waypoint.Longitude;
                location.altitude = waypoint.Elevation /* + 75 */;
                location.accuracy = 0;
                location.altitudeAccuracy = 0;
                location.heading = 0;
                location.speed = 0;

                _teamPathPointObjs.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
                _teamPathPointObjs.Last().transform.parent = _mapObj.transform;
                _teamPathPointObjs.Last().transform.position = _map.ConvertLocationToMapPosition(location);
                _teamPathPointObjs.Last().GetComponent<MeshRenderer>().enabled = false;

                TeamPathPoint teamPathPoint = _teamPathPointObjs.Last().AddComponent<TeamPathPoint>();
                teamPathPoint.location = location;
                teamPathPoint.actualTime = waypoint.Time;
                teamPathPoint.pointNumber = i;
                teamPathPoint.fieldTeam = this;

                if (ConvertActualTimeToSimulatedTime(teamPathPoint.actualTime) < simulatedTimeLastOnline)
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
            lineRenderer.startColor = new Color(0.5f * teamColor.r, 0.5f * teamColor.g, 0.5f * teamColor.b, teamColor.a);
            lineRenderer.endColor = teamColor;
            lineRenderer.numCornerVertices = 10;
            lineRenderer.numCapVertices = 5;

            // Instantiate current location indicator
            _currentLocationIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _currentLocationIndicator.transform.parent = _mapObj.transform;
            _currentLocationIndicator.transform.position = currentScenePosition;
            _currentLocationIndicator.GetComponent<MeshRenderer>().material.color = teamColor;
            _currentLocationIndicator.GetComponent<MeshRenderer>().enabled = !isComplete;

            // Display current location frame display
            if (!isComplete)
            {
                DisplayOrUpdateCurrentLocationFrameDisplay();
            }
        }

        // Setup assigned route path on map
        List<Track> assignedRouteTracks = Track.ReadTracksFromFile(_assignedRouteFilePath);
        if (assignedRouteTracks.Count > 0)
        {
            /* Load waypoints from GPS recording */

            // Only looking at the first track of the file (should only have one track)
            Track track = assignedRouteTracks[0];

            _assignedRouteMapLocations = new List<Location>(track.Waypoints.Count);
            _assignedRouteMapPositions = new List<Vector3>(track.Waypoints.Count);

            _assignedRouteWaypoints = track.Waypoints;

            foreach (Waypoint waypoint in track.Waypoints)
            {
                Location location = new Location();
                location.latitude = waypoint.Latitude;
                location.longitude = waypoint.Longitude;
                location.altitude = (double.IsNaN(waypoint.Elevation) ? 0 : waypoint.Elevation) /* + 75 */;
                location.accuracy = 0;
                location.altitudeAccuracy = 0;
                location.heading = 0;
                location.speed = 0;

                _assignedRouteMapLocations.Add(location);
                _assignedRouteMapPositions.Add(_map.ConvertLocationToMapPosition(location));
            }

            // Instantiate assigned route line
            _teamAssignedRouteLineObj = new GameObject();
            _teamAssignedRouteLineObj.transform.parent = _mapObj.transform;
            _teamAssignedRouteLineObj.transform.SetSiblingIndex(0);
            LineRenderer lineRenderer = _teamAssignedRouteLineObj.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Unlit/DottedLine" /* "Legacy Shaders/Particles/Alpha Blended" */));
            lineRenderer.positionCount = track.Waypoints.Count;
            lineRenderer.SetPositions(_assignedRouteMapPositions.ToArray());
            lineRenderer.startColor = lineRenderer.endColor = new Color(1.0f, 1.0f /* 0.85f */, 1.0f /* 0.0f */, 0.75f);
            lineRenderer.numCornerVertices = 10;
            lineRenderer.numCapVertices = 2;
            _teamAssignedRouteLineObj.SetActive(false);
            //DottedLineRenderer dottedLineRenderer = _teamAssignedRouteLineObj.AddComponent<DottedLineRenderer>();
            //dottedLineRenderer.scaleInUpdate = true;

            // Get assigned route line total distance
            int c0 = 0;
            int c1 = 1;
            _teamAssignedRouteLineTotalDistance = 0.0f;
            while (c1 < lineRenderer.positionCount)
            {
                _teamAssignedRouteLineTotalDistance += Vector3.Distance(lineRenderer.GetPosition(c0), lineRenderer.GetPosition(c1));
                c0++;
                c1++;
            }
        }

        // Setup messages
        TextAsset messagesJsonFile = Resources.Load<TextAsset>(_messagesFilePath);
        MessageJson[] messageJsonArray = JsonHelper.FromJson<MessageJson>(JsonHelper.fixJson(messagesJsonFile.text));
        prerecordedMessages = new Message[messageJsonArray.Length];
        int c = 0;
        foreach (MessageJson messageJson in messageJsonArray)
        {
            prerecordedMessages[c] = new Message();

            if (messageJson.instantiateBySimulatedTime)
                prerecordedMessages[c].simulatedTime = Convert.ToDateTime(messageJson.time);
            else
                prerecordedMessages[c].actualTime = Convert.ToDateTime(messageJson.time);

            prerecordedMessages[c].instantiateBySimulatedTime = messageJson.instantiateBySimulatedTime;
            prerecordedMessages[c].messageDirection = messageJson.direction;
            prerecordedMessages[c].messageContent = messageJson.content;

            prerecordedMessages[c].fieldTeam = this;
            if (prerecordedMessages[c].instantiateBySimulatedTime)
                prerecordedMessages[c].location = GetLocationAtSimulatedTime(prerecordedMessages[c].simulatedTime);
            else
                prerecordedMessages[c].location = GetLocationAtActualTime(prerecordedMessages[c].actualTime);

            // Start the Message if it hasn't been started
            prerecordedMessages[c].Start();

            c++;
        }
        Array.Sort(prerecordedMessages);

        // Setup revealed messages
        revealedMessages = new List<Message>();
        _messageMapIconObjs = new List<GameObject>();
        _messageTimelineIconObjs = new List<GameObject>();

        // Setup clues
        TextAsset cluesJsonFile = Resources.Load<TextAsset>(_cluesFilePath);
        ClueJson[] clueJsonArray = JsonHelper.FromJson<ClueJson>(JsonHelper.fixJson(cluesJsonFile.text));
        prerecordedClues = new Clue[clueJsonArray.Length];
        c = 0;
        foreach (ClueJson clueJson in clueJsonArray)
        {
            prerecordedClues[c] = new Clue();

            if (clueJson.instantiateBySimulatedTime)
                prerecordedClues[c].simulatedTime = Convert.ToDateTime(clueJson.time);
            else
                prerecordedClues[c].actualTime = Convert.ToDateTime(clueJson.time);

            prerecordedClues[c].instantiateBySimulatedTime = clueJson.instantiateBySimulatedTime;
            prerecordedClues[c].photoFileName = clueJson.photoFileName;
            prerecordedClues[c].textDescription = clueJson.textDescription;

            prerecordedClues[c].fieldTeam = this;
            if (prerecordedClues[c].instantiateBySimulatedTime)
                prerecordedClues[c].location = GetLocationAtSimulatedTime(prerecordedClues[c].simulatedTime);
            else
                prerecordedClues[c].location = GetLocationAtActualTime(prerecordedClues[c].actualTime);

            // Start the Clue if it hasn't been started
            prerecordedClues[c].Start();

            c++;
        }
        Array.Sort(prerecordedClues);

        // Setup revealed clues
        revealedClues = new List<Clue>();
        _clueMapIconObjs = new List<GameObject>();
        _clueTimelineIconObjs = new List<GameObject>();

        // Setup Communications list
        prerecordedCommunications = new Communication[prerecordedMessages.Length + prerecordedClues.Length];
        c = 0;
        foreach(Communication msg in prerecordedMessages)
        {
            prerecordedCommunications[c] = msg;
            c++;
        }
        foreach (Communication clue in prerecordedClues)
        {
            prerecordedCommunications[c] = clue;
            c++;
        }
        Array.Sort(prerecordedCommunications);

        // Setup revealed Communications list
        revealedCommunications = new List<Communication>();

        _fieldTeamIsInstantiated = true;
    }

    public void Update()
    {
        if (_fieldTeamIsInstantiated)
        {
            LineRenderer lineRenderer = _teamPathLineObj.GetComponent<LineRenderer>();
            LineRenderer assignedRouteLineRenderer = _teamAssignedRouteLineObj.GetComponent<LineRenderer>();

            // Show assigned path if needed
            if (showExtraDetails)
            {
                _teamAssignedRouteLineObj.SetActive(true);
            }
            else
            {
                _teamAssignedRouteLineObj.SetActive(false);
            }

            // Update path colour if team's colour changed
            if (teamColor != _lastTeamColor)
            {
                _lastTeamColor = teamColor;

                lineRenderer.startColor = new Color(0.5f * teamColor.r, 0.5f * teamColor.g, 0.5f * teamColor.b, teamColor.a);
                lineRenderer.endColor = teamColor;
            }

            // Update size of path
            if (mainController.sceneCameraControls.cameraViewingMode == CameraControls.CameraViewingMode._2D)
            {
                lineRenderer.startWidth = lineRenderer.endWidth = 1.0f / 50.0f * mainController.sceneCamera.orthographicSize;
                assignedRouteLineRenderer.startWidth = assignedRouteLineRenderer.endWidth = 1.0f / 50.0f * mainController.sceneCamera.orthographicSize;

                assignedRouteLineRenderer.material.SetFloat("_RepeatCount",
                    mainController.map.cameraDefaultsAndConstraints.maximumOrthographicSize /
                    mainController.sceneCamera.orthographicSize *
                    25.0f * _teamAssignedRouteLineTotalDistance / mainController.map.cameraDefaultsAndConstraints.maximumOrthographicSize /
                    assignedRouteLineRenderer.widthMultiplier
                    );
            }
            else // if (mainController.sceneCameraControls.cameraViewingMode == CameraControls.CameraViewingMode._3D)
            {
                float width = 1.0f / 50.0f * (mainController.sceneCameraObj.transform.position.y - mainController.map.cameraDefaultsAndConstraints.minimumY);
                if (width < 0.1f)
                    width = 0.1f;

                lineRenderer.startWidth = lineRenderer.endWidth = width;
                assignedRouteLineRenderer.startWidth = assignedRouteLineRenderer.endWidth = width;

                assignedRouteLineRenderer.material.SetFloat("_RepeatCount",
                    mainController.map.cameraDefaultsAndConstraints.maximumY / 5.0f /
                        (mainController.sceneCameraObj.transform.position.y - mainController.map.cameraDefaultsAndConstraints.minimumY) *
                    0.5f * _teamAssignedRouteLineTotalDistance / assignedRouteLineRenderer.widthMultiplier
                    );
            }

            // Update size of current location indicator
            float scaleFactor = 4.0f;
            if (mainController.sceneCameraControls.cameraViewingMode == CameraControls.CameraViewingMode._2D)
            {
                scaleFactor = scaleFactor / 50.0f * mainController.sceneCamera.orthographicSize;
            }
            else // if (mainController.sceneCameraControls.cameraViewingMode == CameraControls.CameraViewingMode._3D)
            {
                scaleFactor = scaleFactor / 50.0f *
                    (mainController.sceneCameraObj.transform.position.y - mainController.map.cameraDefaultsAndConstraints.minimumY);
            }
            _currentLocationIndicator.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            // If entering a radio dead zone
            if (isInRadioDeadZone && _predictedRouteMapPositions == null)
            {
                // Find position on assigned route closest to team's last known position
                int closestPositionIndex = 0;
                float closestDistance = float.MaxValue;
                for (int i = 0; i < _assignedRouteMapPositions.Count; i++)
                {
                    float distanceToPosition = Vector3.Distance(_assignedRouteMapPositions.ElementAt(i), currentScenePosition);
                    if (distanceToPosition < closestDistance)
                    {
                        closestDistance = distanceToPosition;
                        closestPositionIndex = i;
                    }
                }

                // Create predicted route path

                _predictedRouteMapPositions = new List<Vector3>(_assignedRouteMapPositions.Count - closestPositionIndex + 1);
                _predictedRouteMapLocations = new List<Location>(_assignedRouteMapPositions.Count - closestPositionIndex + 1);
                _predictedRouteWaypoints = new List<Waypoint>(_assignedRouteMapPositions.Count - closestPositionIndex + 1);

                _predictedRouteMapPositions.Add(currentScenePosition);
                _predictedRouteMapLocations.Add(currentLocation);

                _predictedRouteWaypoints.Add(new Waypoint());
                _predictedRouteWaypoints[0].Latitude = currentLocation.latitude;
                _predictedRouteWaypoints[0].Longitude = currentLocation.longitude;
                _predictedRouteWaypoints[0].Elevation = currentLocation.altitude;
                _predictedRouteWaypoints[0].Speed = currentLocation.speed;

                for (int i = 0; i < /* _predictedRouteMapPositions.Count */ (_assignedRouteMapPositions.Count - closestPositionIndex + 1) - 1; i++)
                {
                    _predictedRouteMapPositions.Add(_assignedRouteMapPositions[closestPositionIndex + i]);
                    _predictedRouteMapLocations.Add(_assignedRouteMapLocations[closestPositionIndex + i]);
                    _predictedRouteWaypoints.Add(_assignedRouteWaypoints[closestPositionIndex + i]);
                }

                _latestAvailablePredictedRouteWaypointIndex = 0;
                _revealedPredictedRouteCumulativeDistance = 0.0f;

                _revealedPredictedRouteMapPositions = new List<Vector3>();
                _revealedPredictedRouteMapLocations = new List<Location>();
                _revealedPredictedRouteWaypoints = new List<Waypoint>();

                _revealedPredictedRouteMapPositions.Add(_predictedRouteMapPositions[0]);
                _revealedPredictedRouteMapLocations.Add(_predictedRouteMapLocations[0]);
                _revealedPredictedRouteWaypoints.Add(_predictedRouteWaypoints[0]);

                // Instantiate predicted route line
                _teamPredictedRouteLineObj = new GameObject();
                _teamPredictedRouteLineObj.transform.parent = _mapObj.transform;
                _teamPredictedRouteLineObj.transform.SetSiblingIndex(0);
                LineRenderer predictedRouteLineRenderer = _teamPredictedRouteLineObj.AddComponent<LineRenderer>();
                predictedRouteLineRenderer.material = new Material(Shader.Find("Unlit/DottedLine" /* "Legacy Shaders/Particles/Alpha Blended" */));
                predictedRouteLineRenderer.positionCount = _revealedPredictedRouteMapPositions.Count;
                predictedRouteLineRenderer.SetPositions(_revealedPredictedRouteMapPositions.ToArray());
                predictedRouteLineRenderer.startColor = predictedRouteLineRenderer.endColor = teamColor;
                predictedRouteLineRenderer.numCornerVertices = 10;
                predictedRouteLineRenderer.numCapVertices = 2;
            }

            // If leaving a radio dead zone
            else if (!isInRadioDeadZone && _predictedRouteMapPositions != null)
            {
                _predictedRouteMapPositions = null;
                _predictedRouteMapLocations = null;
                _predictedRouteWaypoints = null;
                GameObject.Destroy(_teamPredictedRouteLineObj);
            }

            // Add more points to revealed waypoints (if any)
            bool updateLatestPoint = false;
            for (int i = _latestAvailableWaypointIndex + 1; i < _teamPathPointObjs.Count; i++)
            {
                TeamPathPoint teamPathPoint = _teamPathPointObjs[i].GetComponent<TeamPathPoint>();

                if (ConvertActualTimeToSimulatedTime(teamPathPoint.actualTime) < simulatedTimeLastOnline)
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
                lineRenderer.positionCount = _revealedMapPositions.Count;
                lineRenderer.SetPositions(_revealedMapPositions.ToArray());

                _currentLocationIndicator.transform.position = currentScenePosition;
            }

            UpdateRatioComplete();

            // If the team is in a radio dead zone
            if (isInRadioDeadZone)
            {
                LineRenderer predictedRouteLineRenderer = _teamPredictedRouteLineObj.GetComponent<LineRenderer>();

                float secondsPastSinceOffline = (float)(new TimeSpan(mainController.currentSimulatedTime.dateTime.Ticks - simulatedTimeLastOnline.dateTime.Ticks)).TotalSeconds;
                float predictedDistanceCoveredSinceOffline = _averageSpeed * secondsPastSinceOffline;

                // Reveal more points from predicted route (if needed)
                bool updateLatestPredictedPoint = false;
                for (int i = _latestAvailablePredictedRouteWaypointIndex; i < _predictedRouteWaypoints.Count - 1; i++)
                {
                    double altChangeBetweenPoints;

                    float distanceBetweenPoints = (float)Waypoint.ComputeDistance(
                        _predictedRouteWaypoints[i],
                        _predictedRouteWaypoints[i + 1],
                        out altChangeBetweenPoints);

                    float altitudeChangeBetweenPoints = (float)altChangeBetweenPoints;

                    if (_revealedPredictedRouteCumulativeDistance + distanceBetweenPoints <= predictedDistanceCoveredSinceOffline)
                    {
                        updateLatestPredictedPoint = true;

                        _revealedPredictedRouteMapPositions.Add(_predictedRouteMapPositions[i + 1]);
                        _revealedPredictedRouteMapLocations.Add(_predictedRouteMapLocations[i + 1]);
                        _revealedPredictedRouteWaypoints.Add(_predictedRouteWaypoints[i + 1]);

                        _revealedPredictedRouteCumulativeDistance += distanceBetweenPoints;
                        _latestAvailablePredictedRouteWaypointIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }
                if (updateLatestPredictedPoint)
                {
                    predictedRouteLineRenderer.positionCount = _revealedPredictedRouteMapPositions.Count;
                    predictedRouteLineRenderer.SetPositions(_revealedPredictedRouteMapPositions.ToArray());

                    _currentLocationIndicator.transform.position = predictedCurrentScenePosition;

                    // Update predicted route line total distance (TODO: Make more efficient later)
                    int c0 = 0;
                    int c1 = 1;
                    _teamPredictedRouteLineTotalDistance = 0.0f;
                    while (c1 < predictedRouteLineRenderer.positionCount)
                    {
                        _teamPredictedRouteLineTotalDistance += Vector3.Distance(predictedRouteLineRenderer.GetPosition(c0), predictedRouteLineRenderer.GetPosition(c1));
                        c0++;
                        c1++;
                    }
                }

                // Update size of predicted path
                if (mainController.sceneCameraControls.cameraViewingMode == CameraControls.CameraViewingMode._2D)
                {
                    predictedRouteLineRenderer.startWidth = predictedRouteLineRenderer.endWidth = 1.0f / 30.0f * mainController.sceneCamera.orthographicSize;

                    predictedRouteLineRenderer.material.SetFloat("_RepeatCount",
                        mainController.map.cameraDefaultsAndConstraints.maximumOrthographicSize /
                        mainController.sceneCamera.orthographicSize *
                        25.0f * _teamPredictedRouteLineTotalDistance / mainController.map.cameraDefaultsAndConstraints.maximumOrthographicSize /
                        predictedRouteLineRenderer.widthMultiplier
                        );
                }
                else // if (mainController.sceneCameraControls.cameraViewingMode == CameraControls.CameraViewingMode._3D)
                {
                    float width = 1.0f / 30.0f * (mainController.sceneCameraObj.transform.position.y - mainController.map.cameraDefaultsAndConstraints.minimumY);
                    if (width < 0.1f)
                        width = 0.1f;

                    predictedRouteLineRenderer.startWidth = predictedRouteLineRenderer.endWidth = width;

                    predictedRouteLineRenderer.material.SetFloat("_RepeatCount",
                        mainController.map.cameraDefaultsAndConstraints.maximumY / 5.0f /
                        (mainController.sceneCameraObj.transform.position.y - mainController.map.cameraDefaultsAndConstraints.minimumY) *
                        0.5f * _teamPredictedRouteLineTotalDistance / predictedRouteLineRenderer.widthMultiplier
                        );
                }
            }

            // If the team has just completed, move its icon to the 'completed' section
            if (isComplete && _teamIcon.transform.parent != mainController.completedTeamsPanel.transform)
            {
                GameObject.Destroy(_teamIconObj);
                _teamIconObj = Instantiate(teamIconPrefab, mainController.completedTeamsPanel.transform);
                _teamIconObj.transform.SetSiblingIndex(0);
                _teamIcon = _teamIconObj.GetComponent<TeamIcon>();
                _teamIcon.fieldTeam = this;
                _currentLocationIndicator.GetComponent<MeshRenderer>().enabled = false;
                HideCurrentLocationFrameDisplay();
            }

            if (_currentLocationFrameDisplayIsShowing)
            {
                DisplayOrUpdateCurrentLocationFrameDisplay();
            }

            // Add more messages to revealed messages (if any)
            if (prerecordedMessages != null && prerecordedMessages.Length > 0)
            {
                for (int i = _latestAvailableMessageIndex + 1; i < prerecordedMessages.Length; i++)
                {
                    if (prerecordedMessages[i].simulatedTime.dateTime < simulatedTimeLastOnline.dateTime)
                    {
                        revealedMessages.Add(prerecordedMessages[i]);
                        _latestAvailableMessageIndex++;

                        // Add message icon to map
                        GameObject messageMapIconObj = GameObject.Instantiate(messageMapIconPrefab, mainController.sceneUiObj.transform);
                        messageMapIconObj.transform.SetSiblingIndex(0);
                        messageMapIconObj.GetComponent<MessageMapIcon>().message = prerecordedMessages[i];
                        _messageMapIconObjs.Add(messageMapIconObj);

                        // Add message icon to timeline
                        GameObject messageTimelineIconObj = GameObject.Instantiate(messageTimelineIconPrefab, mainController.timelineUiObj.transform);
                        //messageTimelineIconObj.transform.SetSiblingIndex(0);
                        messageTimelineIconObj.GetComponent<MessageTimelineIcon>().message = prerecordedMessages[i];
                        _messageTimelineIconObjs.Add(messageTimelineIconObj);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Add more clues to revealed clues (if any)
            if (prerecordedClues != null && prerecordedClues.Length > 0)
            {
                for (int i = _latestAvailableClueIndex + 1; i < prerecordedClues.Length; i++)
                {
                    if (prerecordedClues[i].simulatedTime.dateTime < simulatedTimeLastOnline.dateTime)
                    {
                        revealedClues.Add(prerecordedClues[i]);
                        _latestAvailableClueIndex++;

                        // Add clue icon to map
                        GameObject clueMapIconObj = GameObject.Instantiate(clueMapIconPrefab, mainController.sceneUiObj.transform);
                        clueMapIconObj.transform.SetSiblingIndex(0);
                        clueMapIconObj.GetComponent<ClueMapIcon>().clue = prerecordedClues[i];
                        _clueMapIconObjs.Add(clueMapIconObj);

                        // Add clue icon to timeline
                        GameObject clueTimelineIconObj = GameObject.Instantiate(clueTimelineIconPrefab, mainController.timelineUiObj.transform);
                        //clueTimelineIconObj.transform.SetSiblingIndex(0);
                        clueTimelineIconObj.GetComponent<ClueTimelineIcon>().clue = prerecordedClues[i];
                        _clueTimelineIconObjs.Add(clueTimelineIconObj);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Add more Communications to revealed Communications (if any)
            if (prerecordedCommunications != null && prerecordedCommunications.Length > 0)
            {
                for (int i = _latestAvailableCommunicationIndex + 1; i < prerecordedCommunications.Length; i++)
                {
                    if (prerecordedCommunications[i].simulatedTime.dateTime < simulatedTimeLastOnline.dateTime)
                    {
                        revealedCommunications.Add(prerecordedCommunications[i]);
                        _latestAvailableCommunicationIndex++;

                        // If Communications page showing, add new communications box
                        if (mainController.sideUi.currentlyActivePage == SideUi.CurrentlyActivePage.Communications && mainController.sideUi.selectedFieldTeam == this)
                        {
                            mainController.sideUi.communicationsPage.AddCommunicationBox(prerecordedCommunications[i]);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    public void FixedUpdate()
    {
        if (!_mapPositionsDroppedToTerrain && mainController.isStarted)
        {
            for (int i = 0; i < _mapPositions.Count; i++)
            {
                _mapPositions[i] = new Vector3(
                    _mapPositions[i].x,
                    TerrainHeightAtPosition(_mapPositions[i]),
                    _mapPositions[i].z
                    );
            }

            for (int i = 0; i < _revealedMapPositions.Count; i++)
            {
                _revealedMapPositions[i] = new Vector3(
                    _revealedMapPositions[i].x,
                    TerrainHeightAtPosition(_revealedMapPositions[i]),
                    _revealedMapPositions[i].z
                    );
            }

            for (int i = 0; i < _assignedRouteMapPositions.Count; i++)
            {
                _assignedRouteMapPositions[i] = new Vector3(
                    _assignedRouteMapPositions[i].x,
                    TerrainHeightAtPosition(_assignedRouteMapPositions[i]),
                    _assignedRouteMapPositions[i].z
                    );
            }

            if (_predictedRouteMapPositions != null)
            {
                for (int i = 0; i < _predictedRouteMapPositions.Count; i++)
                {
                    _predictedRouteMapPositions[i] = new Vector3(
                        _predictedRouteMapPositions[i].x,
                        TerrainHeightAtPosition(_predictedRouteMapPositions[i]),
                        _predictedRouteMapPositions[i].z
                        );
                }
            }

            if (_revealedPredictedRouteMapPositions != null)
            {
                for (int i = 0; i < _revealedPredictedRouteMapPositions.Count; i++)
                {
                    _revealedPredictedRouteMapPositions[i] = new Vector3(
                        _revealedPredictedRouteMapPositions[i].x,
                        TerrainHeightAtPosition(_revealedPredictedRouteMapPositions[i]),
                        _revealedPredictedRouteMapPositions[i].z
                        );
                }
            }

            LineRenderer routeLineRenderer = _teamPathLineObj.GetComponent<LineRenderer>();
            routeLineRenderer.SetPositions(_revealedMapPositions.ToArray());

            LineRenderer assignedRouteLineRenderer = _teamAssignedRouteLineObj.GetComponent<LineRenderer>();
            assignedRouteLineRenderer.SetPositions(_assignedRouteMapPositions.ToArray());

            if (_teamPredictedRouteLineObj != null)
            {
                LineRenderer predictedRouteLineRenderer = _teamPredictedRouteLineObj.GetComponent<LineRenderer>();
                predictedRouteLineRenderer.SetPositions(_revealedPredictedRouteMapPositions.ToArray());
            }

            _mapPositionsDroppedToTerrain = true;
        }
    }

    public Location GetLocationAtSimulatedTime(DateTime simulatedTime)
    {
        return GetLocationAtActualTime(ConvertSimulatedTimeToActualTime(simulatedTime));
    }

    public Location GetLocationAtActualTime(DateTime actualTime)
    {
        int i = BinarySearchForClosestValue(_gpsWaypointTimes, actualTime);
        return _mapLocations[i];
    }

    public Vector3 GetScenePositionAtSimulatedTime(DateTime simulatedTime)
    {
        return GetScenePositionAtActualTime(ConvertSimulatedTimeToActualTime(simulatedTime));
    }

    public Vector3 GetScenePositionAtActualTime(DateTime actualTime)
    {
        int i = BinarySearchForClosestValue(_gpsWaypointTimes, actualTime);
        return _mapPositions[i];
    }

    public void HighlightPathAtSimulatedTime(DateTime simulatedTime)
    {
        HighlightPathAtActualTime(ConvertSimulatedTimeToActualTime(simulatedTime));
    }

    public void HighlightPathAtActualTime(DateTime actualTime)
    {
        int i = BinarySearchForClosestValue(_gpsWaypointTimes, actualTime);
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

    public void HighlightSimulatedTimeOnTimeline(DateTime simulatedTime)
    {
        teamTimeline.HighlightTimeOnTimeline(simulatedTime);
    }

    public void HighlightActualTimeOnTimeline(DateTime actualTime)
    {
        HighlightSimulatedTimeOnTimeline(ConvertActualTimeToSimulatedTime(actualTime));
    }

    public void UnhighlightTimeline()
    {
        teamTimeline.UnhighlightTimeline();
    }

    public string GetPhotoPathFromSimulatedTime(DateTime simulatedTime)
    {
        return GetPhotoPathFromActualTime(ConvertSimulatedTimeToActualTime(simulatedTime));
    }

    public string GetPhotoThumbnailPathFromSimulatedTime(DateTime simulatedTime)
    {
        return GetPhotoThumbnailPathFromActualTime(ConvertSimulatedTimeToActualTime(simulatedTime));
    }

    public string GetGrayscalePhotoThumbnailPathFromSimulatedTime(DateTime simulatedTime)
    {
        return GetGrayscalePhotoThumbnailPathFromActualTime(ConvertSimulatedTimeToActualTime(simulatedTime));
    }

    public string GetPhotoPathFromActualTime(DateTime actualTime)
    {
        int i = BinarySearchForClosestValue(_photoTimes.ToArray(), actualTime);
        return _timelapsePhotoDirectoryPath + _photoFileNames[i];
    }

    public string GetPhotoThumbnailPathFromActualTime(DateTime actualTime)
    {
        int i = BinarySearchForClosestValue(_photoTimes.ToArray(), actualTime);
        return _timelapsePhotoThumbnailDirectoryPath + _photoFileNames[i];
    }

    public string GetGrayscalePhotoThumbnailPathFromActualTime(DateTime actualTime)
    {
        int i = BinarySearchForClosestValue(_photoTimes.ToArray(), actualTime);
        return _timelapsePhotoThumbnailGrayscaleDirectoryPath + _photoFileNames[i];
    }

    public DateTime ConvertSimulatedTimeToActualTime(DateTime simulatedTime)
    {
        long ticksFromStart = simulatedTime.Ticks - simulatedStartTime.dateTime.Ticks;
        return new DateTime(_actualStartTime.dateTime.Ticks + ticksFromStart);
    }

    public DateTime ConvertActualTimeToSimulatedTime(DateTime actualTime)
    {
        long ticksFromStart = actualTime.Ticks - _actualStartTime.dateTime.Ticks;
        return new DateTime(simulatedStartTime.dateTime.Ticks + ticksFromStart);
    }

    public void ShowThisFieldTeamOnly(bool showFootageThumbnail = false)
    {
        foreach (Transform t in mainController.transform)
        {
            FieldTeam ft = t.gameObject.GetComponent<FieldTeam>();
            if (ft != null && ft.isActiveAndEnabled)
            {
                ft.fieldTeamAppearStatus = FieldTeamAppearStatus.Dimmed;
            }
        }
        this.showExtraDetails = showFootageThumbnail;
        fieldTeamAppearStatus = FieldTeamAppearStatus.Showing;
    }

    public void ShowAllFieldTeams(bool showFootageThumbnails = false)
    {
        mainController.ShowAllFieldTeams(showFootageThumbnails);
    }

    /// <summary>
    /// To determine whether the whole path is in scene camera view
    /// </summary>
    /// <param name="margin">When margin is 0, then it returns true when the line is exactly at the camera edge.</param>
    /// <returns></returns>
    public bool IsPathInCameraView(float margin = 0.0f)
    {
        Camera camera = mainController.sceneCameraObj.GetComponent<Camera>();
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

    #endregion


    #region Private Methods

    private void PerformInitialFileRead()
    {
        if (!recordingDirectoryPath.EndsWith("/"))
        {
            recordingDirectoryPath += "/";
        }

        _recordingResourcesUrl = mainController.resourcesUrl + recordingDirectoryPath;

        _gpsRecordingFilePath = /* _recordingResourcesUrl */ recordingDirectoryPath + "gps-record.gpx";
        _assignedRouteFilePath = /* _recordingResourcesUrl */ recordingDirectoryPath + "assigned-route.gpx";
        _radioDeadZonesFilePath = recordingDirectoryPath + "radio-dead-zones";
        _messagesFilePath = /* _recordingResourcesUrl */ recordingDirectoryPath + "messages";
        _cluesFilePath = /* _recordingResourcesUrl */ recordingDirectoryPath + "clues";
        _timelapsePhotoDirectoryPath = /* _recordingResourcesUrl */ mainController.resourcesUrl + "photos/";
        _timelapsePhotoThumbnailDirectoryPath = /* _recordingResourcesUrl */ /* mainController.resourcesUrl */ /* recordingDirectoryPath + */ "photo-thumbnails/";
        _timelapsePhotoThumbnailGrayscaleDirectoryPath = /* _recordingResourcesUrl */ mainController.resourcesUrl + "photo-thumbnails-grayscale/";
        _photosFileNamesListPath = /* _recordingResourcesUrl */ recordingDirectoryPath + "photos-filenames";

        List<Track> tracks = Track.ReadTracksFromFile(_gpsRecordingFilePath);
        _actualStartTime = tracks[0].Waypoints.First().Time;
        _actualEndTime = tracks[0].Waypoints.Last().Time;

        _initialFileReadDone = true;
    }

    private float UpdateRatioComplete()
    {
        if (_fieldTeamIsStarted)
        {
            if (mainController.currentSimulatedTime.dateTime >= _simulatedEndTime.dateTime)
            {
                _ratioComplete = 1.0f;
            }
            else if (mainController.currentSimulatedTime.dateTime <= simulatedStartTime.dateTime)
            {
                _ratioComplete = 0.0f;
            }
            else
            {
                _ratioComplete = (float)(mainController.currentSimulatedTime.dateTime.Ticks - simulatedStartTime.dateTime.Ticks) /
                    (float)(_simulatedEndTime.dateTime.Ticks - simulatedStartTime.dateTime.Ticks);
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
            _currentLocationFrameDisplay.fieldTeam = this;

            _currentLocationFrameDisplayIsShowing = true;
        }

        _currentLocationFrameDisplay.SetTeamName(teamName + (isInRadioDeadZone ? "\n(Predicted Position)" : ""));

        // Show or hide footage thumbnail
        if (showExtraDetails && !isInRadioDeadZone)
        {
            _currentLocationFrameDisplay.ShowThumbnail();
            _currentLocationFrameDisplayObj.transform.SetSiblingIndex(0);
        }
        else
            _currentLocationFrameDisplay.HideThumbnail();

        Camera sceneCamera = mainController.sceneCameraObj.GetComponent<Camera>();
        RectTransform canvasRect = mainController.sceneUiObj.GetComponent<RectTransform>();
        Vector3 viewportPos = sceneCamera.WorldToViewportPoint(predictedCurrentScenePosition);
        Vector2 worldObjScreenPos = new Vector2(
            ((viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
            ((viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f))
        );
        _currentLocationFrameDisplayObj.GetComponent<RectTransform>().anchoredPosition = worldObjScreenPos;
        if (viewportPos.z < 0.0f)
        {
            _currentLocationFrameDisplayObj.SetActive(false);
        }
        else
        {
            _currentLocationFrameDisplayObj.SetActive(true);
        }

        _currentLocationFrameDisplay.DisplayImage(GetPhotoThumbnailPathFromSimulatedTime(simulatedTimeLastOnline));

        // Display on side UI
        if (mainController.sideUi.selectedFieldTeam == this)
        {
            mainController.sideUi.DisplayFieldTeamLiveImage(
                GetPhotoPathFromSimulatedTime(simulatedTimeLastOnline),
                GetPhotoThumbnailPathFromSimulatedTime(simulatedTimeLastOnline),
                GetGrayscalePhotoThumbnailPathFromSimulatedTime(simulatedTimeLastOnline)
                );
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

    private float TerrainHeightAtPosition(Vector3 pos)
    {
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Terrain");
        if (Physics.Raycast(new Vector3(pos.x, 0.0f, pos.z), Vector3.down, out hit, float.MaxValue, mask))
        {
            return hit.point.y + 5;
        }
        else
        {
            return pos.y + 5;
        }
    }

    #endregion
}
