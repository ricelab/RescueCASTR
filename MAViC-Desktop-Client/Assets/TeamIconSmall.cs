using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamIconSmall : MonoBehaviour
{
    public FieldTeam fieldTeam;

    private Color _lastTeamColor;
    
    // Start is called before the first frame update
    void Start()
    {
        ///
    }

    // Update is called once per frame
    void Update()
    {
        // Update icon colour if team's colour changed
        if (fieldTeam.teamColor != _lastTeamColor)
        {
            _lastTeamColor = fieldTeam.teamColor;
            Image img = this.transform.Find("Image").GetComponent<Image>();
            img.color = fieldTeam.teamColor;
        }
    }
}
