# Node.JS Client Sample

This is some Node.JS pseudo code to shape the client side.

The idea is that you can have different transports (e.g. IoT Hub, MQTT) and the client SDK will hide the implement details.

You only have to implement two callback (get/update state from cloud to device) and you get two functions to get/update state from device to cloud.

The state itself is just a JavaScript object, e.g. a JSON object.
