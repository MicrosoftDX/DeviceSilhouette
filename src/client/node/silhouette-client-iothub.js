const EventEmitter = require('events');
const util = require('util');

var Protocol = require('azure-iot-device-amqp').Amqp;
var Client = require('azure-iot-device').Client;
var Message = require('azure-iot-device').Message;

var client;

function SilhouetteClientIoTHub()
{
  client = Client.fromConnectionString(config.connectionString, Protocol);
  client.open(connectCallback);
}

// Inherit from EventEmitter
util.inherits(SilhouetteClientIoTHub, EventEmitter);

/*
** Setup the IoT Hub callbacks
*/

var connectCallback = function (err) {
  client.on('message', function(msg) {
    // If this is a C2D_updateState message
    // this.emit('C2D_updateState')
    // If this is a C2D_getState message
    // this.emit('C2D_getState')    
  });
}

/*
** D2C Update State
*/

SilhouetteClientIoTHub.prototype.updateState = function(state)
{
  console.log("updateState");
  // client.sendEvent(message, ...)
}

/*
** D2C Get State
*/

SilhouetteClientIoTHub.prototype.getState = function(state)
{
  console.log("getState");
  // client.sendEvent(message, ...)
}

/*
** Create a Silhouette client using the IoT Hub transport
*/

function create(config)
{
  return new SilhouetteClientIoTHub(config);
}

module.exports.create = create;
