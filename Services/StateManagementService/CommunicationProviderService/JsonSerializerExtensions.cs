using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationProviderService
{
    public static class JsonSerializerExtensions
    {
        public static string Serialize(this JsonSerializer jsonSerializer, object value)
        {
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value);
            }
            return sw.ToString();
        }

        public static T Deserialize<T>(this JsonSerializer jsonSerializer, string json)
        {
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                return jsonSerializer.Deserialize<T>(reader);
            }
        }
    }
}
