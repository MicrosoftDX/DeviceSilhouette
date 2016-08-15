using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace SilhouetteRestClient
{
    public class RestClient
    {
        private readonly string _connectionString;
        private readonly string _deviceID;

        public RestClient(string ConnectionString, string DeviceID)
        {
            _connectionString = ConnectionString;
            _deviceID = DeviceID;

        }

        public RestClient()
        {
            _connectionString = "http://localhost:9013/v0.1/devices/";
            _deviceID = "DemoAppLightBulb";

        }

        public async Task<string> GetLatestReportedState()
        {

            var uri = new Uri(_connectionString + _deviceID + "/state/latest-reported" );
            var httpClient = new HttpClient();

            

            try
            {
                var result = await httpClient.GetAsync(uri);
                if (result.IsSuccessStatusCode)
                {
                    string strJson = result.Content.ToString();
                    dynamic jObj = (JObject)JsonConvert.DeserializeObject(strJson);
                    var status = jObj["values"]["status"].Value;
                    return status;

                }
                return "unknown";

                
            }
            catch
            {
                // Details in ex.Message and ex.HResult.       
            }


            // Once your app is done using the HttpClient object call dispose to 
            // free up system resources (the underlying socket and memory used for the object)


            //httpclient.Dispose();

            return "unknown";
        }
    }
}
