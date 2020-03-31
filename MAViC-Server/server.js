/**
 *
 * server.js
 * Node.js Server
 *
 */


"use strict";

const fs = require('fs');

const options = {
  key: fs.readFileSync('key.pem'),
  cert: fs.readFileSync('cert.pem')
};

const http = require('http');
const https = require('https');


/* MAIN SERVER (Static and Socket.io) */

const node_static = require('node-static');
const staticServer = new node_static.Server('../MAViC-Mobile-Client');

const mainServer = https.createServer(options, function(request, response)
{
	request.addListener('end', function() {
		staticServer.serve(request, response);
	}).resume();
});

const io = require('socket.io',
		{
			rememberTransport: false,
			transports: ['WebSocket', 'Flash Socket', 'AJAX long-polling']
		}
	)(mainServer);

mainServer.listen(8082);


/* DSS/EVENT SERVER */

const finalhandler = require('finalhandler');
const debug = require('debug')('dss:boot');
const router = require('./node-dss');

// Non-secure DSS/event server (for Unity), on port 3000
const dssServer = http.createServer(function (req, res)
{
  router(req, res, finalhandler(req, res));
});
const bind = dssServer.listen(process.env.PORT || 3000, () =>
{
  debug(`online @ ${bind.address().port}`);
});

// Secure DSS/event server (for web browsers), on port 3001
const dssServerSecure = https.createServer(options, function (req, res)
{
  router(req, res, finalhandler(req, res));
});
const bindSec = dssServerSecure.listen(process.env.PORT || 3001, () =>
{
  debug(`online @ ${bindSec.address().port}`);
});


/* STATUS */

var serverStartedDate = Date.now();
console.log('Server started. [' + (new Date()).toString() + ']');


/* CLIENT SOCKET SESSION IDs */
var investigatorClientSocketSessionID = null;
var desktopClientSocketSessionID = null;
var mobileClientSocketSessionIDs = [];
var totalNumMobileClients = 0;


/* CALL */
var callIsOnline = false;


io.sockets.on('connection', function(socket)
{
	var clientAddress = socket.request.connection.remoteAddress;

	console.log('A client (' + clientAddress + ') connected [' + (new Date()).toString() + ']');

	var clientType;
	var mobileClientNumber;

	socket.on('disconnect', function()
	{
		if (clientType == 'Mobile')
		{
			console.log('Mobile client ' + mobileClientNumber + ' (' + clientAddress + ') disconnected [' + (new Date()).toString() + ']');
			mobileClientSocketSessionIDs = arrayRemove(mobileClientSocketSessionIDs, socket.id);
		}
		else if (clientType == 'Desktop')
		{
			console.log('Desktop client (' + clientAddress + ') disconnected [' + (new Date()).toString() + ']');
			desktopClientSocketSessionID = null;

			io.sockets.emit('ClientDisconnect', 'Desktop');
		}
		else if (clientType == 'Investigator')
		{
			console.log('Investigator client (' + clientAddress + ') disconnected [' + (new Date()).toString() + ']');
			investigatorClientSocketSessionID = null;
		}
	});


	if (desktopClientSocketSessionID != null)
	{
		socket.emit('ClientConnect', 'Desktop');
	}
	else
	{
		socket.emit('ClientDisconnect', 'Viewer');
	}


	/**
	 * SOCKET MESSAGE HANDLERS
	 */
	
	/* DEBUGGING */
	
	socket.on('Echo', function(data)
	{
		console.log(data + '[' + (new Date()).toString() + ']');
	});


	/* CONNECTION */

	socket.on('MobileClientConnect', function(data)
	{
		mobileClientSocketSessionIDs.push(socket.id);
		mobileClientNumber = totalNumMobileClients++;
		console.log('Mobile client ' + mobileClientNumber + ' connected (' + clientAddress + ') [' + (new Date()).toString() + ']');
		clientType = 'Mobile';
	});

	socket.on('DesktopClientConnect', function(data)
	{
		if (desktopClientSocketSessionID == null)
		{
			desktopClientSocketSessionID = socket.id;
			console.log('Desktop client connected (' + clientAddress + ') [' + (new Date()).toString() + ']');
			clientType = 'Desktop';
			
			io.sockets.emit('ClientConnect', 'Desktop');
		}
		else
		{
			socket.disconnect('unauthorized');
			console.log('Unauthorized desktop client (' + clientAddress + ') tried to connect [' + (new Date()).toString() + ']');
		}
	});

	socket.on('InvestigatorClientConnect', function(data)
	{
		if (investigatorClientSocketSessionID == null)
		{
			investigatorClientSocketSessionID = socket.id;
			console.log('Investigator client connected (' + clientAddress + ') [' + (new Date()).toString() + ']');
			clientType = 'Investigator';
		}
		else
		{
			socket.disconnect('unauthorized');
			console.log('Unauthorized investigator client (' + clientAddress + ') tried to connect [' + (new Date()).toString() + ']');
		}
	});
});


function arrayRemove(arr, value)
{
	return arr.filter(function(ele)
		{
			return ele != value;
		});
}
