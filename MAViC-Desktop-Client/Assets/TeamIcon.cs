using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamIcon : MonoBehaviour
{
    public FieldTeam fieldTeam;

    private Color _lastTeamColor;
    private string _lastTeamName;

    // Update is called once per frame
    void Update()
    {
        if (fieldTeam)
        {
            // Update icon colour if team's colour changed
            if (fieldTeam.teamColor != _lastTeamColor)
            {
                _lastTeamColor = fieldTeam.teamColor;
                Image img = this.transform.Find("Image").GetComponent<Image>();
                img.color = fieldTeam.teamColor;
            }

            // Update name if team's name changed
            if (fieldTeam.teamName != _lastTeamName)
            {
                _lastTeamName = fieldTeam.teamName;
                Text txt = this.transform.Find("Text").GetComponent<Text>();
                txt.text = this.fieldTeam.teamName;
            }
        }
    }
}
