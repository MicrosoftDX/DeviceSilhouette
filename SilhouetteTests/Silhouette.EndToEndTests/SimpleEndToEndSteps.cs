using DotNetClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        private List<DeviceMessage> _receivedMessages;
        private string _stateRequestCorrelationId;

        // The slightly odd style of test method is because SpecFlow currently doesn't support async tests _yet_ :-( 
        // See https://github.com/techtalk/SpecFlow/issues/542
        // Update: in PR https://github.com/techtalk/SpecFlow/pull/647



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Given
        //

        [Given]
        public void Given_a_registered_and_connected_device_with_id_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                _device = await GetDeviceAsync(deviceId);
                _receivedMessages = new List<DeviceMessage>();
                _device.ReceivedMessage += _device_ReceivedMessage;
            });
        }
        private void _device_ReceivedMessage(object sender, ReceiveMessageEventArgs e)
        {
            _receivedMessages.Add(e.Message);
            e.Action = ReceiveMessageAction.None;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // When
        //

        [When]
        public void When_the_device_reports_its_state()
        {
            RunAndBlock(async () =>
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
                        appMetadata = new { testMetadata = _appMetadataValue },
                        values = new { test = _testStateValue },
                        timeToLiveMilliSec = 5000
                    });
            });
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Then
        //

        [Then]
        public void Then_the_reported_state_Api_contains_the_reported_state_for_device_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                var client = GetApiClient();

                var response = await client.GetAsync($"devices/{deviceId}/state/latest-reported");
                response.EnsureSuccessStatusCode();
                dynamic state = await response.Content.ReadAsAsync<dynamic>();

                Assert.IsNotNull(state, "state should not be null");
                Assert.IsNotNull(state.values, "state.values should not be null");
                Assert.IsNotNull(state.values.test, "state.values.test should not be null");
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


        [Then]
        public void Then_the_messages_API_contains_the_reported_state_message_for_device_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                Func<dynamic, bool> messagePredicate = m => m.values != null && m.values.test != null && m.values.test == _testStateValue;

                dynamic message = await FindMessageAsync(deviceId, messagePredicate);

                Assert.IsNotNull(message, "Message should not be null");
            });
        }

        [Then]
        public void Then_the_messages_API_contains_the_command_request_message_for_the_state_for_device_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                Func<dynamic, bool> messagePredicate = m => m.type == "CommandRequest"
                                && m.subtype == "SetState"
                                && m.values != null && m.values.test != null && m.values.test == _testStateValue;

                dynamic message = await FindMessageAsync(deviceId, messagePredicate);

                Assert.IsNotNull(message, "Message should not be null");
                _stateRequestCorrelationId = (string)message.correlationId;
                Assert.IsNotNull(_stateRequestCorrelationId, "CorrelationId should not be null");
            });
        }


        [Then]
        public void Then_the_device_receieves_the_state_request()
        {
            Assert.AreEqual(1, _receivedMessages.Count, "Device should have receieved one message");

            var message = _receivedMessages[0];

            Assert.AreEqual(_stateRequestCorrelationId, message.CorrelationId, "The message received by the device should match the request CorrelationId");
            Assert.AreEqual("CommandRequest", message.MessageType, "The message received by the device should have type CommandRequest");
            Assert.AreEqual("SetState", message.MessageSubType, "The message received by the device should have subtype SetState");

            dynamic body = JToken.Parse(message.Body);
            Assert.IsNotNull(body.test, "The message received by the device should have a test property");
            Assert.AreEqual(_testStateValue, body.test, "The message received by the device should hace the same property value as the requested state");
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Helpers
        //


        private static async Task<dynamic> FindMessageAsync(string deviceId, Func<dynamic, bool> messagePredicate)
        {
            var client = GetApiClient();
            string messagesUrl = $"devices/{deviceId}/messages";
            Func<dynamic, dynamic> findMessageInMessagesList = messageList =>
            {
                foreach (dynamic m in messageList)
                {
                    if (messagePredicate(m))
                    {
                        return m;
                    }
                }
                return null;
            };

            while (!string.IsNullOrEmpty(messagesUrl))
            {
                var response = await client.GetAsync(messagesUrl);
                response.EnsureSuccessStatusCode();
                dynamic messages = await response.Content.ReadAsAsync<dynamic>();
                dynamic message = findMessageInMessagesList(messages.values);
                if (message != null)
                {
                    return message;
                }
                messagesUrl = (string)((JToken)messages)["@nextLink"];
            }
            return null;
        }


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
