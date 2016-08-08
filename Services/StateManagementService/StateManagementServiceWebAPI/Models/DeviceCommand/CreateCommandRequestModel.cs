using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using DeviceRichState;
using StateManagementServiceWebAPI.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StateManagementServiceWebAPI.Models
{
    /// <summary>
    /// Data model used to capture the State Request command added through the API
    /// </summary>
    public class CreateCommandRequestModel
    {
        /// <summary>
        ///     
        /// </summary>
        public CreateCommandRequestModel()
        {
            //empty ctor for WebAPI model binding
        }

        /// <summary>
        /// The subtype of the message to create, i.e. the type of Command Request to create
        /// </summary>
        [EnumIsDefinedValue]
        [JsonConverter(typeof(StringEnumConverter))]
        [Required]
        public CommandRequestMessageSubType? Subtype { get; set; }

        /// <summary>
        /// The application specific metadata
        /// </summary>
        public JToken AppMetadata { get; set; }

        /// <summary>
        /// The device values (state) to set
        /// </summary>
        public JToken Values { get; set; }

        /// <summary>
        /// The time-to-live for the command message
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Message TTL must be greater than zero")]
        public int TimeToLiveMilliSec { get; set; }
    }
}
