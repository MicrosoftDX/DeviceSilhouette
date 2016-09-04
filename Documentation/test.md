# Test

This repository contains Visual Studio test projects for functionality and load tests.
The test projects are located under SilhouetteTests folder. There you can find the SilhouetteTests.sln with all projects.

## Silhouette.EndToEndTests

### Configuration

In this project the configuration is driven by environment variables, same as in other projects in this repository and documented in the [configuration](configuration.md) section.

```posh
$env:Silhouette_IotHubConnectionString="HostName=yourhub.azure-devices.net;SharedAccessKeyName=hubowner;SharedAccessKey=JHMBDjasb12masbdk1289askbsd9SjfHkJSFjqwhfqq="
$env:Silhouette_DeviceIotHubConnectionString="HostName=yourhub.azure-devices.net;DeviceId=e2eDevice1;SharedAccessKey=JHMBDjasb12masbdk1289askbsd9SjfHkJSFjqwhfqq="
```







## Silhouette.LoadTest
