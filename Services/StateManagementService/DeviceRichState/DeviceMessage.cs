using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CommonUtils;

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
        [DataMember]
        private string _messageSubType;
        [DataMember]
        private string _appMetadata;
        [DataMember]
        private string _values;
        [DataMember]
        private long _messageTtlMs;


        // <summary>
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
        public string MessageSubType { get { return _messageSubType; } set {; } }

        /// <summary>
        /// Part of the state message that contains Application specific data
        /// </summary>
        [DataMember]
        public string AppMetadata { get { return _appMetadata; } set {; } }

        /// <summary>
        /// Part of the state message that contains device metrics
        /// </summary>
        [DataMember]
        public string Values { get { return _values; } set {; } }

        [DataMember]
        /// <summary>
        /// The TTL for the message in milliseconds
        /// </summary>
        public long MessageTtlMs { get { return _messageTtlMs; } set {; } }



        [DataMember]
        public bool Persisted { get; set; }

        public DeviceMessage()
        {
            // empty constructor for serialization
        }

        /// <summary>
        /// DO NOT USE THIS CONSTRUCTOR. Instead, use the static CreateXXX methods
        /// </summary>
        /// <param name="deviceId">Unique indentifier of the device</param>
        /// <param name="metadata">Application metadata</param>   
        /// <param name="values">Actual data recevied from the device</param>
        /// <param name="messageType">Who send the message; reported == device, requested == application</param>
        /// <param name="messageStatus">Indication of the status of this message instance</param>
        /// /// <param name="correlationId">Message id</param>
        private DeviceMessage(
            string deviceId,
            string metadata,
            string values,
            MessageType messageType,
            string messageSubType,
            long messageTtlMs,
            string correlationId = null,
            DateTime? timestamp = null)
        {
            // To make these values immutable they are set through private field and get through public property
            // It is not possible to make the setter readonly because of [DataMember]
            _deviceId = deviceId;
            _appMetadata = metadata;
            _values = values;

            _messageType = messageType;
            _messageSubType = messageSubType;

            _messageTtlMs = messageTtlMs;

            _correlationId = String.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;

            _timestamp = timestamp ?? SystemTime.UtcNow();
        }

        public static DeviceMessage CreateCommandRequest(string deviceId,
                string metadata,
                string values,
                CommandRequestMessageSubType messageSubType,
                long messageTtlMs,
                string correlationId = null,
                DateTime? timestamp = null)
        {
            return new DeviceMessage(
                deviceId,
                metadata,
                values,
                MessageType.CommandRequest,
                messageSubType.ToString(),
                messageTtlMs,
                correlationId,
                timestamp
                );
        }
        public static DeviceMessage CreateCommandResponse(string deviceId,
          string metadata,
          string values,
          CommandResponseMessageSubType messageSubType,
          long messageTtlMs,
          string correlationId = null,
          DateTime? timestamp = null)
        {
            return new DeviceMessage(
                deviceId,
                metadata,
                values,
                MessageType.CommandResponse,
                messageSubType.ToString(),
                messageTtlMs,
                correlationId,
                timestamp
                );
        }
        public static DeviceMessage CreateReport(string deviceId,
            string values,
            ReportMessageSubType messageSubType,
            string correlationId = null,
            DateTime? timestamp = null)
        {
            return new DeviceMessage(
                deviceId,
                null, // don't have app metadata in Report
                values,
                MessageType.Report,
                messageSubType.ToString(),
                -1, // don't have TTL in Report
                correlationId,
                timestamp
                );
        }
        public static DeviceMessage CreateInquiry(string deviceId,
            string values,
            InquiryMessageSubType messageSubType,
            string correlationId = null,
            DateTime? timestamp = null)
        {
            return new DeviceMessage(
                deviceId,
                null, // don't have app metadata in Inquiry
                values,
                MessageType.Inquiry,
                messageSubType.ToString(),
                -1, // don't have ttl in Inquiry
                correlationId,
                timestamp
                );
        }

        // Do these helpers belong here or in extension methods?
        public ReportMessageSubType ReportMessageSubType()
        {
            if (MessageType != MessageType.Report)
            {
                CommonUtils.SilhouetteEventSource.Current.LogException($"Can only call {nameof(ReportMessageSubType)} when MessageType is {nameof(MessageType.Report)}");
                throw new InvalidOperationException($"Can only call {nameof(ReportMessageSubType)} when MessageType is {nameof(MessageType.Report)}");
            }
            return EnumUtils.ConstrainedParse<ReportMessageSubType>(MessageSubType);
        }
        public InquiryMessageSubType InquiryMessageSubType()
        {
            if (MessageType != MessageType.Inquiry)
            {
                CommonUtils.SilhouetteEventSource.Current.LogException($"Can only call {nameof(InquiryMessageSubType)} when MessageType is {nameof(MessageType.Inquiry)}");
                throw new InvalidOperationException($"Can only call {nameof(InquiryMessageSubType)} when MessageType is {nameof(MessageType.Inquiry)}");
            }
            return EnumUtils.ConstrainedParse<InquiryMessageSubType>(MessageSubType);
        }
        public CommandRequestMessageSubType CommandRequestMessageSubType()
        {
            if (MessageType != MessageType.CommandRequest)
            {
                CommonUtils.SilhouetteEventSource.Current.LogException($"Can only call {nameof(CommandRequestMessageSubType)} when MessageType is {nameof(MessageType.CommandRequest)}");
                throw new InvalidOperationException($"Can only call {nameof(CommandRequestMessageSubType)} when MessageType is {nameof(MessageType.CommandRequest)}");
            }
            return EnumUtils.ConstrainedParse<CommandRequestMessageSubType>(MessageSubType);
        }
        public CommandResponseMessageSubType CommandResponseMessageSubType()
        {
            if (MessageType != MessageType.CommandResponse)
            {
                CommonUtils.SilhouetteEventSource.Current.LogException($"Can only call {nameof(CommandResponseMessageSubType)} when MessageType is {nameof(MessageType.CommandResponse)}");
                throw new InvalidOperationException($"Can only call {nameof(CommandResponseMessageSubType)} when MessageType is {nameof(MessageType.CommandResponse)}");
            }
            return EnumUtils.ConstrainedParse<CommandResponseMessageSubType>(MessageSubType);
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
        Inquiry,

    }

    public enum CommandRequestMessageSubType
    {
        /// <summary>
        /// CommandRequest: body contains desired state for the device
        /// </summary>
        SetState,
        /// <summary>
        /// CommandRequest: request for the device to report its state
        /// </summary>
        ReportState,
        /// <summary>
        /// CommandRequest: sent in response to to a device sending an InquiryRequest
        /// </summary>
        LatestState,
    }

    public enum CommandResponseMessageSubType
    {
        /// <summary>
        /// CommandResponse: Device sent ACK
        /// </summary>
        Acknowledged,
        /// <summary>
        /// CommandResponse: the request message was not delivered before the message TTL
        /// </summary>
        Expired,
        /// <summary>
        /// CommandResponse: Device sent NAK
        /// </summary>
        NotAcknowledged,
        /// <summary>
        /// CommandResponse: the number of delivery attempts for the request message exceeded the retry count
        /// </summary>
        ExceededRetryCount,
    }
    public enum ReportMessageSubType
    {
        /// <summary>
        /// Report: The device is reporting its state
        /// </summary>
        State,
    }
    public enum InquiryMessageSubType
    {
        /// <summary>
        /// InquiryRequest: The device is requesting its last state
        /// </summary>
        GetState,
    }
}
