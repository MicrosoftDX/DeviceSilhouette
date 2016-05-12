using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace DeviceStateNamespace
{

    [DataContract(Name = "Latitude")]
    //public class Latitude : IState
    public class Latitude : BaseCustomeState  
        {
            [DataMember]
            public string Xaxis { get; set; }
            [DataMember]
            public string Yaxis { get; set; }
            [DataMember]
            public string Zaxis { get; set; }

            public Latitude(string x, string y, string z)
            {
                this.Xaxis = x;
                this.Yaxis = y;
                this.Zaxis = z;
            }
        }


    [DataContract]
    [KnownType(typeof(Latitude))]

    public class DeviceState : BaseDeviceState
        {
            public DeviceState(string deviceId, Latitude state)
            {
                DeviceID = deviceId;
                customState = state;
            }
        }
    
}
