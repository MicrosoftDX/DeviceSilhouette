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

 
    public interface IState
    {

    }

    [DataContract]
    public abstract class BaseCustomeState
    {

    }

    [DataContract]
   // [KnownType(typeof(IState))]
    public abstract class BaseDeviceState : IDeviceState
    {
        //TODO: how to serlize the Istate

        [DataMember]
        public string DeviceID { get; set; }
        [DataMember]
        public DateTime Timestamp { get; set; }
        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public BaseCustomeState customState { get; set; }
    }

}
