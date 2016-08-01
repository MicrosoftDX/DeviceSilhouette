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

        private List<DeviceMessage> _deviceReceivedMessages;
        private DeviceMessage _deviceReceivedMessage;
        private Task _deviceReceivedMessageTask;
        private TaskCompletionSource<int> _deviceReceivedMessageTaskCompletionSource;


        private int _testStateValue;
        private string _appMetadataValue;

        private HttpResponseMessage _stateRequestHttpResponse;
        private string _commandUrl;
        private string _stateRequestCorrelationId;
        private dynamic _command;



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
                _deviceReceivedMessages = new List<DeviceMessage>();
                _device.ReceivedMessage += device_OnMessageReceived;
                _device.StartReceiveMessageLoop();
                Log($"\tStarted device '{deviceId}' and listening for messages");
            });
        }
        private void device_OnMessageReceived(object sender, ReceiveMessageEventArgs e)
        {
            var device = (DeviceSimulator)sender;
            Log($"Device '{device.DeviceId}', received message with correlationId '{e.Message.CorrelationId}'");
            _deviceReceivedMessages.Add(e.Message);
            e.Action = ReceiveMessageAction.None;

            // trigger received task if requested
            if (_deviceReceivedMessageTaskCompletionSource != null)
            {
                _deviceReceivedMessageTaskCompletionSource.SetResult(0);
            }
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
                Log($"Device reporting state: test value {_testStateValue}");

                await _device.SendStateMessageAsync(new { test = _testStateValue });
            });
        }
        [When]
        public void When_we_wait_for_SECONDSTOWAIT_seconds(int secondsToWait)
        {
            Thread.Sleep(TimeSpan.FromSeconds(secondsToWait));
        }

        [When]
        public void When_a_state_request_is_sent_through_the_Api_for_device_DEVICEID_with_timeoutMs_TIMEOUTMS(string deviceId, int timeoutMs)
        {
            RunAndBlock(async () =>
            {
                _testStateValue = _random.Next(1, 1000000);
                _appMetadataValue = Guid.NewGuid().ToString();

                Log($"Sending state request via API. Test value {_testStateValue}, metadata value {_appMetadataValue}");

                var client = GetApiClient();
                _stateRequestHttpResponse = await client.PostAsJsonAsync($"devices/{deviceId}/commands",
                    new
                    {
                        appMetadata = new { testMetadata = _appMetadataValue },
                        values = new { test = _testStateValue },
                        timeToLiveMilliSec = timeoutMs
                    });
            });
        }

        [When]
        public void When_the_device_accepts_the_state_request()
        {
            RunAndBlock(async () =>
            {
                Assert.IsNotNull(_deviceReceivedMessage, "Should have a received message from a previous step");

                Log($"Device completing message. CorrelationId '{_deviceReceivedMessage.CorrelationId}'");
                await _device.CompleteReceivedMessageAsync(_deviceReceivedMessage);
            });
        }

        [When]
        public void When_we_set_up_a_trigger_for_the_device_receiving_messages()
        {
            _deviceReceivedMessageTaskCompletionSource = new TaskCompletionSource<int>();
            _deviceReceivedMessageTask = _deviceReceivedMessageTaskCompletionSource.Task;   
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
            Log($"API response. CommandUrl: {_commandUrl}");

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
                Log($"CorrelationId: {_stateRequestCorrelationId}");

                Assert.IsNotNull(_stateRequestCorrelationId, "CorrelationId should not be null");
            });
        }

        [Then]
        public void Then_the_messages_API_contains_the_command_response_ACK_for_the_state_request_for_device_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                Assert.IsNotNull(_stateRequestCorrelationId, "Should have a correlationId saved from a previous step");
                Func<dynamic, bool> messagePredicate = m => ((string)m.type) == "CommandResponse"
                                && ((string)m.correlationId) == _stateRequestCorrelationId;

                dynamic message = await FindMessageAsync(deviceId, messagePredicate);

                Assert.IsNotNull(message, $"No CommandResponse found with correlationId '{_stateRequestCorrelationId}'");

                Assert.AreEqual("Acknowledged", (string)message.subtype, "Response SubType should be Acknowledged");
            });
        }

        [Then]
        public void Then_the_device_receieves_the_state_request_within_TIMEOUT_seconds(int timeout)
        {
            RunAndBlock(async() =>
            {
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeout));
                var task = await Task.WhenAny(_deviceReceivedMessageTask, timeoutTask);
                if (task == timeoutTask)
                {
                    Assert.Fail("Timed out waiting for device to receive message");
                }

                Assert.AreEqual(1, _deviceReceivedMessages.Count, "Device should have received one message");

                _deviceReceivedMessage = _deviceReceivedMessages[0];

                Assert.AreEqual("CommandRequest", _deviceReceivedMessage.MessageType, "The message received by the device should have type CommandRequest");
                Assert.AreEqual("SetState", _deviceReceivedMessage.MessageSubType, "The message received by the device should have subtype SetState");

                dynamic body = JToken.Parse(_deviceReceivedMessage.Body);
                Assert.IsTrue(body.test != null, "The message received by the device should have a test property");
                Assert.AreEqual(_testStateValue, (int)body.test, "The message received by the device should hace the same property value as the requested state");
            });
        }
        [Then]
        public void Then_the_device_message_matches_the_messages_API_correlationId()
        {
            Assert.IsNotNull(_deviceReceivedMessage, "This step requires a received message");
            Assert.IsNotNull(_stateRequestCorrelationId, "This step requires that the correlation id has been retrieved");

            Assert.AreEqual(_stateRequestCorrelationId, _deviceReceivedMessage.CorrelationId, "Device Message choudl match correlation id in API");
        }

        [Then]
        public void Then_the_command_API_contains_the_command_for_the_state_request_for_device_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                var client = GetApiClient();
                Assert.IsFalse(string.IsNullOrEmpty(_stateRequestCorrelationId), "State request correlation id should be set before invoking this test method");
                Assert.IsFalse(string.IsNullOrEmpty(_commandUrl), "command url should be set before invoking this test method");

                var response = await client.GetAsync(_commandUrl);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                _command = await response.Content.ReadAsAsync<dynamic>();
                

                Assert.IsNotNull(_command, "Command should not be null");
                Assert.IsNotNull(_command.request, "Command.Request should not be null");

                Assert.IsNotNull(_command.id, "CorrelationId should not be null");
                Assert.AreEqual(_stateRequestCorrelationId, (string)_command.id, "Command id should match state request correlation id");
                Assert.AreEqual("CommandRequest", (string)_command.request.type, "Command request type should be CommandRequest");
                Assert.AreEqual("SetState", (string)_command.request.subtype, "Command request subtype should be SetState");
                Assert.IsTrue(_command.request.values != null, "Command request values should be set");
                Assert.IsTrue(_command.request.values.test != null, "Command request values test property should be set");
                Assert.AreEqual(_testStateValue, (int)_command.request.values.test, "Command request values test property should be the requested value");
            });
        }

        [Then]
        public void Then_the_command_received_from_the_API_has_no_response()
        {
            Assert.IsTrue(_command.response == null, "Command.Response should be null");
        }
        [Then]
        public void Then_the_command_received_from_the_API_has_an_ACK_response()
        {
            Assert.AreEqual("Acknowledged", (string)_command.response.subtype, "Command.Response.Subtype should be Acknowledged");
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
            try
            {
                asyncAction().Wait();
            }
            catch(AggregateException ae) when (ae.InnerException is AssertFailedException)
            {
                var afe = (AssertFailedException)ae.InnerException;
                throw new AssertFailedException("Wrapped: " + afe.Message, afe); // wrap the exception so that the inner exception preserves the stack trace
            }
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

        private static void Log(string message)
        {
            Console.WriteLine($"\t{DateTime.Now:yyyy-MM-dd-HH-mm-ss} {message}");
        }

    }
}
