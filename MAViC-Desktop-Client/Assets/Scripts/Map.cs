//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
using UnityEngine;

public class Map : MonoBehaviour
{
    //public NetworkEvents networkEvents;
    //public GameObject mobileClientPrefab;

    public Location[] referenceLocations;
    public Vector3[] referenceMapPositions;

    // Camera position constraints
    public CameraDefaultsAndConstraints cameraDefaultsAndConstraints;

    // Amount to add to terrain height
    public float addToTerrainHeight = 5.0f;

    public void Start()
    {
        //networkEvents.AddHandler("ConnectionRequest", AddNewMobileClient);
    }

    public Vector3 ConvertLocationToMapPosition(Location location)
    {
        float x = (float)(
            (location.longitude - referenceLocations[0].longitude) *
            (referenceMapPositions[0].x - referenceMapPositions[1].x) / (referenceLocations[0].longitude - referenceLocations[1].longitude) +
            referenceMapPositions[0].x
        );

        float y = (float)(
            (location.altitude - referenceLocations[0].altitude) *
            (referenceMapPositions[0].y - referenceMapPositions[1].y) / (referenceLocations[0].altitude - referenceLocations[1].altitude) +
            referenceMapPositions[0].y
        );

        float z = (float)(
            (location.latitude - referenceLocations[0].latitude) *
            (referenceMapPositions[0].z - referenceMapPositions[1].z) / (referenceLocations[0].latitude - referenceLocations[1].latitude) +
            referenceMapPositions[0].z
        );

        Vector3 mapPos = new Vector3(x, y, z);

        return new Vector3(mapPos.x,
            TerrainHeightAtPosition(mapPos),
            mapPos.z);
    }

    public float TerrainHeightAtPosition(Vector3 pos)
    {
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Terrain");
        if (Physics.Raycast(new Vector3(pos.x, 0.0f, pos.z), Vector3.down, out hit, float.MaxValue, mask))
        {
            return hit.point.y + addToTerrainHeight;
        }
        else
        {
            return pos.y + addToTerrainHeight;
        }
    }

    //private void AddNewMobileClient(string clientID)
    //{
    //    Debug.Log("AddNewMobileClient: " + clientID);

    //    GameObject newMobileClientObj = Instantiate(mobileClientPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    //    newMobileClientObj.transform.parent = this.transform;
    //    //MobileClient newMobileClient = newMobileClientObj.GetComponent<MobileClient>();
    //    //newMobileClient.clientID = clientID;
    //    //newMobileClient.StartCall();
    //}
}
