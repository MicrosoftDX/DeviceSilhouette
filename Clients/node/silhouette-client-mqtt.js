// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
const EventEmitter = require('events');
const util = require('util');

function SilhouetteClientMQTT()
{
  // Start something that listens for messages from the cloud service...
  // this.emit('C2D_updateState')
  // this.emit('C2D_getState')
}

// Inherit from EventEmitter
util.inherits(SilhouetteClientMQTT, EventEmitter);

/*
** D2C Update State
*/

SilhouetteClientMQTT.prototype.updateState = function(state)
{
  console.log("updateState");
}

/*
** D2C Get State
*/

SilhouetteClientMQTT.prototype.getState = function(state)
{
  console.log("getState");
}

/*
** Create a Silhouette client using the MQTT transport
*/

function create(config)
{
  return new SilhouetteClientMQTT(config);
}

module.exports.create = create;

