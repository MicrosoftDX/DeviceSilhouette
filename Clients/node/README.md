# Node.JS Client Sample

This is some Node.JS pseudo code to shape the client side.

The idea is that you can have different transports (e.g. IoT Hub, MQTT) and the client SDK will hide the implement details.

You only have to implement two callback (get/update state from cloud to device) and you get two functions to get/update state from device to cloud.

The state itself is just a JavaScript object, e.g. a JSON object.


### Important Note - update node module:
Node.JS SDK have not yet implemented system properties properly in the SDK. Below is a workaround in order to be able to access the MessageType.

Modify the file node_modules\azure-iot-device-http\node_modules\azure-iot-http-base\lib\http.js as follows:

Find this block of code:
```javascript
/*Codes_SRS_NODE_HTTP_05_010: [If the HTTP response has an 'iothub-correlationid' header, it shall be saved as the correlationId property on the created Message.]*/
else if (item.toLowerCase() === "iothub-correlationid") {
  msg.correlationId = response.headers[item];
}
```

Paste this block of code right after the block of code mentioned above:
```javascript
  else if (item.search("iothub-app-") === 0) { // starts with iothub-app-
      var propertyName = item.substring("iothub-app-".length);
      msg.properties.add(propertyName, response.headers[item]);
  }
}
```

For more details, see [azure-iot-sdks #414 - EventData Properties dictionary empty ](https://github.com/Azure/azure-iot-sdks/issues/414)

### Environment variables



Same as the service, the config for sample_client_simple.js and sample_client_interactive.js use the environment variables.
This client expect to find environment variable for "Silhouette_DeviceIotHubConnectionString".
This should be the IoTHub connection string for device with ID "device1" for sample_client_simple.js and "device42" sample_client_interactive.js.

The connection string expected format is:
```
HostName=<IoTHubName>.azure-devices.net;DeviceId=device1;SharedAccessKey=<The_Device_SAS_Token_for_IoTHub>
```


When working with VSCODE, you can set the environment variables in the launch.json under "env"

```javascript
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Launch",
            "type": "node",
            "request": "launch",
            "program": "${workspaceRoot}/sample_client_interactive.js",
            "stopOnEntry": false,
            "args": [],
            "cwd": "${workspaceRoot}",
            "preLaunchTask": null,
            "runtimeExecutable": null,
            "runtimeArgs": [
                "--nolazy"
            ],
            "env": {
                "NODE_ENV": "development",
                "Silhouette_DeviceIotHubConnectionString": "HostName=<IoTHubName>.azure-devices.net;DeviceId=device1;SharedAccessKey=<The_Device_SAS_Token_for_IoTHub>"
            },
            "externalConsole": false,
            "sourceMaps": false,
            "outDir": null
        },
        {
            "name": "Attach",
            "type": "node",
            "request": "attach",
            "port": 5858,
            "address": "localhost",
            "restart": false,
            "sourceMaps": false,
            "outDir": null,
            "localRoot": "${workspaceRoot}",
            "remoteRoot": null
        }
    ]
}
```



