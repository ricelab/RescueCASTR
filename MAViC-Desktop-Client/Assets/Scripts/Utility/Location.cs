using System;

[Serializable]
public class Location
{
    public double Latitude;
    public double Longitude;
    public double Altitude;
    public double Accuracy;
    public double AltitudeAccuracy;
    public double Heading;
    public double Speed;
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
