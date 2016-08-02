# Device Silhouette


##About

Device Silhouette is an open source framework developed by TED GE EMEA team in Microsoft.

The Device Silhouette is a solution for managing IoT devices state in the cloud. The "Silhouette" is virtual version of the device that includes the device’s latest state. Applications can interact with the Silhouette even when the device is offline. The Device Silhouettes persist the last reported state and desired future state. You can retrieve the last reported state of a device or set a desired future state through a Rest API.

The Silhouette holds information about all state messages sent and received to/from the device, including delivery status updates. With the Silhouette you can send desired state to the device, set a TTL for the request and check if the message was delivered successfully or not. Every message sent increase the version of the Silhouette state and you can retrieve a short term history of all messages, this functionality enables building a rule engine, state machine and solve conflicts.

## Documentation 

1. Overview
2. How it works
3. Architecture
4. Developer Guide
5. Deplyment
6. Security
7. Related Services


## Reporting issues and feedback

If you encounter any bugs with the tool please file an issue in the Issues section of our GitHub repo.

## Contribute Code

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.


## License

MIT
