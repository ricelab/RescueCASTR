using System;

[Serializable]
public class Location
{
    public double latitude = 0;
    public double longitude = 0;
    public double altitude = 0;
    public double accuracy = 0;
    public double altitudeAccuracy = 0;
    public double heading = 0;
    public double speed = 0;
}

[Serializable]
public class LocationLogMessage
{
    public double latitude;
    public double longitude;
    public double accuracy;
    public double altitudeAccuracy;
    public double heading;
    public double speed;
    public ulong timestamp;
}
