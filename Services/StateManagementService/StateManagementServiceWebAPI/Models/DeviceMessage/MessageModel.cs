using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagementServiceWebAPI.Models.DeviceMessage
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageModel
    {
        /// <summary>
        /// The device ID for the message
        /// </summary>
        public string DeviceId { get; internal set; }
        /// <summary>
        /// The timestamp for when the message was enqueued
        /// </summary>
        public DateTime TimeStamp { get; internal set; }
        /// <summary>
        /// The version number for the message. This is a unique identifier for the message within a device id
        /// </summary>
        public int Version { get; internal set; }

        // TODO - add the rest of the properties!
    }
}
