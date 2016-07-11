using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagementServiceWebAPI.Models
{
    public class ErrorModel
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        // TODO - need to implement logging and include tracking id in error message
        //public string TrackingId { get; set; }
    }

    public static class ErrorStatus
    {
        public const string InvalidDeviceId = "invalid-device-id";
    }

    // TODO - localisation
    public static class ErrorMessage
    {
        public static string InvalidDeviceId(string deviceId)
        {
            const string format = "The device '{0}' is not registered with IoT Hub";
            return string.Format(format, deviceId);
        }
    }


}
