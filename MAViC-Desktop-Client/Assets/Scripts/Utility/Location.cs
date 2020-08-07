using System;

[Serializable]
public class Location
{
    public double Latitude = 0;
    public double Longitude = 0;
    public double Altitude = 0;
    public double Accuracy = 0;
    public double AltitudeAccuracy = 0;
    public double Heading = 0;
    public double Speed = 0;
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
