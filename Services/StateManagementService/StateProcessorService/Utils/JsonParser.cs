using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateProcessorService.Utils
{
    public class StateObject
    {
        public string DeviceID { get; set; }
        public string Timestamp { get; set; }
        public string Status { get; set; }
    }

    class JsonParser
    {
        StateObject state;

        public JsonParser(string message)
        {
            state = JsonConvert.DeserializeObject<StateObject>(message);
        }

        public string Status()
        {
            return state.Status;
        }

        public string DeviceID()
        {
            return state.DeviceID;
        }
    }
}
