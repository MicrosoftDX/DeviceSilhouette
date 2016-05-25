using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace DeviceStateNamespace
{
 
    public interface IDeviceState
    {
    }
   

    [DataContract]
    public abstract class BaseDeviceState : IDeviceState
    {        
        [DataMember]
        public string DeviceID { get; set; }
        [DataMember]
        public DateTime Timestamp { get; set; }
        [DataMember]
        public int Version { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string CustomState { get; set; }

    }

}
