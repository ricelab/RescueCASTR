using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPXparser;
using System.IO;
using System;
using System.Linq;

public class FieldTeam : MonoBehaviour
{
    #region Public Properties

    public FieldTeamsGroup fieldTeamsGroup;

    public GameObject teamIconPrefab;
    public GameObject teamTimelinePrefab;

    public GameObject mapFrameDisplayPrefab;

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

    #endregion


    #region Private Properties

    private bool _fieldTeamIsStarted = false;
    private bool _initialFileReadDone = false;
    private bool _fieldTeamIsInstantiated = false;

    private UDateTime _endTime = null;

    private UDateTime _actualStartTime;
    private UDateTime _actualEndTime;

    private double _ratioComplete = 0.0f;

    private TeamIcon _teamIcon;
    private TeamTimeline _teamTimeline;

    private string _gpsRecordingFilePath;
    private string _timelapsePhotoDirectoryPath;
    private string _timelapsePhotoThumbnailDirectoryPath;

    private GameObject _mapObj;
    private Map _mapLogic;

    private string[] _photoFileNames;
    private DateTime[] _photoTimes;

    private DateTime[] _gpsWaypointTimes;

    private GameObject[] _teamPathPointObjs;
    private GameObject _teamPathLineObj;

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

            fieldTeamsGroup = this.gameObject.transform.parent.GetComponent<FieldTeamsGroup>();
            _mapObj = fieldTeamsGroup.map;
            _mapLogic = _mapObj.GetComponent<Map>();

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
                lineRenderer.startColor = lineRenderer.endColor = teamColor;
            }

            UpdateRatioComplete();
        }
    }


    public void FieldTeamInstantiate()
    {
        Start();
        
        GameObject newTeamIconObj = Instantiate(teamIconPrefab,
            _ratioComplete < 1.0 ? fieldTeamsGroup.currentlyDeployedTeamsPanel.transform : fieldTeamsGroup.completedTeamsPanel.transform);
        newTeamIconObj.transform.SetSiblingIndex(0);
        _teamIcon = newTeamIconObj.GetComponent<TeamIcon>();
        _teamIcon.fieldTeam = this;

        GameObject newTeamTimelineObj = Instantiate(teamTimelinePrefab, fieldTeamsGroup.timelineContentPanel.transform);
        newTeamTimelineObj.transform.SetSiblingIndex(0);
        _teamTimeline = newTeamTimelineObj.GetComponent<TeamTimeline>();
        _teamTimeline.fieldTeam = this;

        
        List<Track> tracks = Track.ReadTracksFromFile(_gpsRecordingFilePath);
        foreach (var track in tracks)
        {
            // Load photo frames

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


            // Load waypoints from GPS recording

            Vector3[] mapPositions = new Vector3[track.Waypoints.Count];

            _teamPathPointObjs = new GameObject[track.Waypoints.Count];
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

                _teamPathPointObjs[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _teamPathPointObjs[i].transform.parent = _mapObj.transform;
                _teamPathPointObjs[i].transform.position = _mapLogic.ConvertLocationToMapPosition(location);
                _teamPathPointObjs[i].GetComponent<MeshRenderer>().enabled = false;
                _teamPathPointObjs[i].transform.localScale.Scale(new Vector3(10, 10, 10));

                TeamPathPoint teamPathPointLogic = _teamPathPointObjs[i].AddComponent<TeamPathPoint>();
                teamPathPointLogic.location = location;
                teamPathPointLogic.actualTime = waypoint.Time;
                teamPathPointLogic.pointNumber = i;
                teamPathPointLogic.fieldTeam = this;

                mapPositions[i] = _mapLogic.ConvertLocationToMapPosition(location);

                _gpsWaypointTimes[i] = waypoint.Time;

                i++;
            }

            _teamPathLineObj = new GameObject();
            _teamPathLineObj.transform.parent = _mapObj.transform;
            _teamPathLineObj.transform.SetSiblingIndex(0);
            _teamPathLineObj.AddComponent<LineRenderer>();
            LineRenderer lineRenderer = _teamPathLineObj.GetComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            lineRenderer.positionCount = track.Waypoints.Count;
            lineRenderer.SetPositions(mapPositions);
            lineRenderer.startColor = lineRenderer.endColor = teamColor;
            lineRenderer.startWidth = lineRenderer.endWidth = 1.0f;
            lineRenderer.SetPositions(mapPositions);
            lineRenderer.numCornerVertices = 10;
            lineRenderer.numCapVertices = 5;
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
        foreach (GameObject teamPathPoint in _teamPathPointObjs)
        {
            TeamPathPoint teamPathPointLogic = teamPathPoint.GetComponent<TeamPathPoint>();
            teamPathPointLogic.UnhighlightPathPoint();
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
        int i = BinarySearchForClosestValue(_photoTimes, time);
        return _timelapsePhotoDirectoryPath + _photoFileNames[i];
    }

    public string GetPhotoThumbnailPathFromActualTime(DateTime time)
    {
        int i = BinarySearchForClosestValue(_photoTimes, time);
        return _timelapsePhotoThumbnailDirectoryPath + _photoFileNames[i];
    }

    public void ShowThisFieldTeamOnly()
    {
        foreach (Transform t in fieldTeamsGroup.transform)
        {
            t.gameObject.GetComponent<FieldTeam>()._teamPathLineObj.SetActive(false);
        }
        _teamPathLineObj.SetActive(true);
    }

    public void ShowAllFieldTeams()
    {
        foreach (Transform t in fieldTeamsGroup.transform)
        {
            t.gameObject.GetComponent<FieldTeam>()._teamPathLineObj.SetActive(true);
        }
    }

    /// <summary>
    /// To determine whether the whole path is in scene camera view
    /// </summary>
    /// <param name="margin">When margin is 0, then it returns true when the line is exactly at the camera edge.</param>
    /// <returns></returns>
    public bool IsPathInCameraView(float margin = 0.0f)
    {
        Camera camera = fieldTeamsGroup.sceneCamera.GetComponent<Camera>();
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

    private void UpdateRatioComplete()
    {
        if (_fieldTeamIsStarted)
        {
            if (fieldTeamsGroup.currentTime.dateTime >= _endTime.dateTime)
            {
                _ratioComplete = 1.0f;
            }
            else if (fieldTeamsGroup.currentTime.dateTime <= startTime.dateTime)
            {
                _ratioComplete = 0.0f;
            }
            else
            {
                _ratioComplete = (float)(fieldTeamsGroup.currentTime.dateTime.Ticks - startTime.dateTime.Ticks) /
                    (float)(_endTime.dateTime.Ticks - startTime.dateTime.Ticks);
            }
        }
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
