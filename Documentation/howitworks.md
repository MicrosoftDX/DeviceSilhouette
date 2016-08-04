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


## Long term persistency and analytics 
For long term access (and to allow analytics), messages are being output to external storage. Currently to blob storage, but it can be extended to a different storage type.

## Purging actor state
In addition to persistency, the Silhouette state needs to be purged periodically otherwise the performance of the system will be impacted.

This section documents the requirements for purging state from the Silhouette:

* Messages should only be purged once they are known to have been persisted to long-term storage.
* There should be at least 1 reported state retained (assuming there are any to start with). Note that this applies in v1 with the constraint that state reports should contain the full state. This requirement may not be sufficient if/when the constraint is relaxed)
* Any command request message without a response message should be retained.
* If retaining a message then any other messages with the same correlation id must be retained. This is to allow an application to correctly reason about state
* If retaining a message then all later messages must be kept. Again, this is to allow an application to correctly reason about state

We should consider having a retention period for which all messages will be retained. E.g. if the retention period is 30 minutes, then all messages in the last 30 minutes will be retained. There may well be additional retained messages based on the above rules.

Alternatively, having a minimum number of retained messages could work.

Either way, how long the retention period should be probably or how large the minimum number of messages should be will likely depend on the scenario. For scenarios with a high frequency of messages have a long period/high number probably doesn't add any value and would have a negative impact on performance. As a result, it would make sense to provide a configuration option for this.






## Web API 
## Example scenarios

