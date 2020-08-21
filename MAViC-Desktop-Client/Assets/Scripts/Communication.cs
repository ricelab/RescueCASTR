using System;

[Serializable]
public abstract class Communication : IComparable
{
    public FieldTeam fieldTeam;
    public UDateTime simulatedTime;
    public UDateTime actualTime;
    public bool instantiateBySimulatedTime = false;
    public Location location;
    public bool highlightMapIcon = false;
    public bool highlightTimelineIcon = false;
    public bool isStarted = false;

    public void Start()
    {
        if (!isStarted)
        {
            if (instantiateBySimulatedTime)
            {
                actualTime = fieldTeam.ConvertSimulatedTimeToActualTime(simulatedTime);
            }
            else // if (!instantiateBySimulatedTime)
            {
                simulatedTime = fieldTeam.ConvertActualTimeToSimulatedTime(actualTime);
            }

            isStarted = true;
        }
    }

    public int CompareTo(Object obj)
    {
        if (obj == null)
        {
            return 1;
        }

        Communication comm = obj as Communication;
        if (comm != null)
        {
            Start();
            return this.simulatedTime.dateTime.CompareTo(comm.simulatedTime.dateTime);
        }
        else
        {
            throw new ArgumentException("Object is not a Communication");
        }
    }
}
