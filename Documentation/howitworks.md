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
## Web API 
## Example scenarios

