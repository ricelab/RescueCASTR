using System;
using UnityEngine.UI;

[Serializable]
public class ClueJson
{
    public string time;
    public bool instantiateBySimulatedTime;
    public string photoFileName;
    public string textDescription;
}

[Serializable]
public class Clue : Communication
{
    public string textDescription;
    public string photoFileName;
    public Image photo;
}
