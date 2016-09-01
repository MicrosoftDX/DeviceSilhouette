# Device Silhouette


##About

Device Silhouette is an open source framework developed by TED GE EMEA team in Microsoft.

The Device Silhouette is a solution for managing IoT devices state in the cloud. The "Silhouette" is virtual version of the device that includes the device’s latest state. Applications can interact with the Silhouette even when the device is offline. The Device Silhouettes persist the last reported state and desired future state. You can retrieve the last reported state of a device or set a desired future state through a Rest API.

The Silhouette holds information about all state messages sent and received to/from the device, including delivery status updates. With the Silhouette you can send desired state to the device, set a TTL for the request and check if the message was delivered successfully or not. Every message sent increase the version of the Silhouette state and you can retrieve a short term history of all messages, this functionality enables building a rule engine, state machine and solve conflicts.

## Documentation Index

2.	[Overview](Documentation/overview.md)
 * Introduction - What is Device Silhouette? 
 * Features 
 *  Benefits
 * Scenarios: [Home lightning](Documentation/lightsSampleScenario.md), [Oven maintenance](Documentation/ovenscenario.md)
3.	[How it works?](Documentation/howitworks.md)
 * [Silhouette Message](Documentation/silhouettemessage.md)
 * Messaging endpoints and lifecycle
 * [Messages flow](Documentation/messagesflow.md)
 * Messages purging and persistancy 
 * [REST API](Documentation/RESTAPI.md) 
 * Example scenarios
4.	[Architecture](Documentation/architecture.md)
 * General architecture
 * Service Fabric Services
5.	[Developer guide](Documentation/developerguide.md)
 * Providers (existing and extending)
 * Setting the dev environment
 * [Configuration](Documentation/configuration.md)
 * Service
 * Clients
 * Test 
 * Sample app
6.	[Deploy to production](Documentation/deployment.md)
 * IoT Hub
 * Blob Storage
 * Azure AD
 * Service fabric
7.	[Security](Documentation/security.md)
 * Devices security
 * Service Fabric Security
 * REST API Authentication and Authorization
8.	[Related services](Documentation/relatedservices.md)
 * Azure IoT Hub
 * Azure Service Fabric
 * Azure Blob Storage
 * Azure Active Directory


## Reporting issues and feedback

If you encounter any bugs with the tool please file an issue in the Issues section of our GitHub repo.

## Contribute Code

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.


## License

MIT
