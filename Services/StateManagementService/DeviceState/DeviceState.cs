using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace DeviceStateNamespace
{
    [DataContract]
    public class Latitude : IState
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
    public class DeviceState : BaseDeviceState
        {
            public DeviceState(string deviceId, Latitude state)
            {
                DeviceID = deviceId;
                State = state;
            }
        }
    
}
