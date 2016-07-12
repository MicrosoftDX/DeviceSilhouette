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

    public class InvalidRequestErrorModel : ErrorModel
    {
        public List<ValidationMessage> ValidationMessages { get; set; }
    }
    public class ValidationMessage
    {
        public string PropertyName { get; set; }
        public List<string> Messages { get; set; }
    }

    public static class ErrorStatus
    {
        public const string InvalidDeviceId = "invalid-device-id";
        public const string InvalidRequest = "invalid-request";
    }

    // TODO - localisation
    public static class ErrorMessage
    {
        public static string InvalidDeviceId(string deviceId)
        {
            const string format = "The device '{0}' was not found";
            return string.Format(format, deviceId);
        }
        public static string InvalidRequest()
        {
            return "The request body was invalid";
        }
    }


}
