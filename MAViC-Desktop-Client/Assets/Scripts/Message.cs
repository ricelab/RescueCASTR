using System;

[Serializable]
public class MessageJson
{
    public string time;
    public bool instantiateBySimulatedTime;
    public MessageDirection direction;
    public string content;
}

public enum MessageDirection
{
    FromTeamToCommand,
    FromCommandToTeam
};

[Serializable]
public class Message : Communication
{
    public MessageDirection messageDirection = MessageDirection.FromTeamToCommand;
    public string messageContent;
}
