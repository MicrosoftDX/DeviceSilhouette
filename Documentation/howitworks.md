# How it works


Main concepts:

## Silhouette message

The Silhouette holds a sequence of received messages. A message can be received from external applications through the REST API or from a device through the message broker. 
Messages are being logged in the Silhouette where every message has its own version number which indicates the order the messages have been received by the Silhouette.
For details about the message schema held by the Silhouette see the [Silhouette Message](silhouettemessage.md) page.

## Messaging endpoints and lifecycle

A message sent to/from Silhouette has a specific direction, out of the two following directions:

1. Device To Cloud (D2C) - Messages sent from Device to Cloud. Messages in this direction can be from type Report or InquiryRequest.
2. Cloud To Device (C2D) - Messages sent from Cloud to Device. Messages in this direction can be from type CommandRequest.

Detailed explanation about messages lifecycle in Silhouette see [Messages Flow](messagesflow.md)


## Web API 

External applications should interact with the Silhouette through the [REST API.](RESTAPI.md)
The [REST API.](RESTAPI.md) provides the functionality to retrieve information about the most current state of the device and to send commands to the device.


## Messages purging and persistency 


For long term access, and to allow analytics, messages are being output to external storage. Currently to blob storage, but it can be extended to a different storage type.

In addition to persistency, the Silhouette state needs to be purged periodically otherwise the performance of the system will be impacted. The message retention period and the number of maximum messages to retain are configurable.




## Example scenarios

