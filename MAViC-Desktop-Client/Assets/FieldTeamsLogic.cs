using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldTeamsLogic : MonoBehaviour
{
    public GameObject currentlyDeployedTeamsPanel;
    public GameObject completedTeamsPanel;
    public GameObject timelineContentPanel;
    public GameObject map;
    public GameObject sceneCamera;
    public GameObject sceneUi;
    public GameObject timelineCamera;
    public GameObject timelineUi;
    public UDateTime currentTime;

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

    public void Start()
    {
        FieldTeam[] fieldTeams = this.GetComponentsInChildren<FieldTeam>();
        foreach (FieldTeam fieldTeam in fieldTeams)
        {
            AddFieldTeam(fieldTeam);
        }
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
}
