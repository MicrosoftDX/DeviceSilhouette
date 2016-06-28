const EventEmitter = require('events');
const util = require('util');

// Convenience variable so I can get to "this"
var self;

var Protocol = require('azure-iot-device-http').Http;
var Client = require('azure-iot-device').Client;
var Message = require('azure-iot-device').Message;

var client;

function SilhouetteClientIoTHub(config)
{
  // Remember "this"
  self = this;

  client = Client.fromConnectionString(config.connectionString, Protocol);
  client.on('message', processMessage);
  client.open(function(err) {
    console.log("Client connected.");
  });
  
  // TODO: make sure the client object can be accessed from the caller
}

// Inherit from EventEmitter
util.inherits(SilhouetteClientIoTHub, EventEmitter);

/*
** Process incoming messages
*/

function processMessage(msg)
{
  // With AMQP, use this trick
  // var msgType = msg.transportObj.applicationProperties.MessageType;
  var msgType = getMessageType(msg.properties);
  // TODO: what if we can't find the messageType? i.e. it's not a Silhouette message?
  // TODO: should we forward the message to some other callback?
  switch (msgType) {
    case 'State:Set':
      console.log("C2D_UpdateState");
      self.emit('C2D_updateState', JSON.parse(JSON.parse(msg.data).State));
      break;
    case 'State:Get':
      console.log("C2D_GetState");
      self.emit('C2D_getState');
      break;
    default:
      console.log("Unknown MessageType.");
      break;
  }
  client.complete(msg, function(err) {
    // TODO: Handle errors
  });
}

/*
** Get the MessageType from the message Properties
** TODO: doesn't work in AMQP. See issue https://github.com/Azure/azure-iot-sdks/issues/352
** TODO: doesn't work in HTTP either. Had to patch toMessage() in azure-iot-http-base to parse the headers.
*/

function getMessageType(properties)
{
  for (var i=0; i<properties.count(); i++) {
    if (properties.getItem(i).key == "iothub-app-MessageType")
      return properties.getItem(i).value;
  }
  
  return null;
}

/*
** D2C Update State
*/

SilhouetteClientIoTHub.prototype.updateState = function(state)
{
  // TODO: Make sure timestamp in UTC and not in local computer timezone	
  var formattedDate = new Date().toISOString();

  //var timestamp = Date.now();
  var full_state =
  {
  "DeviceID" : "device1",
  "Timestamp" : formattedDate,
  "Status" : "Reported",
  "State" : state
  };
	
  var data = JSON.stringify(full_state);
  var message = new Message(data);
  message.properties.add('MessageType', 'State:Set');
  //console.log("outgoing message:");
  //console.log(message);
  client.sendEvent(message, function(err) {
    // TODO: what if we have an error here ?
  });
}

/*
** D2C Get State
*/

SilhouetteClientIoTHub.prototype.getState = function(state)
{	
  var formattedDate = new Date().toISOString();
  var getStateMsg = 
  {
	"DeviceID" : "device1",
	"Timestamp" : formattedDate,
	"Status" : "Get"   
  };  
  var data = JSON.stringify(getStateMsg);
  
  var message = new Message(data);
  message.properties.add('MessageType', 'State:Get');
  client.sendEvent(message, function(err) {
    console.log("failed to get state with error: " + err);
  });
}

/*
** Create a Silhouette client using the IoT Hub transport
*/

function create(config)
{
  return new SilhouetteClientIoTHub(config);
}

module.exports.create = create;
