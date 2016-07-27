using DotNetClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Silhouette.EndToEndTests
{
    [Binding]
    public class SimpleEndToEndSteps
    {
        private const string BaseUrlAddress = "http://localhost:9013/v0.1/";
        private readonly Random _random = new Random();


        private DeviceSimulator _device;
        private int _testStateValue;
        private string _appMetadataValue;
        private HttpResponseMessage _stateRequestHttpResponse;
        private string _commandUrl;

        // The slightly odd style of test method is because SpecFlow currently doesn't support async tests _yet_ :-( 
        // See https://github.com/techtalk/SpecFlow/issues/542
        // Update: in PR https://github.com/techtalk/SpecFlow/pull/647


        [Given]
        public void Given_a_registered_and_connected_device_with_id_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                _device = await GetDeviceAsync(deviceId);
            });
        }





        [When]
        public void When_the_device_reports_its_state()
        {
            RunAndBlock(async() =>
            {
                _testStateValue = _random.Next(1, 1000000);
                await _device.SendStateMessageAsync(new { test = _testStateValue });
            });
        }
        [When]
        public void When_we_wait_for_SECONDSTOWAIT_seconds(int secondsToWait)
        {
            Thread.Sleep(TimeSpan.FromSeconds(secondsToWait));
        }


        [When]
        public void When_a_state_request_is_sent_through_the_Api_for_device_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {

                _testStateValue = _random.Next(1, 1000000);
                _appMetadataValue = Guid.NewGuid().ToString();

                var client = GetApiClient();
                _stateRequestHttpResponse = await client.PostAsJsonAsync($"devices/{deviceId}/commands",
                    new
                    {
                        appMetadata= new  { testMetadata = _appMetadataValue},
                        values= new { test = _testStateValue },
                        timeToLiveMilliSec = 5000
                    });


            });
        }




        [Then]
        public void Then_the_reported_state_Api_should_contain_the_reported_state_for_device_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                var client = GetApiClient();

                var response = await client.GetAsync($"devices/{deviceId}/state/latest-reported");
                response.EnsureSuccessStatusCode();
                dynamic state = await response.Content.ReadAsAsync<dynamic>();

                Assert.AreEqual(_testStateValue, (int)state.values.test); // Check that we get the value back
            });
        }

        [Then]
        public void Then_the_Api_status_code_is_created()
        {
            Assert.AreEqual(HttpStatusCode.Created, _stateRequestHttpResponse.StatusCode);
        }

        [Then]
        public void Then_the_Api_response_includes_a_Location_header_with_the_command_Url()
        {
            Assert.IsNotNull(_stateRequestHttpResponse.Headers.Location, "Location should not be null");
            _commandUrl = _stateRequestHttpResponse.Headers.Location.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(_commandUrl), "Location should not be empty");
        }


        //[Then]
        //public void Then_the_messages_API_should_contain_the_reported_state_message()
        //{
        //    ScenarioContext.Current.Pending();
        //}


        private void RunAndBlock(Func<Task> asyncAction)
        {
            asyncAction().Wait();
        }

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


        private static HttpClient GetApiClient()
        {
            return new HttpClient()
            {
                BaseAddress = new Uri(BaseUrlAddress)
            };
        }

    }
}
