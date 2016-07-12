using DeviceRichState;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagementServiceWebAPI.Models
{
    public class DeviceStateModel
    {
        public DeviceStateModel(DeviceState deviceState)
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

        public string DeviceId { get; set; }

        public DateTime Timestamp { get; set; }

        public int Version { get; set; }

        public string CorrelationId { get; set; }

        public string MessageType { get; set; }

        public string MessageStatus { get; set; }

        public JToken AppMetadata { get; set; }

        public JToken Values { get; set; }
    }
}
