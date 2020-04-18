using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLogic : MonoBehaviour
{
    public NetworkEvents networkEvents;
    public GameObject mobileClientPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        networkEvents.AddHandler("ConnectionRequest", AddNewMobileClient);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddNewMobileClient(string clientID)
    {
        Debug.Log("AddNewMobileClient: " + clientID);

        GameObject newMobileClientObj = Instantiate(mobileClientPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        MobileClient newMobileClient = newMobileClientObj.GetComponent<MobileClient>();
        newMobileClient.clientID = clientID;
        newMobileClient.StartCall();
    }
}
