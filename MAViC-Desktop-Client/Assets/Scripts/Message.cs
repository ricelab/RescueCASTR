using System;

[Serializable]
public class Message
{
    public enum MessageDirection
    {
        FromTeamToCommand,
        FromCommandToTeam
    };

    public FieldTeam fieldTeam;
    public UDateTime simulatedTime;
    public UDateTime actualTime;
    public bool instantiateBySimulatedTime = false;
    public Location location;
    public MessageDirection messageDirection = MessageDirection.FromTeamToCommand;
    public string messageContent;
    public bool isStarted = false;

    public void Start()
    {
        if (!isStarted)
        {
            if (instantiateBySimulatedTime)
            {
                actualTime = fieldTeam.ConvertSimulatedTimeToActualTime(simulatedTime);
            }
            else
            {
                simulatedTime = fieldTeam.ConvertActualTimeToSimulatedTime(actualTime);
            }

            isStarted = true;
        }
    }
}
