using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
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
            var uri = new Uri(_connectionString + _deviceID + "/state/latest-reported");

            var RootFilter = new HttpBaseProtocolFilter();
            RootFilter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.MostRecent;
            RootFilter.CacheControl.WriteBehavior = Windows.Web.Http.Filters.HttpCacheWriteBehavior.NoCache;
           
            var httpClient = new HttpClient(RootFilter);
           
            string strJson = null;

            try
            {
                var result = await httpClient.GetAsync(uri);

                if (result.IsSuccessStatusCode)
                {
                    strJson = result.Content.ToString();
                }
    
            }
            catch (Exception ex)
            {
                strJson = ex.Message;
                   
            }

            httpClient.Dispose();

            return strJson; ;
        }

        public async Task<string> UpdateState(string newState)
        {
            var uri = new Uri(_connectionString + _deviceID + "/commands");
            var httpClient = new HttpClient();

            

            string JsonString =
            @"{
                ""subtype"": ""setState"",
                ""appMetadata"": { ""origin"" : ""UWPApp"" },
                ""values"": { ""status"" : """ + newState + @""" },
                ""timeToLiveMilliSec"": 5000
            }";

           
            var result = await httpClient.PostAsync(uri, new HttpStringContent(JsonString,Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json"));

            return result.ToString();
            
        }
    }
}
