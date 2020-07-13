using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    public UDateTime currentTime;
    public GameObject currentlyDeployedTeamsPanel;
    public GameObject completedTeamsPanel;
    public GameObject timelineContentPanel;
    public GameObject mapObj;
    public GameObject sceneCamera;
    public GameObject sceneUiObj;
    public GameObject wholeScreenUiObj;
    public GameObject timelineCamera;
    public GameObject timelineUiObj;
    public GameObject sideUiObj;
    public SideUi sideUi;
    public GameObject currentTimeText;

    public UDateTime earliestStartTime
    {
        get
        {
            return _earliestStartTime;
        }
    }
    public UDateTime latestEndTime
    {
        get
        {
            return _latestEndTime;
        }
    }

    private UDateTime _earliestStartTime = null;
    private UDateTime _latestEndTime = null;

    private DateTime _startTimeOfSimulation;
    private DateTime _actualStartTime;

    public void Start()
    {
        _startTimeOfSimulation = currentTime.dateTime;
        _actualStartTime = DateTime.Now;
        
        FieldTeam[] fieldTeams = this.GetComponentsInChildren<FieldTeam>();
        foreach (FieldTeam fieldTeam in fieldTeams)
        {
            AddFieldTeam(fieldTeam);
        }
    }

    public void Update()
    {
        // Update clock
        long ticksSinceSimulationStart = DateTime.Now.Ticks - _actualStartTime.Ticks;
        currentTime.dateTime = new DateTime(_startTimeOfSimulation.Ticks + ticksSinceSimulationStart);
        currentTimeText.GetComponent<Text>().text = currentTime.dateTime.ToString("yyyy/MM/dd HH:mm:ss");
    }

    public void AddFieldTeam(FieldTeam fieldTeam)
    {
        if (_earliestStartTime == null || fieldTeam.startTime.dateTime < _earliestStartTime.dateTime)
        {
            _earliestStartTime = fieldTeam.startTime;
        }
        if (_latestEndTime == null || fieldTeam.endTime.dateTime > _latestEndTime)
        {
            _latestEndTime = fieldTeam.endTime;
        }

        fieldTeam.FieldTeamInstantiate();
    }

    public void ShowAllFieldTeams()
    {
        foreach (Transform t in this.transform)
        {
            FieldTeam ft = t.gameObject.GetComponent<FieldTeam>();
            if (ft != null && ft.isActiveAndEnabled)
            {
                ft.fieldTeamAppearStatus = FieldTeam.FieldTeamAppearStatus.Showing;
            }
        }
    }
}
