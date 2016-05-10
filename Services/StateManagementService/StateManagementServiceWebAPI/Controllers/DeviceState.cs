using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagementServiceWebAPI.Controllers
{
    public class Latitude : IState
    {
        public string Xaxis { get; set; }
        public string Yaxis { get; set; }
        public string Zaxis { get; set; }

        public Latitude(string x, string y, string z)
        {
            this.Xaxis = x;
            this.Yaxis = y;
            this.Zaxis = z;
        }       
    }

    public class DeviceState : BaseDeviceState
    {
        public DeviceState(string deviceId, Latitude state)
        {
            DeviceID = deviceId;
            State = state;
        }        
    }
}
