using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace DeviceStateNamespace
{    

    [DataContract]
    public class DeviceState : BaseDeviceState
        {
            public DeviceState(string deviceId, string state)
            {
                DeviceID = deviceId;
                CustomState = state;
            }
        }
    
}
