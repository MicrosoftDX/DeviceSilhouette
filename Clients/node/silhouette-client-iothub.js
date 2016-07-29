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
  client.open(function(err) {
    console.log("Client connected.");
    client.on('message', processMessage);
    client.on('error', function(err) {
      console.log(err);
    });
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
  var msgSubType = getMessageSubType(msg.properties);
 
  // TODO: what if we can't find the messageType? i.e. it's not a Silhouette message?
  // TODO: should we forward the message to some other callback?
  
  if (msgType == 'CommandRequest') {
    switch (msgSubType) {
      case 'SetState':
        self.emit('C2D_updateState', JSON.parse(msg.data));
        break;
      case 'ReportState':
        self.emit('C2D_getState');
        break;
      default:
        console.log("Unknown MessageSubType.");
        break;
    }
  } else if (msgType == 'InquiryResponse') {
    switch (msgSubType) {
      case 'GetState':
        self.emit('C2D_updateState', JSON.parse(msg.data));
        break;
      default:
        console.log("Unknown MessageSubType.");
        break;
    }
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
    if (properties.getItem(i).key.toLowerCase()  === "iothub-app-messagetype")
      return properties.getItem(i).value;
  }
  
  return null;
}

function getMessageSubType(properties)
{
  for (var i=0; i<properties.count(); i++) {
    if (properties.getItem(i).key.toLowerCase()  === "iothub-app-messagesubtype")
      return properties.getItem(i).value;
  }
  
  return null;
}

/*
** D2C Update State
*/

SilhouetteClientIoTHub.prototype.updateState = function(metadata, values, deviceID)
{
  // TODO: Make sure timestamp in UTC and not in local computer timezone	
  var formattedDate = new Date().toISOString();
	
  var data = JSON.stringify(values);
  var message = new Message(data);
  message.properties.add('MessageType', 'Report');
  message.properties.add('MessageSubType', 'State');
  // message.correlationId = "qwertyuiop"; // TODO set correlationId when responding to messages
  client.sendEvent(message, function(err) {
    // TODO: what if we have an error here ?
  });
}

/*
** D2C Get State
*/

SilhouetteClientIoTHub.prototype.getState = function(state, deviceID)
{	
  var message = new Message("{}");
  message.properties.add('MessageType', 'InquiryRequest');
  message.properties.add('MessageSubType', 'GetState');
  client.sendEvent(message, function(err, res) {
    // TODO: add callback for error handling
    if (err) console.log("failed to get state with error: " + err);
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
