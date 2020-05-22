using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using Microsoft.MixedReality.WebRTC;
using Microsoft.MixedReality.WebRTC.Unity;
using Newtonsoft.Json;

public class MobileClient : MonoBehaviour
{
    #region Properties

    #region Public Properties

    public string clientID;
    public NodeDssSignaler nodeDssSignaler;
    public DataChannel dataChannel;

    public MapLogic MapLogic;

    public TextMesh clientIDText;

    #endregion

    #region Private Properties

    private LocationLogMessage _currentLocation;
    private Vector3 _currentScenePos;

    #endregion

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = new Vector3(0.0f, 0.25f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentLocation != null)
        {
            _currentScenePos = new Vector3(
                (float)( (_currentLocation.longitude + 122.912179) * 8.58 / 0.013164 - 4.15 ),
                0.25f,
                (float)( (_currentLocation.latitude - 49.210142) * 9.41 / 0.009415 - 4.95 )
            );

            this.transform.localPosition = _currentScenePos;
        }
    }

    public void StartCall()
    {
        nodeDssSignaler.RemotePeerId = clientID;
        clientIDText.text = clientID;

        StartCoroutine(LaunchCall());
    }

    IEnumerator LaunchCall()
    {
        yield return new WaitForSeconds(3);

        Task<DataChannel> addDataChannelTask = nodeDssSignaler.PeerConnection.Peer.AddDataChannelAsync("MobileClientDataChannel", true, true);

        yield return new WaitForSeconds(2);

        dataChannel = addDataChannelTask.Result;
        dataChannel.MessageReceived += DataChannel_MessageReceived;

        nodeDssSignaler.PeerConnection.Peer.CreateOffer();
        
        yield return null;
    }

    private void DataChannel_MessageReceived(byte[] obj)
    {
        string msg = Encoding.Default.GetString(obj);

        //Debug.Log("DataChannel_MessageReceived: " + msg);

        _currentLocation = JsonConvert.DeserializeObject<LocationLogMessage>(msg, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
    }
}
