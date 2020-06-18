using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldTeam : MonoBehaviour
{
    public GameObject teamIconPrefab;
    
    public string teamName;
    public Color teamColor;
    public string gpsRecordingFilename;

    private FieldTeamsLogic _fieldTeamsLogic;
    private TeamIcon _teamIcon;

    // Start is called before the first frame update
    void Start()
    {
        //teamColor = new Color(
        //    Random.Range(0f, 1f),
        //    Random.Range(0f, 1f),
        //    Random.Range(0f, 1f)
        //);

        _fieldTeamsLogic = this.gameObject.transform.parent.GetComponent<FieldTeamsLogic>();

        GameObject newTeamIconObj = Instantiate(teamIconPrefab, _fieldTeamsLogic.currentlyDeployedTeamsPanel.transform);
        _teamIcon = newTeamIconObj.GetComponent<TeamIcon>();
        _teamIcon.fieldTeam = this;
    }

    // Update is called once per frame
    void Update()
    {
        //
    }
}
