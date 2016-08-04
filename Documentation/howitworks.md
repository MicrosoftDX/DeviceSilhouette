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






## Messages purging and persistancy 


 - Long term persistency and analytics 
For long term access, and to allow analytics, messages are being output to external storage. Currently to blob storage, but it can be extended to a different storage type.

- Purging the Silhouette state:
In addition to persistency, the Silhouette state needs to be purged periodically otherwise the performance of the system will be impacted.

## Purging actor state
In addition to persistency, the Silhouette state needs to be purged periodically otherwise the performance of the system will be impacted. Messages are being only after  they are known to have been persisted to long-term storage.

Alternatively, having a minimum number of retained messages could work.

Either way, how long the retention period should be probably or how large the minimum number of messages should be will likely depend on the scenario. For scenarios with a high frequency of messages have a long period/high number probably doesn't add any value and would have a negative impact on performance. As a result, it would make sense to provide a configuration option for this.






## Web API 
## Example scenarios

