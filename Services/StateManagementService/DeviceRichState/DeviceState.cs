using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeviceRichState
{
    [DataContract]
    public class DeviceMessage
    {
        // NOTE - when adding new properties, also add the related tests in DeviceMessage_SerializationTests

        [DataMember]
        private string _deviceId;
        [DataMember]
        private string _correlationId;
        [DataMember]
        internal DateTime _timestamp;
        [DataMember]
        private MessageType _messageType;

        /// <summary>
        /// Can only be set at DeviceMessage instantiation
        /// </summary>
        [DataMember]
        public string DeviceId { get { return _deviceId; } set {; } }

        /// <summary>
        /// Timestamp is UTC time, set automatically on creation
        /// </summary>
        [DataMember]
        public DateTime Timestamp { get { return _timestamp; } set {; } }

        /// <summary>
        /// Version is set by silhouette actor, auto increment
        /// </summary>
        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public string CorrelationId { get { return _correlationId; } set {; } }

        [DataMember]
        public MessageType MessageType { get { return _messageType; } set {; } }

        [DataMember]
        public MessageSubType MessageSubType { get; set; }

        /// <summary>
        /// Part of the state message that contains Application specific data
        /// </summary>
        [DataMember]
        public string AppMetadata { get; set; }

        public bool Persisted { get; set; }

        /// <summary>
        /// Part of the state message that contains device metrics
        /// </summary>
        [DataMember]
        public string Values { get; set; }
        /// <summary>
        /// The TTL for the message in milliseconds
        /// </summary>
        public long MessageTtlMs { get; set; }

        public DeviceMessage()
        {
            // empty constructor for serialization
        }

        /// <summary>
        /// Holds the RichState of a device based on the state message and 
        /// </summary>
        /// <param name="deviceId">Unique indentifier of the device</param>
        /// <param name="metadata">Application metadata</param>   
        /// <param name="values">Actual data recevied from the device</param>
        /// <param name="messageType">Who send the message; reported == device, requested == application</param>
        /// <param name="messageStatus">Indication of the status of this message instance</param>
        /// /// <param name="correlationId">Message id</param>
        public DeviceMessage(string deviceId, string metadata, string values, MessageType messageType, MessageSubType messageSubType, string correlationId = null)
        {
            // To make these values immutable they are set through private field and get through public property
            // It is not possible to make the setter readonly because of [DataMember]
            _deviceId = deviceId;
            _timestamp = SystemTime.UtcNow();
            _messageType = messageType;

            _correlationId = String.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;
            MessageSubType = messageSubType;

            AppMetadata = metadata;
            Values = values;
        }
    }

    public enum MessageType
    {

        /// <summary>
        /// App requesting device to do something.
        /// Currently change state or report state. Could be extensible for future.
        /// </summary>
        CommandRequest,

        /// <summary>
        /// Used for ACK/NAK etc responses to a CommandRequest
        /// </summary>
        CommandResponse,

        /// <summary>
        /// Messages received from the device to report it's state. Can also referred to telemetry
        /// </summary>
        Report,

        /// <summary>
        /// Device requesting to get it's last known state. May be invoked by the device, for example, after being offline for a while.
        /// </summary>
        InquiryRequest,

        /// <summary>
        /// Used for ACK/NAK etc responses to a InquiryRequest
        /// </summary>
        InquiryResponse

    }

    public enum MessageSubType
    {
        /// <summary>
        /// CommandRequest: body contains desired state for the device
        /// </summary>
        SetState,
        /// <summary>
        /// CommandRequest: request for the device to report its state
        /// </summary>
        ReportState,

        New,
        /// <summary>
        /// CommandResponse/InquiryResponse: Device sent ACK
        /// </summary>
        Acknowledged,
        Enqueued,
        /// <summary>
        /// CommandResponse/InquiryResponse: the request message was not delivered before the message TTL
        /// </summary>
        Expired,
        /// <summary>
        /// CommandResponse/InquiryResponse: Device sent NAK
        /// </summary>
        NotAcknowledged,
        /// <summary>
        /// CommandResponse/InquiryResponse: the number of delivery attempts for the request message exceeded the retry count
        /// </summary>
        ExceededRetryCount,
        Received,

        /// <summary>
        /// Report: The device is reporting its state
        /// </summary>
        State,


        /// <summary>
        /// InquiryRequest: The device is requesting its last state
        /// </summary>
        GetState,

    }
}
