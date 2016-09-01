# Developer Guide


## Setting the development environment

Development Environment:
Visual Studio 2015 Update 3
Azure Service Fabric SDK - follow the intructions on !(service fabric get started](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/) for installtion

https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/


Requiered Azure Services:
To run the service in a development environment you will first need to create the following Azure resources, setup those resources and take a note of their connection strings:

1. Azure IoTHub 
2. Azure Storage Account (We will use Blob only)

Clone this repository, and compile the Visual Studio solution under Services/StateManagementService. Update the connection strings for IoTHub and storage in the StateProcessorService app.config file

The Device Silhouette is a Service Fabric application. You can run the solution on your local machine (set the StateManagementService as the startup project), using a local cluster of Service Fabric, or deploy it to Azure Service Fabric cluster. 



## Providers (existing and extending)
## Service
## Clients
## Test 
## Sample app

