# Developer Guide


## Setting the development environment

### 1. Installations

Must have components in order to run the service and the client:

1. Visual Studio 2015 Update 3
2. Azure Service Fabric SDK - follow the intructions on [service fabric get started](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/) for installtion
3. Node.js - This is requiered for running the node.js client sample

You may also want to install additional components listed below, you will need those components for enhenced features in this repro:


2. Spec Flow - This is required for running the test project, insatall can be found [here](https://visualstudiogallery.msdn.microsoft.com/c74211e7-cb6e-4dfa-855d-df0ad4a37dd6 )
2. Windows 10 Universal Windows Platform (UWP) SDK - Requiered for running the Home Lights Sample App.

Additional recomended tools:

1. VS Code - You may want to use it for running and debugging node.js
2. [Azure IoT Hub Device Explorer](https://github.com/Azure/azure-iot-sdks/blob/master/tools/DeviceExplorer/doc/how_to_use_device_explorer.md)


### 2. Create Requiered Azure Services:

Create the following Azure resources and take a note of their connection strings:

1. Azure IoTHub 
2. Azure Storage Account (We will use Blob only)

### 3. Configuration

### 4. Running the service

Clone this repository, and compile the Visual Studio solution under Services/StateManagementService. Update the connection strings for IoTHub and storage in the StateProcessorService app.config file

The Device Silhouette is a Service Fabric application. You can run the solution on your local machine (set the StateManagementService as the startup project), using a local cluster of Service Fabric, or deploy it to Azure Service Fabric cluster.

### 5. Running the node.js sample app



## Providers (existing and extending)
## Service
## Clients
## Test 
## Sample app

