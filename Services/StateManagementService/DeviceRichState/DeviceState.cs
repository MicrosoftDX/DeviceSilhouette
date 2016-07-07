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

        [DataMember]
        private string _deviceId;
        [DataMember]
        private string _correlationId;
        [DataMember]
        private DateTime _timestamp;
        [DataMember]
        private Types _messageType; 

        /// <summary>
        /// Can only be set at DeviceState instantiation
        /// </summary>
        [DataMember]
        public string DeviceId { get { return _deviceId; } set {;} }

        /// <summary>
        /// Timestamp is UTC time, set automatically on creation
        /// </summary>
        [DataMember]
        public DateTime Timestamp { get { return _timestamp; } set {;} }

        /// <summary>
        /// Version is set by silhouette actor, auto increment
        /// </summary>
        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public string CorrelationId { get { return _correlationId; } set {;} }

        [DataMember]
        public Types MessageType { get { return _messageType; } set {;}  }

        [DataMember]
        public Status MessageStatus { get; set; }

        /// <summary>
        /// Part of the state message that contains Application specific data
        /// </summary>
        [DataMember]
        public string AppMetadata { get; set; }

       /// <summary>
       /// Part of the state message that contains device metrics
       /// </summary>
       [DataMember]
        public string Values { get; set; }


        /// <summary>
        /// Holds the RichState of a device based on the state message and 
        /// </summary>
        /// <param name="deviceId">Unique indentifier of the device</param>
        /// <param name="metadata">Application metadata</param>   
        /// <param name="values">Actual data recevied from the device</param>
        /// <param name="messageType">Who send the message; reported == device, requested == application</param>
        /// <param name="messageStatus">Indication of the status of this message instance</param>
        /// /// <param name="correlationId">Message id</param>
        public DeviceState(string deviceId, string metadata, string values, Types messageType, Status messageStatus = Status.Unknown, string correlationId = null)
        {
            // To make these values immutable they are set through private field and get through public property
            // It is not possible to make the setter readonly because of [DataMember]
            _deviceId = deviceId;
            _timestamp = DateTime.UtcNow;
            _messageType = messageType;

            if (messageStatus == Status.Unknown)
            {
                _correlationId = Guid.NewGuid().ToString();

                if (messageType == Types.Report)
                    MessageStatus = Status.Received;

                if (messageType == Types.Request)
                    MessageStatus = Status.New;
            }
            else
                _correlationId = String.IsNullOrEmpty(correlationId) ? Guid.NewGuid().ToString() : correlationId;
                MessageStatus = messageStatus;

            AppMetadata = metadata;
            Values = values;
        }        
    }

    public class PublicDeviceState
    {
        public PublicDeviceState(DeviceState deviceState)
        {
            DeviceId = deviceState.DeviceId;
            Timestamp = deviceState.Timestamp;
            Version = deviceState.Version;
            CorrelationId = deviceState.CorrelationId;
            MessageType = deviceState.MessageType.ToString("F");
            MessageStatus = deviceState.MessageStatus.ToString("F");
            AppMetadata = deviceState.AppMetadata;
            Values = deviceState.Values;

        }

        /// <summary>
        /// Can only be set at DeviceState instantiation
        /// </summary>
        [DataMember]
        public string DeviceId { get; set; }

        /// <summary>
        /// Timestamp is UTC time, set automatically on creation
        /// </summary>
        [DataMember]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Version is set by silhouette actor, auto increment
        /// </summary>
        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public string CorrelationId { get; set; }

        [DataMember]
        public string MessageType { get; set; }

        [DataMember]
        public string MessageStatus { get; set; }

        /// <summary>
        /// Part of the state message that contains Application specific data
        /// </summary>
        [DataMember]
        public string AppMetadata { get; set; }

        /// <summary>
        /// Part of the state message that contains device metrics
        /// </summary>
        [DataMember]
        public string Values { get; set; }

    }

    public enum Types { Report, Request }

    public enum Status { Acknowledged, Expired, DeliveryCountExceeded, NotAcknowledged, Enqueued, New, Received, Unknown }    
}
