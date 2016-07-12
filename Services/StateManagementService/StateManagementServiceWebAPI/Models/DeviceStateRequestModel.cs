using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace StateManagementServiceWebAPI.Models
{
    public class DeviceStateRequestModel
    {
        public DeviceStateRequestModel()
        {
            //empty ctor for WebAPI model binding
        }
        
        [Required]
        public JToken AppMetadata { get; set; }

        [Required]
        public JToken Values { get; set; }

        [Required]
        public int TimeToLiveMilliSec { get; set; }
    }
}
