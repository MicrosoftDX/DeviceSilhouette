using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetClient;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;

namespace Silhouette.EndToEndTests
{
    [TestClass]
    public class SimpleEndToEndTests 
    {
        private const string BaseUrlAddress = "http://localhost:9013/v0.1/";

        private async Task<DeviceSimulator> GetDeviceAsync(string deviceId)
        {
            const string templateConnectionString = "%Silhouette_IotHubConnectionString%";
            string connectionString = Environment.ExpandEnvironmentVariables(templateConnectionString);
            if (connectionString == templateConnectionString)
            {
                throw new Exception("Ensure that the Silhouette_IotHubConnectionString environment variable is set");
            }

            var device = new DeviceSimulator(connectionString, deviceId);

            await device.InitializeAsync();

            return device;
        }

        [TestMethod]
        public async Task WhenDeviceSendsStateReport_ThenItIsReadableViaApi()
        {
            // TODO - add a method to checkpoint the 
            // TODO - look at a way to clear the SF state for testing purposes

            const string deviceId = "e2eDevice1";
            var device = await GetDeviceAsync(deviceId);


            var random = new Random();
            int testValue = random.Next(1, 1000000);
            await device.SendStateMessageAsync(new { test = testValue }); // report state


            await Task.Delay(2000); // TODO need to handle retrying state query until update is seen
            var client = GetApiClient();

            var response = await client.GetAsync($"devices/{deviceId}/state/latest-reported");
            response.EnsureSuccessStatusCode();
            //dynamic state = await response.Content.ReadAsAsync<LatestReportedState>();
            var content = await response.Content.ReadAsStringAsync();
            dynamic state = JsonConvert.DeserializeObject(content);

            Assert.AreEqual(testValue, (int)state.deviceValues.test); // Check that we get the value back
        }


        //[TestMethod]
        //public async Task WhenStateRequestIsSentViaApi_ThenDeviceReceivesIt()
        //{
        //    const string deviceId = "e2eDevice1";
        //    var device = await GetDeviceAsync(deviceId);
        //    var random = new Random();
        //    int testValue = random.Next(1, 1000000);

        //    var client = GetApiClient();

        //    var response = await client.PostAsJsonAsync($"devices/{deviceId}/commands",
        //            new
        //            {
        //                appMetadata = (object)null,
        //                values = new { test = testValue },
        //                timeToLiveMs = 5000
        //            }
        //        );

        //    Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            
            
            

        //}



        private static HttpClient GetApiClient()
        {
            return new HttpClient()
            {
                BaseAddress = new Uri(BaseUrlAddress)
            };
        }


        public class LatestReportedState
        {
            public string deviceId { get; set; }
            public DateTime timestamp { get; set; }
            public int version { get; set; }
            public string correlationId { get; set; }
            public string messageType { get; set; }
            public string messageSubType { get; set; }
            public object appMetadata { get; set; }
            public string deviceValues { get; set; }
        }

    }
}
