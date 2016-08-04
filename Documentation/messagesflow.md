### C2D Update State

**Direction:** C2D

**MessageType:** "CommandRequest"

**MessageSubType:** "SetState"

**Flow:**


(1) "State REST API" receive a message to update the device state (POST API call). The call should include a parameter for command TTL in milliseconds.

(2) "State REST API" sends the call to "State Processor service", including the command TTL property.

(3) "State Processor service" writes a message to "Device State Repository" with a new correlationID, Type=CommandRequest, Subtype=SetState.

(4) "State Processor service" keep try to put a message in "Send C2D" endpoint while the command TTL is not expired. MessageType=CommandRequest, MessageSubtype=SetState

(4.1) "Feedback service" check for ACK and NACK from the transport layer.

(4.2) Upon receiving ACK message update the "State Repository" with a new message.Same correlationID as in (3), Type=CommandResponse, Subtype=Acknowledged.

(4.3) Upon receiving NACK message retry and send the message again, update the "State Repository" with a new message. Same correlationID as in (3), Type=CommandResponse, Subtype=NotAcknowledged.

(4.4) When command TTL expires. If no ACK received, stop retrying and set "State Repository" with a new message. Same correlationID as in (3). Type=CommandResponse, Subtype=Expired.
 
(5) Message received by device through the "Receive C2D" endpoint

(6) Device change its state

(7) Device write a message with messageType=Report and MessageSubType=State  in "Send D2C" endpoint.

(8) Message received on the "Receive D2C" endpoint

(9) "State Processor service" picks the message from "Receive D2C" endpoint

(10) "State Processor service" writes the state to "Device State Repository". New CorreleationID. Type=Report, Subtype=State.


**Repository Example:**
```
{

"deviceId": "device1",

"timestamp": "2016-07-20T10:31:03.5469371Z",

"version": 269,

"correlationId": "2341b061-ae03-4547-bc1c-ac83b1662f31",

"Type": "CommandRequest",

"Subtype": "SetState",

"appMetadata": "{"Origin": "api"}",

"values": "{"Xaxis": 0,"Yaxis": 20,"Zaxis": 0}"

}

{

"deviceId": "device1",

"timestamp": "2016-07-20T10:31:12.8988807Z",

"version": 270,

"correlationId": "2341b061-ae03-4547-bc1c-ac83b1662f31",

"Type": "CommandResponse",

"Subtype": "Expired",

"appMetadata": "",

"values": ""

}
```


### C2D Get State

**Direction:** C2D

**MessageType:** "CommandRequest"

**MessageSubType:** "ReportState"

**Flow:**

(1) "State Processor service" writes a message to "Device State Repository" with a new correlationID, MessageType=CommandRequest, MessageSubtype=ReportState.

(2) "State Processor service" keep try to put message in "Send C2D" endpoint while the command TTL is not expired. MessageType=CommandRequest, MessageSubtype=ReportState.

(3.1) "Feedback service" check for ACK and NACK from the transport layer.

(3.2) Upon receiving ACK message update the "State Repository" with a new message. Same correlationID as in (1), MessageType=CommandResponse, MessageSubtype=Acknowledged.

(3.3) Upon receiving NACK message retry and send the message again, update the "State Repository" with a new message.Same correlationID as in (1), MessageType=CommandResponse, MessageSubtype=NotAcknowledged.

(3.4) When command TTL expires. If no ACK received, stop retrying and set State Repository" with a new message. Same correlationID as in (1), MessageType=CommandResponse, MessageSubtype=Exired.

(4) Message received by device through the "Receive C2D" endpoint.

(5) Device updates the "State Processor service" by sending a message through "Send D2C" endpoint with Update State message. MessageType=Report and MessageSubType=State.  

(6) Message received on the "Receive D2C" endpoint

(7) "State Processor service" picks the message from "Receive D2C" endpoint

(8) "State Processor service" writes a new message to "Device State Repository". New CorreleationID. MessageType=Report, MessageSubtype=State.




**Example:**


```
{

"deviceId": "device1",

"timestamp": "2016-07-20T10:51:03.5469371Z",

"version": 300,

"correlationId": "2341b061-ae03-4547-bc1c-ac83b1665555",

"Type": "CommandRequest",

"Subtype": "ReportState",

"appMetadata": "{"Origin": "service"}",

"values": ""

}

{

"deviceId": "device1",

"timestamp": "2016-07-20T10:51:08.5469371Z",

"version": 301,

"correlationId": "2341b061-ae03-4547-bc1c-ac83b1665555",

"Type": "CommandResponse",

"Subtype": "Acknowledged",

"appMetadata": "",

"values": ""

}
```

### D2C Get State

**Direction:** D2C 

**MessageType:** "InquiryRequest"

**MessageSubType:** "GetState"

**Flow:**

(1) Device write a message from type in "Send D2C" endpoint. MessageType=InquiryRequest and MessageSubType=GetState  

(2) Message received on the "Receive D2C" endpoint

(3) State Processor service picks the message from "Receive D2C" endpoint

(4) "State Processor service" writes a new message to "Device State Repository". New CorreleationID. MessageType=InquiryRequest, MessageSubType=GetState.

(5) "State Processor Service" query the last known state from "Device State Repository"

(6) "State Processor service" updates the device by sending a message to the device through "Send C2D" endpoint with Update State message 


**Repository Example:**

```
{

"deviceId": "device1",

"timestamp": "2016-07-20T11:31:08.5469371Z",

"version": 355,

"correlationId": "2341b061-ae03-4547-bc1c-ac83b1667777",

"Type": "InquiryRequest",

"Subtype": "GetState",

"appMetadata": "{"Origin":"Device"}",

"values": ""

}
```


### D2C Set State

**Direction:** D2C 

**MessageType:** "Report"

**MessageSubType:** "State"

**Flow:**

(1) Device write a message in "Send D2C" endpoint. Message body contains the device current state. MessageType=Report, MessageSubType=State.

(2) Message received on the "Receive D2C" endpoint

(3) "State Processor service" picks the message from "Receive D2C" endpoint.

(4) "State Processor service" writes a new message to "Device State Repository". New CorreleationID. MessageType=Report, MessageSubType=State.


**Repository Example:**

```
{

"deviceId": "device1",

"timestamp": "2016-07-20T12:31:03.5469371Z",

"version": 444,

"correlationId": "2341b061-ae03-4547-bc1c-ac83b1668888",

"Type": "Report",

"Subtype": "State",

"appMetadata": "{"Origin": "Device"}",

"values": "{"Xaxis": 0,"Yaxis": 0,"Zaxis": 0}"

}
```







