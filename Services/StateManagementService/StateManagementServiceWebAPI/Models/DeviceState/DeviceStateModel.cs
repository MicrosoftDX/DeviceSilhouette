// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
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
        /// <param name="deviceMessage"></param>
        public DeviceStateModel(DeviceRichState.DeviceMessage deviceMessage)
        {
            DeviceId = deviceMessage.DeviceId;
            Timestamp = deviceMessage.Timestamp;
            Version = deviceMessage.Version;
            CorrelationId = deviceMessage.CorrelationId;
            MessageType = deviceMessage.MessageType.ToString("F");
            MessageSubType = deviceMessage.MessageSubType;
            AppMetadata = string.IsNullOrEmpty(deviceMessage.AppMetadata) ? null : JToken.Parse(deviceMessage.AppMetadata);
            Values = string.IsNullOrEmpty(deviceMessage.Values) ? null : JToken.Parse(deviceMessage.Values); 
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
        public string MessageSubType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public JToken AppMetadata { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public JToken Values { get; set; }
    }
}

