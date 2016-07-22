using DeviceRichState;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagementServiceWebAPI.Models
{
    /// <summary>
    /// Represents the device state
    /// </summary>
    public class DeviceStateModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceState"></param>
        public DeviceStateModel(DeviceState deviceState)
        {
            DeviceId = deviceState.DeviceId;
            Timestamp = deviceState.Timestamp;
            Version = deviceState.Version;
            CorrelationId = deviceState.CorrelationId;
            MessageType = deviceState.MessageType.ToString("F");
            MessageStatus = deviceState.MessageStatus.ToString("F");
            AppMetadata = deviceState.AppMetadata;
            DeviceValues = deviceState.Values;
        }

        /// <summary>
        /// The ID of the device the state is for
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// The timestamp for the state
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The version/sequence number for the state
        /// </summary>
        public int Version { get; set; }

        // TODO - revisit this model with the new message shape/types

        /// <summary>
        /// The correlation ID that the state corresponds to
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MessageStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public JToken AppMetadata { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public JToken DeviceValues { get; set; }
    }
}
