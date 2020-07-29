using System;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    public string resourcesUrl = "https://pages.cpsc.ucalgary.ca/~bdgjones/mavic-resources/";
    public UDateTime currentSimulatedTime;
    public GameObject currentlyDeployedTeamsPanel;
    public GameObject completedTeamsPanel;
    public GameObject timelineContentPanel;
    public GameObject mapObj;
    public Map map;
    public GameObject sceneCameraObj;
    public Camera sceneCamera;
    public CameraControls sceneCameraControls;
    public GameObject sceneUiObj;
    public GameObject wholeScreenUiObj;
    public GameObject timelineCameraObj;
    public Camera timelineCamera;
    public GameObject timelineUiObj;
    public GameObject sideUiObj;
    public SideUi sideUi;
    public GameObject currentTimeTextObj;

    public UDateTime earliestSimulatedStartTime
    {
        get
        {
            return _earliestSimulatedStartTime;
        }
    }
    public UDateTime latestSimulatedEndTime
    {
        get
        {
            return _latestSimulatedEndTime;
        }
    }

    private UDateTime _earliestSimulatedStartTime = null;
    private UDateTime _latestSimulatedEndTime = null;

    private DateTime _startTimeOfSimulation;
    private DateTime _actualStartTime;

    public void Start()
    {
        if (!resourcesUrl.EndsWith("/"))
        {
            resourcesUrl += "/";
        }

        _startTimeOfSimulation = currentSimulatedTime.dateTime;
        _actualStartTime = DateTime.Now;

        map = mapObj.GetComponent<Map>();
        sceneCamera = sceneCameraObj.GetComponent<Camera>();
        sceneCameraControls = sceneCameraObj.GetComponent<CameraControls>();
        timelineCamera = timelineCameraObj.GetComponent<Camera>();
        
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
        currentSimulatedTime.dateTime = new DateTime(_startTimeOfSimulation.Ticks + ticksSinceSimulationStart);
        currentTimeTextObj.GetComponent<Text>().text = currentSimulatedTime.dateTime.ToString("yyyy/MM/dd HH:mm:ss");
    }

    public void AddFieldTeam(FieldTeam fieldTeam)
    {
        if (_earliestSimulatedStartTime == null || fieldTeam.simulatedStartTime.dateTime < _earliestSimulatedStartTime.dateTime)
        {
            _earliestSimulatedStartTime = fieldTeam.simulatedStartTime;
        }
        if (_latestSimulatedEndTime == null || fieldTeam.simulatedEndTime.dateTime > _latestSimulatedEndTime)
        {
            _latestSimulatedEndTime = fieldTeam.simulatedEndTime;
        }

        fieldTeam.FieldTeamInstantiate();
    }

    public void ShowAllFieldTeams(bool showFootageThumbnails = false)
    {
        foreach (Transform t in this.transform)
        {
            FieldTeam ft = t.gameObject.GetComponent<FieldTeam>();
            if (ft != null && ft.isActiveAndEnabled)
            {
                ft.showFootageThumbnail = showFootageThumbnails;
                ft.fieldTeamAppearStatus = FieldTeam.FieldTeamAppearStatus.Showing;
            }
        }
    }
}
