using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamPathPointLogic : MonoBehaviour
{
    public Location location;
    public DateTime time;
    public int pointNumber;
    public FieldTeam fieldTeam;

    private GameObject _mapFrameDisplay;
    private MapFrameDisplayLogic _mapFrameDisplayLogic;

    // Start is called before the first frame update
    void Start()
    {
        ///
    }

    // Update is called once per frame
    void Update()
    {
        ///
    }

    void OnMouseEnter()
    {
        Debug.Log("MouseEnter: " + pointNumber);

        this.GetComponent<MeshRenderer>().enabled = true;

        string imagePath = fieldTeam.GetPhotoThumbnailPathFromTime(time);

        _mapFrameDisplay = Instantiate(fieldTeam.mapFrameDisplayPrefab, fieldTeam.map.transform);
        _mapFrameDisplayLogic = _mapFrameDisplay.GetComponent<MapFrameDisplayLogic>();
        _mapFrameDisplayLogic.DisplayImage(imagePath);
        _mapFrameDisplay.transform.position = this.transform.position;
        _mapFrameDisplay.transform.localScale = new Vector3(25.0f, 25.0f, 25.0f);
    }

    void OnMouseExit()
    {
        Debug.Log("MouseExit: " + pointNumber);

        this.GetComponent<MeshRenderer>().enabled = false;

        GameObject.Destroy(_mapFrameDisplay);
    }
}
