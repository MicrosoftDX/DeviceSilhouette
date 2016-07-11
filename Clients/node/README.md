# Node.JS Client Sample

This is some Node.JS pseudo code to shape the client side.

The idea is that you can have different transports (e.g. IoT Hub, MQTT) and the client SDK will hide the implement details.

You only have to implement two callback (get/update state from cloud to device) and you get two functions to get/update state from device to cloud.

The state itself is just a JavaScript object, e.g. a JSON object.


#### Note:
Node.JS SDK have not yet implemented system properties properly in the SDK. Below is a workaround in order to be able to access the MessageType.

Modify the file node_modules\azure-iot-device-http\node_modules\azure-iot-http-base\lib\http.js as follows:

Find this block of code:
```
/*Codes_SRS_NODE_HTTP_05_010: [If the HTTP response has an 'iothub-correlationid' header, it shall be saved as the correlationId property on the created Message.]*/
else if (item.toLowerCase() === "iothub-correlationid") {
  msg.correlationId = response.headers[item];
}
```

Paste this block of code right after the block of code mentioned above:
```
else if (item.toLowerCase() === "iothub-app-messagetype") {
  msg.properties.add("MessageType", response.headers[item]);
}
```

