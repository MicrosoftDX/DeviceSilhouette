using System;

namespace StateManagementServiceWebAPI.Controllers
{
    public interface IDeviceState
    {                    
    }

    public interface IState
    {

    }


    public abstract class BaseDeviceState : IDeviceState
    {       
        public string DeviceID { get; set; }
        public DateTime Timestamp { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public IState State { get; set; }
    }

}