using System;
using UnityEngine.UI;

[Serializable]
public class Clue
{
    public FieldTeam fieldTeam;
    public UDateTime simulatedTime;
    public UDateTime actualTime;
    public bool instantiateBySimulatedTime = false;
    public Location location;
    public string textDescription;
    public string photoFileName;
    public Image photo;
    public bool isStarted = false;

    public void Start()
    {
        if (!isStarted)
        {
            if (instantiateBySimulatedTime)
            {
                actualTime = fieldTeam.ConvertTimeToActualTime(simulatedTime);
            }
            else
            {
                simulatedTime = fieldTeam.ConvertActualTimeToTime(actualTime);
            }

            isStarted = true;
        }
    }
}
