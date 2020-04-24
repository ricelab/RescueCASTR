/**
 *
 * main.js
 * Main JavaScript code
 *
 */


'use strict';

jQuery(function()
{
	const url = window.location.hostname;
	const dssServerUrl = "https://" + url + ":3001";

	const doc = jQuery(document),
		win = jQuery(window);

	const POLL_SERVER_TIMEOUT = 200;
	const UPDATE_LOCATION_TIMEOUT = 2000;
	const SEND_LOCATION_TIMEOUT = 200;

	const constraints =
	{
		audio: false,
		video: { facingMode: { exact: "environment" } }
	};
	const configuration = {};
	const pc = new RTCPeerConnection(configuration);
	
	var iceCandidateQueue = [];

	var dc;

	var locationIsStreaming = false;
	var geolocationWatchId;
	var updatedLocation;

  	
  	var $_GET = {};
	if(document.location.toString().indexOf('?') !== -1)
	{
	    var query = document.location
	                   .toString()
	                   // get the query string
	                   .replace(/^.*?\?/, '')
	                   // and remove any existing hash string (thanks, @vrijdenker)
	                   .replace(/#.*$/, '')
	                   .split('&');

	    for(var i=0, l=query.length; i<l; i++)
	    {
	       var aux = decodeURIComponent(query[i]).split('=');
	       $_GET[aux[0]] = aux[1];
	    }
	}
	var clientID = $_GET['clientID'];
	console.log(clientID);
	if (clientID.toUpperCase() === 'DESKTOP')
	{
		window.alert("Mobile client cannot use ClientID 'desktop'.");
		throw new Error("Mobile client cannot use ClientID 'desktop'.");
	}


	/**
	* CALL
	*/

	// Send any ice candidates to the other peer.
	// pc.onicecandidate = ({candidate}) => signaling.send({candidate});
	pc.onicecandidate = function (event)
	{
		if (event.candidate)
		{
		  PostToServer(
		  {
		    'MessageType': 3, // ICE
		    'Data': event.candidate + "|" + event.candidate.sdpMLineIndex.toString(10) + "|" + event.candidate.sdpMid,
		    'IceDataSeparator': "|"
		  });
		}
	};

	// Let the "negotiationneeded" event trigger offer generation.
	pc.onnegotiationneeded = async () =>
	{
		// try
		// {
		//   await pc.setLocalDescription(await pc.createOffer());

		//   console.log(pc.localDescription.sdp);
		  
		//   // send the offer
		//   PostToServer(
		//   {
		//     'MessageType': 1,  // offer
		//     'Data': pc.localDescription.sdp
		//   });
		// }
		// catch (err)
		// {
		//   console.error(err);
		// }
	};

	pc.ondatachannel = function(event)
	{
		console.log('ondatachannel');

		dc = event.channel;

		dc.onclosing = function()
		{
			StopStreamingLocation();
		}

		StartStreamingLocation();
	};


	function setOnTrackOrStreamHandler(peer, handler)
	{
		if ('ontrack' in peer)
		{
		  peer.ontrack = function(event)
		  {
		    console.log('ontrack');

		    var remoteStream = event.streams[0];
		    handler(remoteStream);
		  }
		}
		else
		{
		  peer.onaddstream = function(event)
		  {
		    console.log('onaddstream');

		    var remoteStream = event.stream;
		    handler(remoteStream);
		  }
		}
	}

	function playRemoteStream(stream)
	{
		$('#remoteVideo').prop('srcObject', stream);
	}


	function SetupLocalStream()
	{
		// Get audio/video stream
		navigator.mediaDevices.getUserMedia(constraints).then(function(stream)
		{
		  stream.getTracks().forEach((track) => pc.addTrack(track, stream));
		  window.localStream = stream;

		  $('#localVideo').prop('srcObject', window.localStream);

		  setOnTrackOrStreamHandler(pc, (remoteStream) =>
		  {
		    playRemoteStream(remoteStream);
		  });

		  // Send connection request to desktop client

		  var msg =
		  {
		  	'EventName': 'ConnectionRequest',
		  	'EventData': clientID
		  };

		  $.ajax(
		  {
		    url: dssServerUrl + "/event/desktop",
		    type: 'POST',
		    //dataType: 'json',
		    //contentType: 'application/json; charset=utf-8',
		    data: JSON.stringify(msg),
		    cache: false,
		    processData: false
		  })
		  .done(function(response)
		  {
		    console.log("POST Response: " + response);
		  });

		  PollServer();
		})
		.catch(function(err)
		{
		  console.log("Error setting up local stream: " + err);
		});
	}

	function PostToServer(msg)
	{
		$.ajax(
		{
		  url: dssServerUrl + "/data/desktop",
		  type: 'POST',
		  //dataType: 'json',
		  //contentType: 'application/json; charset=utf-8',
		  data: JSON.stringify(msg),
		  cache: false,
		  processData: false
		})
		.done(function(response)
		{
		  console.log("POST Response: " + response);
		});
	}

	async function GetAndProcessFromServer()
	{
		$.ajax(
		{
		  type: 'GET',
		  //dataType: 'json',
		  url: dssServerUrl + "/data/" + clientID,
		  success: function(msgStr)
		  {
		    var msg = JSON.parse(msgStr);

		    switch (msg.MessageType)
		    {
		      case 1:   // offer
		          pc.setRemoteDescription(new RTCSessionDescription(
		          {
		            'type': 'offer',
		            'sdp': msg.Data
		          }))
		          .then(function()
		          {
		            pc.createAnswer(function(result)
		            {
		              pc.setLocalDescription(result, function()
		              {
		                while (iceCandidateQueue.length > 0)
		                {
		                  pc.addIceCandidate(iceCandidateQueue.shift());
		                }

		                //signaling.send({desc: pc.localDescription});
		                PostToServer(
		                {
		                  'MessageType': 2,
		                  'Data': pc.localDescription.sdp
		                });
		              },
		              function()
		              {
		                console.log("setLocalDescription error");
		              });
		            },
		            function(error)
		            {
		              console.log("createAnswer error");
		            });
		          });
		          
		          break;
		      case 2:   // answer
		          pc.setRemoteDescription(new RTCSessionDescription(
		          {
		            'type': 'answer',
		            'sdp': msg.Data
		          }))
		          .then(function()
		          {
		            ///
		          });
		          
		          break;
		      case 3:   // ICE
		          var parts = msg.Data.split(msg.IceDataSeparator);
		          
		          var candidateObj = new RTCIceCandidate(
		          {
		            candidate: parts[0],
		            sdpMLineIndex: parts[1],
		            sdpMid: parts[2]
		          });
		          
		          if(!pc || !pc.remoteDescription || !pc.remoteDescription.type)
		          {
		            iceCandidateQueue.push(candidateObj);
		          }
		          else
		          {
		            pc.addIceCandidate(candidateObj);
		          }
		          
		          break;
		      default:
		          console.log("Unknown message: " + msg.MessageType + ": " + msg.Data);

		          break;
		    }
		  },
		  error: function (xhr, ajaxOptions, thrownError)
		  {
		      // if(xhr.status==404)
		      // {
		      //     console.log(thrownError);
		      // }
		  }
		});
	}

	function PollServer()
	{
		GetAndProcessFromServer();
		setTimeout(PollServer, POLL_SERVER_TIMEOUT);
	}

	function StartStreamingLocation()
	{
		StopStreamingLocation();

		if (navigator.geolocation)
		{
			locationIsStreaming = true;

			geolocationWatchId = navigator.geolocation.watchPosition(
				function(pos)
				{
					console.log(pos.coords);

					updatedLocation =
					{
						'latitude': pos.coords.latitude,
						'longitude': pos.coords.longitude,
						'altitude': pos.coords.altitude,
						'accuracy': pos.coords.accuracy,
						'altitudeAccuracy': pos.coords.altitudeAccuracy,
						'heading': pos.coords.heading,
						'speed': pos.coords.speed,
						'timestamp': pos.timestamp
					};
				},
				function(err)
				{
					console.warn('ERROR(' + err.code + '): ' + err.message);
				},
				{
					enableHighAccuracy: false,
					timeout: UPDATE_LOCATION_TIMEOUT,
					maximumAge: 0
				});

			SendLocation();
		}
		else
		{
			console.log('Geolocation is not supported by this browser/OS.');
		}
	}

	function StopStreamingLocation()
	{
		locationIsStreaming = false;

		if (geolocationWatchId)
		{
			navigator.geolocation.clearWatch(geolocationWatchId);
			geolocationWatchId = null;
		}
	}

	function SendLocation()
	{
		if (updatedLocation)
		{
			dc.send(JSON.stringify(updatedLocation));
		}
		
		if (locationIsStreaming)
		{
			setTimeout(SendLocation, SEND_LOCATION_TIMEOUT);
		}
	}


	/**
	* MAIN
	*/

	SetupLocalStream();
});
