# Device Silhouette

This project creates a virtual version or actor (“silhouette”) of each device in the cloud that includes the device’s latest state so that applications interact with the device even when the device is offline. The Device Silhouettes persist the last reported state and desired future state. You can retrieve the last reported state of a device or set a desired future state through a Rest API or using one of the Client SDKs

![Architecture](https://github.com/dx-ted-emea/pudding/blob/master/images/general-architecture4.gif?raw=true)

## Setting up the project
There are 2 channels to communicate with the Device Silhouette:

1. Direct REST API
2. Messaging Communication Provider -currently implemented only IoTHub.

Setup the following Azure Services and take a note of their connection string:
1. Azure IoTHub
2. Azure Storage Account

Clone this repository, and compile the Visual Studio solution under [Services/StateManagementService](Services/StateManagementService). 
Update the connection strings for IoTHub and storage in the StateProcessorService app.config file

The Device Silhouette is a Service Fabric application. You can run the solution on your local machine (set the StateManagementService as the startup project), using a local cluster of Service Fabric, or deploy it to Azure Service Fabric cluster. 

[Prepare your development environment](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/)
 
## Testing IoTHub communicator using a node.js client
Add a device to the IoTHub and take a note of its name and connection string. This can be done using [Device Explorer](https://github.com/Azure/azure-iot-sdks/releases/download/2016-02-03/SetupDeviceExplorer.msi).

The node.js client is located under [src/client/node](src/client/node). Edit the files:

- sample_client_new.js
- silhouette-client-iothub-new.js

And replace connectionString with the device connection string, and DeviceID with the device name.   

Run sample_client_new.js - it will send messages to IoTHub Device2Cloud endpoint for Device Silhouette to capture and process. The same client will read messages from the Cloud2Device endpoint. 

```node
node sample_client_new.js
```

## Testing REST APIs using Swagger
The visual studio solution contains a swagger UI for the REST APIs. 

If run locally, can be accessed via [http://localhost:9013/swagger/ui/index](http://localhost:9013/swagger/ui/index). Alternatively, if a Service Fabric cluster is deployed in Azure, the swagger UI can be accessed at [<cluster url>:9013/swagger/ui/index](<cluster url>:9013/swagger/ui/index).




