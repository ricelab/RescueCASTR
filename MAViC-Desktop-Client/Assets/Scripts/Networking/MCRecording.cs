using System;
using Microsoft.MixedReality.WebRTC;

[Serializable]
public class MCRecordingFrame
{
    #region Properties

    #region Public Properties
    public string ClientID;
    public DateTime Timestamp;
    public Location Location;
    public I420VideoFrameStorage VideoFrame;
    #endregion

    #endregion


    #region Constructors

    public MCRecordingFrame(string clientID, DateTime timestamp, Location location, I420VideoFrameStorage videoFrameStorage)
    {
        this.ClientID = clientID;
        this.Timestamp = timestamp;
        this.Location = location;
        this.VideoFrame = videoFrameStorage;
    }

    public MCRecordingFrame(string clientID, DateTime timestamp, Location location, I420AVideoFrame videoFrame)
        : this(clientID, DateTime.Now, location, null)
    {
        unsafe
        {
            uint bufferSize = 3 * videoFrame.width * videoFrame.height / 2;
            if (videoFrame.dataA.ToPointer() != null)
            {
                bufferSize += videoFrame.width * videoFrame.height;
            }

            this.VideoFrame = new I420VideoFrameStorage();
            this.VideoFrame.Capacity = bufferSize;
            this.VideoFrame.Width = videoFrame.width;
            this.VideoFrame.Height = videoFrame.height;
            videoFrame.CopyTo(this.VideoFrame.Buffer);
        }
    }

    public MCRecordingFrame(string clientID, Location location, I420VideoFrameStorage videoFrameStorage)
        : this(clientID, DateTime.Now, location, videoFrameStorage)
    {
    }

    public MCRecordingFrame(string clientID, Location location, I420AVideoFrame videoFrame)
        : this(clientID, DateTime.Now, location, videoFrame)
    {
    }

    #endregion
}
