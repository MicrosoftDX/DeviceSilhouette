using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DeviceRichState
{
    [DataContract]
    public class DeviceState
    {
        [DataMember]
        public string DeviceID { get; set; }

        /// <summary>
        /// Timestamp is UTC time
        /// </summary>
        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public int SequenceNumber { get; set; }

        [DataMember]
        public string MessageId { get; set; }

        [DataMember]
        public Types MessageType { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string AppMetadata { get; set; }

       /// <summary>
       /// 
       /// </summary>
       [DataMember]
        public string Values { get; set; }

        public DeviceState()
        {
        }

        //TODO: add enums for messageType and Status

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="state"></param>
        /// <param name="messageType">Allowed values : reported | requested</param>
        /// <param name="status">Allowed values : ACK | NACK | </param>
        public DeviceState(string deviceId, string state, Types messageType, string status = null)
        {
            DeviceID = deviceId;
            Timestamp = DateTime.UtcNow;
            MessageId = Guid.NewGuid().ToString();
            MessageType = messageType;

            if (status == null)
            {

                if (messageType == Types.reported)
                    Status = "received";

                if (messageType == Types.requested)
                    Status = "new";
            }



            //split state in metadata and values
            //State = state;
        }
    }

    public enum Types
    {
        reported,
        requested;
    }
}
