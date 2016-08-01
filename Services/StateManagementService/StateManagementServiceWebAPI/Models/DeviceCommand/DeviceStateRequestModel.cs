using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace StateManagementServiceWebAPI.Models
{
    /// <summary>
    /// Data model used to capture the State Request command added through the API
    /// </summary>
    public class DeviceStateRequestModel
    {
        /// <summary>
        ///     
        /// </summary>
        public DeviceStateRequestModel()
        {
            //empty ctor for WebAPI model binding
        }
        

        /// <summary>
        /// The application specific metadata
        /// </summary>
        [Required]
        public JToken AppMetadata { get; set; }

        /// <summary>
        /// The device values (state) to set
        /// </summary>
        [Required]
        public JToken Values { get; set; }

        /// <summary>
        /// The time-to-live for the command message
        /// </summary>
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Message TTL must be greater than zero")]
        public int TimeToLiveMilliSec { get; set; }
    }
}
