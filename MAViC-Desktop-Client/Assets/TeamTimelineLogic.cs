using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamTimelineLogic : MonoBehaviour
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
                Image img = this.transform.Find("Line").GetComponent<Image>();
                img.color = fieldTeam.teamColor;
            }

            // Update name if team's name changed
            if (fieldTeam.teamName != _lastTeamName)
            {
                _lastTeamName = fieldTeam.teamName;
                Text txt = this.transform.Find("Label").GetComponent<Text>();
                txt.text = this.fieldTeam.teamName;
            }

            RectTransform lineTransform = this.transform.Find("Line").GetComponent<RectTransform>();
            
            float scaleX =
                (float)(fieldTeam.endTime.dateTime.Ticks - fieldTeam.startTime.dateTime.Ticks) /
                (float)(fieldTeam.fieldTeamsLogic.latestEndTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks);

            //float pivotX =
            //    (float)(fieldTeam.startTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks) /
            //    (float)(fieldTeam.fieldTeamsLogic.latestEndTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks) /
            //    (float)(scaleX);

            float n =
                (float)(fieldTeam.startTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks) /
                (float)(fieldTeam.fieldTeamsLogic.latestEndTime.dateTime.Ticks - fieldTeam.fieldTeamsLogic.earliestStartTime.dateTime.Ticks);
            float pivotX = n + n / (1.0f - scaleX) * scaleX;

            lineTransform.pivot = new Vector2(pivotX, lineTransform.pivot.y);
            lineTransform.localScale = new Vector3(scaleX, lineTransform.localScale.y, lineTransform.localScale.z);
        }
    }

    void OnMouseOver()
    {
        ///
    }
}
