# Test

This repository contains Visual Studio test projects for functionality and load tests.
The test projects are located under SilhouetteTests folder. There you can find the SilhouetteTests.sln with all projects.

## Silhouette.EndToEndTests

### installation

If you havent 

### Configuration

In this project the configuration is driven by environment variables, same as in other projects in this repository and documented in the [configuration](configuration.md) section.

Create MyTestConfig.PS1 configuration file as in this example:

```posh
$env:Silhouette_IotHubConnectionString="HostName=yourhub.azure-devices.net;SharedAccessKeyName=hubowner;SharedAccessKey=JHMBDjasb12masbdk1289askbsd9SjfHkJSFjqwhfqq="
$env:Silhouette_DeviceIotHubConnectionString="HostName=yourhub.azure-devices.net;DeviceId=e2eDevice1;SharedAccessKey=JHMBDjasb12masbdk1289askbsd9SjfHkJSFjqwhfqq="
```

To set the environment variables run MyTestConfig.PS1 from Package Manager Console before running the test.

### API endpoint

This project expects the services to be running locally on port 80.

### 





## Silhouette.LoadTest
