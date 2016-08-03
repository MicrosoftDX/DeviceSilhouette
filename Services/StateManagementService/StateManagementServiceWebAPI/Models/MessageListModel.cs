using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateManagementServiceWebAPI.Models.DeviceMessage
{
    /// <summary>
    /// 
    /// </summary>
    public class ListModel<TItem>
    {

        /// <summary>
        /// The messages in the current page
        /// </summary>
        public IEnumerable<TItem> Values { get; set; }
        /// <summary>
        /// The link to use to get the next page of messages
        /// </summary>
        [JsonProperty(PropertyName ="@nextLink")]
        public string NextLink { get; set; }
    }
}
