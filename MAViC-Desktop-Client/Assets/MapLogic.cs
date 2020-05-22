using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPXparser;

public class MapLogic : MonoBehaviour
{
    public NetworkEvents networkEvents;
    public GameObject mobileClientPrefab;

    public string gpsRecordingFilename;

    public Location[] referenceLocations;
    public Vector3[] referenceMapPositions;
    
    // Start is called before the first frame update
    void Start()
    {
        networkEvents.AddHandler("ConnectionRequest", AddNewMobileClient);

        List<Track> tracks = Track.ReadTracksFromFile(gpsRecordingFilename);
        foreach (var track in tracks)
        {
            //Console.WriteLine(track.Name + ": ");
            //Console.WriteLine(track.Statistics.ToString());
            //Console.WriteLine();

            Vector3[] mapPositions = new Vector3[track.Waypoints.Count];
            int i = 0;

            foreach (Waypoint waypoint in track.Waypoints)
            {
                //Debug.Log(waypoint.ToString());
                
                Location location = new Location();
                location.Latitude = waypoint.Latitude;
                location.Longitude = waypoint.Longitude;
                location.Altitude = waypoint.Elevation + 300;
                location.Accuracy = 0;
                location.AltitudeAccuracy = 0;
                location.Heading = 0;
                location.Speed = 0;

                //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //sphere.transform.parent = this.transform;
                //sphere.transform.position = ConvertLocationToMapPosition(location);

                mapPositions[i++] = ConvertLocationToMapPosition(location);
            }

            GameObject line = new GameObject();
            line.transform.parent = this.transform;
            line.AddComponent<LineRenderer>();
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            lineRenderer.positionCount = track.Waypoints.Count;
            lineRenderer.SetPositions(mapPositions);
            lineRenderer.startColor = lineRenderer.endColor = new Color(0.5f, 0.0f, 0.0f, 1.0f);
            lineRenderer.startWidth = lineRenderer.endWidth = 1.0f;
            lineRenderer.SetPositions(mapPositions);
            lineRenderer.numCornerVertices = 10;
            lineRenderer.numCapVertices = 5;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ///
    }

    public Vector3 ConvertLocationToMapPosition(Location location)
    {
        float x = (float)(
            (location.Longitude - referenceLocations[0].Longitude) *
            (referenceMapPositions[0].x - referenceMapPositions[1].x) / (referenceLocations[0].Longitude - referenceLocations[1].Longitude) +
            referenceMapPositions[0].x
        );

        float y = (float)(
            (location.Altitude - referenceLocations[0].Altitude) *
            (referenceMapPositions[0].y - referenceMapPositions[1].y) / (referenceLocations[0].Altitude - referenceLocations[1].Altitude) +
            referenceMapPositions[0].y
        );

        float z = (float)(
            (location.Latitude - referenceLocations[0].Latitude) *
            (referenceMapPositions[0].z - referenceMapPositions[1].z) / (referenceLocations[0].Latitude - referenceLocations[1].Latitude) +
            referenceMapPositions[0].z
        );

        return new Vector3(x, y, z);
    }

    private void AddNewMobileClient(string clientID)
    {
        Debug.Log("AddNewMobileClient: " + clientID);

        GameObject newMobileClientObj = Instantiate(mobileClientPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        newMobileClientObj.transform.parent = this.transform;
        MobileClient newMobileClient = newMobileClientObj.GetComponent<MobileClient>();
        newMobileClient.clientID = clientID;
        newMobileClient.StartCall();
    }
}
