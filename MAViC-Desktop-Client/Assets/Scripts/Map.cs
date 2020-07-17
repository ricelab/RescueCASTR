using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Map : MonoBehaviour
{
    //public NetworkEvents networkEvents;
    public GameObject mobileClientPrefab;

    public Location[] referenceLocations;
    public Vector3[] referenceMapPositions;
    
    // Start is called before the first frame update
    void Start()
    {
        //networkEvents.AddHandler("ConnectionRequest", AddNewMobileClient);
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
