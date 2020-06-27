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

    public FieldTeamsLogic fieldTeamsLogic;

    public GameObject teamIconPrefab;
    public GameObject teamTimelinePrefab;

    public GameObject mapFrameDisplayPrefab;

    public string teamName;
    public Color teamColor;
    public string recordingDirectoryPath;

    public double ratioComplete;

    public UDateTime startTime;

    public UDateTime endTime
    {
        get
        {
            if (_endTime == null)
            {
                List<Track> tracks = Track.ReadTracksFromFile(_gpsRecordingFilePath);
                DateTime dateTime = new DateTime(startTime.dateTime.Ticks + tracks[0].Waypoints.Last().Time.Ticks - tracks[0].Waypoints.First().Time.Ticks);
                _endTime = dateTime;
            }
            return _endTime;
        }
    }

    #endregion


    #region Private Properties

    private bool _fieldTeamIsInstantiated = false;

    private UDateTime _endTime = null;

    private TeamIcon _teamIcon;
    private TeamTimelineLogic _teamTimelineLogic;

    private string _gpsRecordingFilePath;
    private string _timelapsePhotoDirectoryPath;
    private string _timelapsePhotoThumbnailDirectoryPath;

    private GameObject _map;
    private MapLogic _mapLogic;

    private string[] _photoFileNames;
    private DateTime[] _photoTimes;

    private GameObject[] _teamPathPoints;
    private GameObject _teamPathLine;

    private Color _lastTeamColor;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        if (!recordingDirectoryPath.EndsWith("/"))
        {
            recordingDirectoryPath += "/";
        }

        _gpsRecordingFilePath = recordingDirectoryPath + "gps-record.gpx";
        _timelapsePhotoDirectoryPath = recordingDirectoryPath + "photos/";
        _timelapsePhotoThumbnailDirectoryPath = recordingDirectoryPath + "photo-thumbnails/";

        fieldTeamsLogic = this.gameObject.transform.parent.GetComponent<FieldTeamsLogic>();
        _map = fieldTeamsLogic.map;
        _mapLogic = _map.GetComponent<MapLogic>();
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

                LineRenderer lineRenderer = _teamPathLine.GetComponent<LineRenderer>();
                lineRenderer.startColor = lineRenderer.endColor = teamColor;
            }
        }
    }

    public void FieldTeamInstantiate()
    {
        GameObject newTeamIconObj = Instantiate(teamIconPrefab,
            ratioComplete < 1.0 ? fieldTeamsLogic.currentlyDeployedTeamsPanel.transform : fieldTeamsLogic.completedTeamsPanel.transform);
        newTeamIconObj.transform.SetSiblingIndex(0);
        _teamIcon = newTeamIconObj.GetComponent<TeamIcon>();
        _teamIcon.fieldTeam = this;

        GameObject newTeamTimelineObj = Instantiate(teamTimelinePrefab, fieldTeamsLogic.timelineContentPanel.transform);
        newTeamTimelineObj.transform.SetSiblingIndex(0);
        _teamTimelineLogic = newTeamTimelineObj.GetComponent<TeamTimelineLogic>();
        _teamTimelineLogic.fieldTeam = this;

        
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

            _teamPathPoints = new GameObject[track.Waypoints.Count];

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

                _teamPathPoints[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _teamPathPoints[i].transform.parent = _map.transform;
                _teamPathPoints[i].transform.position = _mapLogic.ConvertLocationToMapPosition(location);
                _teamPathPoints[i].GetComponent<MeshRenderer>().enabled = false;
                _teamPathPoints[i].transform.localScale.Scale(new Vector3(10, 10, 10));

                TeamPathPointLogic teamPathPointLogic = _teamPathPoints[i].AddComponent<TeamPathPointLogic>();
                teamPathPointLogic.location = location;
                teamPathPointLogic.time = waypoint.Time;
                teamPathPointLogic.pointNumber = i;
                teamPathPointLogic.fieldTeam = this;

                mapPositions[i] = _mapLogic.ConvertLocationToMapPosition(location);

                i++;
            }

            _teamPathLine = new GameObject();
            _teamPathLine.transform.parent = _map.transform;
            _teamPathLine.transform.SetSiblingIndex(0);
            _teamPathLine.AddComponent<LineRenderer>();
            LineRenderer lineRenderer = _teamPathLine.GetComponent<LineRenderer>();
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


    public string GetPhotoPathFromTime(DateTime time)
    {
        int i = BinarySearchForClosestValue(_photoTimes, time);
        return _timelapsePhotoDirectoryPath + _photoFileNames[i];
    }

    public string GetPhotoThumbnailPathFromTime(DateTime time)
    {
        int i = BinarySearchForClosestValue(_photoTimes, time);
        return _timelapsePhotoThumbnailDirectoryPath + _photoFileNames[i];
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
