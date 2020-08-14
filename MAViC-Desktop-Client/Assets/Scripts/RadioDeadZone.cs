using System;
using UnityEngine;

[Serializable]
public class RadioDeadZoneJson
{
    public string startTime;
    public string endTime;
    public bool instantiateBySimulatedTime;
}

[Serializable]
public class RadioDeadZone
{
    public UDateTime simulatedStartTime;
    public UDateTime simulatedEndTime;

    public UDateTime actualStartTime;
    public UDateTime actualEndTime;
}
