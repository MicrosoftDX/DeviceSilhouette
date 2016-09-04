# Deploy to production

## Create requiered resorces in Azure

In order to deploy Device Silhouette to production, you will have to create the resources in the list below. Follow the links provided and create those resources.
Make sure to create all resources in the same location.

1. IoT Hub - [Get started with Azure IoT Hub ](https://azure.microsoft.com/en-us/documentation/articles/iot-hub-csharp-csharp-getstarted/)
2. Blob Storage - [About Azure storage accounts](https://azure.microsoft.com/en-us/documentation/articles/storage-create-storage-account/)
3. Service Fabric Cluster, Azure Active Directory and Azure Key Vault - [Create a Service Fabric cluster in Azure](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-cluster-creation-via-portal/)

## Create configuration file

Create MyCloudConfig.ps1 configuration file for StateManagementService, same as you created for running locally, this time with the connection strings and properties for production.
For more details see the [configuration](configuration.md) section.

```posh
$env:Silhouette_IotHubConnectionString="HostName=yourhub.azure-devices.net;SharedAccessKeyName=hubowner;SharedAccessKey=JHMBDjasb12masbdk1289askbsd9SjfHkJSFjqwhfqq="
$env:Silhouette_StorageConnectionString="DefaultEndpointsProtocol=https;AccountName=yourstorage;AccountKey=JkafnSADl34lNSADgd09ldsmnMASlfvmsvds9sd23dmvdsv/9dsv/sdfkjqwndssdljkvds9kjKJHhfds9Jjha=="
$env:Persistent_StorageConnectionString="DefaultEndpointsProtocol=https;AccountName=yourstorage;AccountKey=JkafnSADl34lNSADgd09ldsmnMASlfvmsvds9sd23dmvdsv/9dsv/sdfkjqwndssdljkvds9kjKJHhfds9Jjha=="
$env:Repository_MessagesRetentionMilliseconds = 120000
$env:Repository_MessagesTimerInterval=1
$env:Repository_MinMessagesToKeep=3
```

## Deploy to service fabric

### Deploy through Visual Studio:

1. Run the MyCloudConfig.ps1 script in the Package Manager Console before running the Publish.
2. Deploy by the instructions in [Publish an application to a remote cluster by using Visual Studio](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-publish-app-remote-cluster/)

Deploying from PowerShell:
https://azure.microsoft.com/en-us/documentation/articles/service-fabric-deploy-remove-applications/

If you are deploying from PowerShell then simply run the MyCloudConfig.ps1 script before runnigng the Deploy-FabricApplication.ps1 script.




