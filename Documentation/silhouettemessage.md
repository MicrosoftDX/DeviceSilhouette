# Silhouette Message


The Silhouette holds a sequence of received messages. A message can be received from external applications through the REST API or from a device through the message broker. 

This page defines the message properties and schema.
 

In general, the message is comprised by:

 - Silhouette defined properties
 - App or service custom metadata
 - Device values

The messages are JSON objects that are modelled in the [DeviceState](https://github.com/dx-ted-emea/pudding/wiki/7.1.1.-Device-State-Object). The three sections above will be named properties, whose values are in turn of type object.

### Silhouette Defined Properties
These are required properties set by silhouette:

 - DeviceId
 - Version (SequenceNumber)
 - CorrelationId
 - Timestamp
 - Type 
 - Subtype


Silhouette properties are immutable, they are set when creating a new DeviceState instance and can only be read.

The correlation id will be used to associate messages for a single state request. When an ACK/NACK is send for a requested state we will create a new message with the same correlation id but different timestamp.

#### Types and subtypes:


| Type | Type explanation | Possible Subtypes | 
|-----|-----|-------|
| CommandRequest  | App requesting device to do something.<br>Currently change state or report state.<br> Could be extensible for future. | SetState<br>ReportState<br/>LatestState |
| CommandResponse  | Used for ACK/NAK etc responses to a CommandRequest | Acknowledged<br>Enqueued<br>Expired<br>NotAcknowledged<br>ExceededRetryCount<br>Received|
| Report  | Messages received from the device to report it's state.<br>Can also referred to telemetry| State |
| InquiryRequest  | Device requesting some information.<br>For example, if the device has been offline for a while and wants to know its last reported state then it can send a subtype of `GetState` which will cause Silhouette to send a `CommandRequest` with subtype `LatestState` and the latest state in the body  | GetState|




### App Custom Metadata
These are properties that only have a meaning within the context of the specific application. They can be any key:value pair. Some suggested pairs:
 - Origin
   - What is the source of the command to a device

### Values
These are the values sent by a device or values sent to the device. They can be any key:value pair.


### Message structure
the structure of the Json message is as follows

```json
{
	"silhouetteProperties": {
		"deviceId": "",
		"version": "",
		"correlationId": "",
		"timestamp": "",
		"type": "",
		"subtype": ""
	},
	"appMetadata": {},
	"Values": {}
}
```
