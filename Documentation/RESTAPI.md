# Device Silhouette REST API


## API Endpoint overview

The top-level endpoints for a device are:
* messages - this is the API representation of the messages that flow through Silhouette
* commands - this is a subset of messages (CommandRequest and CommandResponse), and the entity body is collapsed to return both aspects in the same entity for easier consumption by the application 
* state - this allows the state to be retrieved in different forms (e.g. the last reported state, or states computed based on commands to set the state)

The table below lists the API endpoints. Note that all the Urls below are prefixed with `/devices/{deviceId}`

Method | Url                            | Notes
-------|--------------------------------|----------------|
`GET`  | `/`                            | Should this be an endpoint? What should it return?
`GET`  | `/commands`                    | Retrieve a list of commands. <br />Results are paged. <br/>See [commands body](#get-commands-response)
`POST` | `/commands`                    | Add a new command to send <br/>    command/status (support partial?)<br/>cloud metadata<br/>   Response<br/>* 202 Accepted??? - link to command with id/seq#/…<br /> Filtering? e.g. type=state request/deepget
`GET`  | `/commands/{commandId}`        | Get the details for the command with id `commandId`  <br/>See [command body](#get-commandscommandid-response) below
`GET`  | `/messages`                    | Retrieve a list of messages. Q: do we need to support filtering?  <br />Results are paged. <br/>See [messages body](#get-messages-response)
`GET`  | `/messages/{messageId}`        | Get details for the message with id `messageId`<br /> See [message body](#get-messagesmessageid-response) below
`GET`  | `/state/latest-reported`       | Return the latest state reported by the device
`GET`  | `/state/latest-acknowledged`   | **Not in version 1**  (requires state merging to be resolved<br />Return the latest state based on the state reported by the device and requested through commands that have been acknowledged (ACK'd) by the device
`GET`  | `/state/latest-requested`      | Return the latest state based on the state reported by the device and requested through any commands that haven't expired

## Message bodies

### GET /commands/{commandId} response
A command joins the messages for a command request and the command response to make it simpler to work with

```json
{
  "id" : "d4eae849-85a5-4988-8899-14b1ec2491f1",
  "deviceId": "devicexyz",
  "request" : {
    "version" : 123,
    "timestamp" : "2016-07-20T11:30:00Z",
    "type" : "CommandRequest",
    "subtype" : "SetState",
    "appMetadata": {
      "custom-app-stuff" : "goes here"
    },
    "values": {
      "light" : "on"
    }
  },
  "response" : {
    "version" : 127,
    "timestamp" : "2016-07-20T11:35:00Z",
    "type" : "CommandResponse",
    "subtype" : "ACK"
  }
}
```

As an implementation note, `id` is the correlation in the messages for the command

***TODO - add links to the message request, message response, and self link 

### GET /commands response
The commands response uses the paging approach - see [Paging](#paging)


```json
{
  "values" : [
    {
      "id" : "d4eae849-85a5-4988-8899-14b1ec2491f1",
      "rest of command here" :null  
    },
    ...
  ],
  "@nextLink": "{url to retrieve next page of items}" 
}
```

### GET /messages/{messageId} response
A message entity in the API corresponds to an individual message sent between client and device

The message body will vary depending on the type of the message

**CommandRequest**

```json
{
	"deviceId": "devicexyz",
	"version" : 123,
	"correlationId": "d4eae849-85a5-4988-8899-14b1ec2491f1",
	"timestamp" : "2016-07-20T11:30:00Z",
	"type" : "CommandRequest",
	"subtype" : "SetState",
	"appMetadata": {
		"custom-app-stuff" : "goes here"
	},
	"values": {
		"light" : "on"
	}
}
```

**CommandResponse**

```json
{
	"deviceId": "devicexyz",
	"version" : 127,
	"correlationId": "d4eae849-85a5-4988-8899-14b1ec2491f1",
	"timestamp" : "2016-07-20T11:30:00Z",
	"type" : "Response",
	"subtype" : "ACK"
}
```

**Device state report**

```json
{
	"deviceId": "devicexyz",
	"version" : 129,
	"correlationId": "74a52a39-83ab-4da6-b0ca-2318d55d277f",
	"timestamp" : "2016-07-20T11:36:00Z",
	"type" : "Report",
	"subtype" : "State",
	"values": {
		"light" : "on"
	}
}
```

***TODO - add links to the message request, message response, and self link 

### GET /messages response
The messages response uses the paging approach - see [Paging](#paging)


```json
{
  "values" : [
    {
      "deviceId" : "d4eae849-85a5-4988-8899-14b1ec2491f1",
      "messageType" : "CommandRequest",
      "version": 456,
      "rest of message here" :null  
    },
    ...
  ],
  "@nextLink": "{url to retrieve next page of items}" 
}
```

* outline the api calls that support the scenarios

### Error responses

Suggested format for error messages.

We should define the status values, and list the HTTP status that they map to (e.g. 404 could be logical for invalid device id)

```json
{
    "code" : "invalid-device-id",
    "message" : "The device id 'device123' is not registered with IOT Hub",
    "trackingId" : "<log tracking Id - used to locate error in log>"
}
```

### Paging

In the initial implementation, page sizes will be controlled by the server. 

For paged results, the result format is illustrated below

```json
{
  "values" : [
    { },
    { },
    { },
    ...
  ],
  "@nextLink": "{url to retrieve next page of items}" 
}
```

The actual results are returned in the `values` array. The `@nextLink` is the continuation token is the URL to retrieve the next page of restuls. Consumers must treat this as a opaque string.


Based on [Microsoft API Guidelines](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#98-pagination)

Thoughts for future
* think about allowing the client to set a desired page sizes
* think about which paged results should allow filtering, and what that filtering should be


## TODO - add notes on state merging
* v1 assumptions (plan is to look at addressing in future versions): 
  * state being requested is full state
  * state updates from the client will report the full state 
* impact of these relaxing these assumptions is that the versioning cannot be applied to the full state record above



