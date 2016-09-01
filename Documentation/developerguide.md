# Developer Guide


## Setting the development environment

### 1. Installations

**Must have** components in order to run the service and the client:

1. Visual Studio 2015 Update 3
2. Azure Service Fabric SDK - follow the intructions on [service fabric get started](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/) for installtion
3. Node.js - This is requiered for running the node.js client sample

Components for **enhenced features** in this repro:

1. Spec Flow - This is required for running the test project, insatall can be found [here](https://visualstudiogallery.msdn.microsoft.com/c74211e7-cb6e-4dfa-855d-df0ad4a37dd6 )
2. Windows 10 Universal Windows Platform (UWP) SDK - Requiered for running the Home Lights Sample App.

Additional **recomended tools**:

1. VS Code - You may want to use it for running and debugging node.js
2. [Azure IoT Hub Device Explorer](https://github.com/Azure/azure-iot-sdks/blob/master/tools/DeviceExplorer/doc/how_to_use_device_explorer.md)


### 2. Create Requiered Azure Services:

Create the following Azure resources and take a note of their connection strings:

1. Azure IoTHub 
2. Azure Storage Account (We will use Blob only)

### 3. Create Configuration File


All configurations in this repro are driven by environment variables. 
Detailed explanation about this approach can be found in the [Configuration](configuration.md) section.
Create MyConfig.ps1 script that sets the variables like in this example and save it:


```posh
$env:Silhouette_IotHubConnectionString="HostName=yourhub.azure-devices.net;SharedAccessKeyName=hubowner;SharedAccessKey=JHMBDjasb12masbdk1289askbsd9SjfHkJSFjqwhfqq="
$env:Silhouette_StorageConnectionString="DefaultEndpointsProtocol=https;AccountName=yourstorage;AccountKey=JkafnSADl34lNSADgd09ldsmnMASlfvmsvds9sd23dmvdsv/9dsv/sdfkjqwndssdljkvds9kjKJHhfds9Jjha=="
$env:Persistent_StorageConnectionString="DefaultEndpointsProtocol=https;AccountName=yourstorage;AccountKey=JkafnSADl34lNSADgd09ldsmnMASlfvmsvds9sd23dmvdsv/9dsv/sdfkjqwndssdljkvds9kjKJHhfds9Jjha=="
$env:Repository_MessagesRetentionMilliseconds = 120000
$env:Repository_MessagesTimerInterval=1
$env:Repository_MinMessagesToKeep=3
```

### 4. Running the service

* If you have not done yet, clone this repository to your development machine.
* Run Visual Studio as Administartor and open Services/StateManagementService/StateManagementService.sln
* Make sure StateManagementService is set as the startup project.
* In VS, open Package Manager Console and run the MyConfig.ps1 file you created. This will set the environment variables.
* Run the project by pressing Start. This will deploy the service to the locall Service Fabric Cluster.
* Once the Service deployment is completed check its avilability on: http://localhost/swagger/ui/index
* Note: you can chnage the service port by setting the endpoint in StateManagementServiceWebAPI\PackageRoot\ServiceManifest.xml
* If all run succesfuly you should be able to see the swagger UI:

![swaggerUI](images/swaggerUI1.PNG)

### 5. Running the node.js sample app



## Providers (existing and extending)
## Service
## Clients
## Test 
## Sample app

