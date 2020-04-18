using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using Microsoft.MixedReality.WebRTC;
using Microsoft.MixedReality.WebRTC.Unity;

public class MobileClient : MonoBehaviour
{
    #region Properties

    #region Public Properties

    public string clientID;
    public NodeDssSignaler nodeDssSignaler;
    public DataChannel dataChannel;

    public TextMesh clientIDText;

    #endregion

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        ///
    }

    // Update is called once per frame
    void Update()
    {
        ///
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
    }
}
