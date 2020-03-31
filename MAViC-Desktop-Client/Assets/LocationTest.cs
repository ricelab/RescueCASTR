using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationTest : MonoBehaviour
{
    public NetworkEvents networkEvents;
    
    // Start is called before the first frame update
    void Start()
    {
        networkEvents.AddHandler("LocationUpdate", updateLocation);
    }

    // Update is called once per frame
    void Update()
    {
        ///
    }

    private void updateLocation(string data)
    {
        Debug.Log(data);
    }
}
