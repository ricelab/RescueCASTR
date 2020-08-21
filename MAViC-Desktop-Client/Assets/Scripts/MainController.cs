using System;
using System.Web;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ScenarioJson
{
    public string currentSimulatedTime;
    public Location pointLastSeen;
    public Location lastKnownPosition;
    public FieldTeamJson[] fieldTeams;
}

public class MainController : MonoBehaviour
{
    public string resourcesUrl = "http://pages.cpsc.ucalgary.ca/~bdgjones/mavic-resources/";

    public string scenariosPath = "Scenarios/";

    public int scenarioId;
    public int mapId;

    public GameObject[] mapObjList;
    public Map[] mapList;

    public UDateTime currentSimulatedTime;

    public GameObject fieldTeamPrefab;

    public Location pointLastSeen;
    public Location lastKnownPosition;
    public GameObject plsMarkerObj;
    public GameObject lkpMarkerObj;
    public GameObject plsMarkerPrefab;
    public GameObject lkpMarkerPrefab;

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

    public ImageLoaderCache footageThumbnailsCache;
    public ImageLoaderCache footagePhotosCache;
    public ImageLoaderCache cluesPhotosCache;

    public FullscreenView fullscreenView
    {
        get
        {
            if (footageFullscreenView != null)
                return footageFullscreenView;
            else if (clueFullscreenView != null)
                return clueFullscreenView;
            else if (messageFullscreenView != null)
                return messageFullscreenView;

            else
                return null;
        }
    }
    public FootageFullscreenView footageFullscreenView;
    public ClueFullscreenView clueFullscreenView;
    public MessageFullscreenView messageFullscreenView;

    public GameObject fullscreenViewObj;

    public GameObject footageFullscreenViewPrefab;
    public GameObject clueFullscreenViewPrefab;
    public GameObject messageFullscreenViewPrefab;

    public bool footageFullscreenViewShowingLive = false;

    public bool mouseHoveringOverIcon = false;

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

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("The full URL is: " + Application.absoluteURL);

            // Set scenario and map based on URL parameters

            Uri uri = new Uri(Application.absoluteURL);

            string scenarioArg = HttpUtility.ParseQueryString(uri.Query).Get("scenario");
            string mapArg = HttpUtility.ParseQueryString(uri.Query).Get("map");

            if (scenarioArg == null || !int.TryParse(scenarioArg, out scenarioId))
            {
                scenarioId = 0;
            }
            if (mapArg == null || !int.TryParse(mapArg, out mapId))
            {
                mapId = 0;
            }
        }

        footageThumbnailsCache = new ImageLoaderCache(100);
        footagePhotosCache = new ImageLoaderCache(25);
        cluesPhotosCache = new ImageLoaderCache(25);

        mapObj = mapObjList[mapId];
        foreach(GameObject mo in mapObjList)
        {
            mo.SetActive(false);
        }
        mapObj.SetActive(true);

        map = mapObj.GetComponent<Map>();
        sceneCamera = sceneCameraObj.GetComponent<Camera>();
        sceneCameraControls = sceneCameraObj.GetComponent<CameraControls>();
        timelineCamera = timelineCameraObj.GetComponent<Camera>();

        // Load scenario JSON
        TextAsset scenarioJsonFile = Resources.Load<TextAsset>(scenariosPath + scenarioId + "/scenario");
        ScenarioJson scenarioJson = JsonUtility.FromJson<ScenarioJson>(scenarioJsonFile.text);

        // Set current simulated time
        currentSimulatedTime = Convert.ToDateTime(scenarioJson.currentSimulatedTime);

        // Set Point Last Seen
        pointLastSeen = scenarioJson.pointLastSeen;

        // Set Last Known Position
        lastKnownPosition = scenarioJson.lastKnownPosition;

        // Setup field teams
        foreach(FieldTeamJson fieldTeamJson in scenarioJson.fieldTeams)
        {
            GameObject fieldTeamObj = GameObject.Instantiate(fieldTeamPrefab, this.transform);
            FieldTeam fieldTeam = fieldTeamObj.GetComponent<FieldTeam>();
            fieldTeam.teamName = fieldTeamJson.name;
            ColorUtility.TryParseHtmlString(fieldTeamJson.color, out fieldTeam.teamColor);
            fieldTeam.recordingDirectoryPath = scenariosPath + scenarioId + "/TeamRecords/" + fieldTeamJson.path;
            fieldTeam.simulatedStartTime = Convert.ToDateTime(fieldTeamJson.simulatedStartTime);
            fieldTeam.mainController = this;

            AddFieldTeam(fieldTeam);
        }

        // Instantiate PLS and LKP markers
        plsMarkerObj = GameObject.Instantiate(plsMarkerPrefab, sceneUiObj.transform);
        plsMarkerObj.transform.SetSiblingIndex(0);
        lkpMarkerObj = GameObject.Instantiate(lkpMarkerPrefab, sceneUiObj.transform);
        lkpMarkerObj.transform.SetSiblingIndex(0);

        _startTimeOfSimulation = currentSimulatedTime.dateTime;
        _actualStartTime = DateTime.Now;
    }

    public void Update()
    {
        // Update clock
        long ticksSinceSimulationStart = DateTime.Now.Ticks - _actualStartTime.Ticks;
        currentSimulatedTime.dateTime = new DateTime(_startTimeOfSimulation.Ticks + ticksSinceSimulationStart);
        currentTimeTextObj.GetComponent<Text>().text = currentSimulatedTime.dateTime.ToString("yyyy/MM/dd HH:mm:ss");

        // Update PLS marker
        RectTransform canvasRect = sceneUiObj.GetComponent<RectTransform>();
        Vector2 viewportPos = sceneCamera.WorldToViewportPoint(map.ConvertLocationToMapPosition(pointLastSeen));
        Vector2 worldObjScreenPos = new Vector2(
            ((viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
            ((viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f))
        );
        plsMarkerObj.GetComponent<RectTransform>().anchoredPosition = worldObjScreenPos;

        // Update LKP marker
        viewportPos = sceneCamera.WorldToViewportPoint(map.ConvertLocationToMapPosition(lastKnownPosition));
        worldObjScreenPos = new Vector2(
            ((viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
            ((viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f))
        );
        lkpMarkerObj.GetComponent<RectTransform>().anchoredPosition = worldObjScreenPos;
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

    public void ShowAllFieldTeams(bool showExtraDetails = false)
    {
        foreach (Transform t in this.transform)
        {
            FieldTeam ft = t.gameObject.GetComponent<FieldTeam>();
            if (ft != null && ft.isActiveAndEnabled)
            {
                ft.showExtraDetails = showExtraDetails;
                ft.fieldTeamAppearStatus = FieldTeam.FieldTeamAppearStatus.Showing;
            }
        }
    }
}
