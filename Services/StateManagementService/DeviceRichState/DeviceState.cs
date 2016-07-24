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
    public class DeviceState
    {
        // NOTE - when adding new properties, also add the related tests in DeviceState_SerializationTests

        [DataMember]
        private string _deviceId;
        [DataMember]
        private string _correlationId;
        [DataMember]
        internal DateTime _timestamp;
        [DataMember]
        private MessageType _messageType;

        /// <summary>
        /// Can only be set at DeviceState instantiation
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
        
        public DeviceState()
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
        public DeviceState(string deviceId, string metadata, string values, MessageType messageType, MessageSubType messageSubType = MessageSubType.Unknown, string correlationId = null)
        {
            // To make these values immutable they are set through private field and get through public property
            // It is not possible to make the setter readonly because of [DataMember]
            _deviceId = deviceId;
            _timestamp = SystemTime.UtcNow();
            _messageType = messageType;

            if (messageSubType == MessageSubType.Unknown)
            {
                _correlationId = Guid.NewGuid().ToString();

                if (messageType == MessageType.Report)
                    MessageSubType = MessageSubType.State;

                if (messageType == MessageType.CommandRequest)
                    MessageSubType = MessageSubType.New;
            }
            else
            {
                _correlationId = String.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;
                MessageSubType = messageSubType;
            }

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
        /// Subtypes for CommandRequest
        /// </summary>
        SetState,
        ReportState,

        /// <summary>
        /// Subtypes for CommandResponse and InquiryResponse
        /// </summary>
        New,
        Unknown,
        Acknowledged,
        Enqueued,
        Expired,
        NotAcknowledged,
        ExceededRetryCount,
        Received,

        /// <summary>
        /// Subtypes for Report
        /// </summary>
        State,

        /// <summary>
        /// Subtypes for InquiryRequest
        /// </summary>
        GetState,


    }

    //public enum MessageStatus
    //{
    //    Acknowledged,
    //    Expired,
    //    DeliveryCountExceeded,
    //    NotAcknowledged,
    //    Enqueued,
    //    New,
    //    Received,
    //    Unknown
    //}
}
