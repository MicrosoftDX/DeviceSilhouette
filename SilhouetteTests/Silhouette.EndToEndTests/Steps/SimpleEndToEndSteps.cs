using DotNetClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

using Silhouette.EndToEndTests;

namespace Silhouette.EndToEndTests.Steps
{
    [Binding]
    public class SimpleEndToEndSteps : StepsBase
    {
        private const string BaseUrlAddress = "http://localhost:9013/v0.1/";

        private readonly Random _random = new Random();

        private string _appMetadataValue;

        private HttpResponseMessage _stateRequestHttpResponse;
        private string _commandUrl;
        private dynamic _command;

        public string CurrentCorrelationId
        {
            get { return (string)ScenarioContext.Current["CurrentCorrelationId"]; }
            set { ScenarioContext.Current["CurrentCorrelationId"] = value; }
        }
        public int TestStateValue
        {
            get { return (int)ScenarioContext.Current["TestStateValue"]; }
            set { ScenarioContext.Current["TestStateValue"] = value; }
        }




        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Given
        //




        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // When
        //


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
                TestStateValue = _random.Next(1, 1000000);
                _appMetadataValue = Guid.NewGuid().ToString();

                Log($"Sending state request via API. Test value {TestStateValue}, metadata value {_appMetadataValue}");

                var client = GetApiClient();
                _stateRequestHttpResponse = await client.PostAsJsonAsync($"devices/{deviceId}/commands",
                    new
                    {
                        subtype = "setState",
                        appMetadata = new { testMetadata = _appMetadataValue },
                        values = new { test = TestStateValue },
                        timeToLiveMilliSec = timeoutMs
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
                Assert.AreEqual(TestStateValue, (int)state.values.test); // Check that we get the value back
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
        public void Then_the_messages_API_contains_the_reported_state_message_for_device_DEVICEID_within_TARGETTIME_seconds_but_wait_up_to_TIMEOUT_seconds_to_verify(string deviceId, int targetTime, int timeout)
        {
            RunAndBlock(async () =>
            {
                
                Func<dynamic, bool> messagePredicate = m => m.values != null && m.values.test != null && m.values.test == TestStateValue;

                var stopwatch = Stopwatch.StartNew();
                dynamic message = await RetryAsync(
                        cancellationToken => FindMessageAsync(deviceId, messagePredicate, cancellationToken)
                        , timeout);
                var elapsedTime = stopwatch.Elapsed;

                Assert.IsNotNull(message, "Message should not be null");

                // Got here, so we got a message that looks ok...
                Log($"elapsed time: {elapsedTime}");
                var targetTimeSpan = TimeSpan.FromSeconds(targetTime);
                if (elapsedTime > targetTimeSpan)
                {
                    AddTimeoutMessage($"Waited {elapsedTime} for message. Target: {targetTimeSpan}");
                }
            });
        }

        [Then]
        public void Then_the_messages_API_contains_the_command_request_message_for_the_state_for_device_DEVICEID_within_TARGETTIME_seconds_but_wait_up_to_TIMEOUT_seconds_to_verify(string deviceId, int targetTime, int timeout)
        {
            // target time - this is the elapsed time that we assert against
            // timeout - this is the maximum time that the test will wait. Useful to assess whether the message arrived or not

            RunAndBlock(async () =>
            {

                Func<dynamic, bool> messagePredicate = m => m.type == "CommandRequest"
                                && m.subtype == "SetState"
                                && m.values != null && m.values.test != null && m.values.test == TestStateValue;

                var stopwatch = Stopwatch.StartNew();
                dynamic message = await RetryAsync(cancellationToken => FindMessageAsync(deviceId, messagePredicate, cancellationToken)
                                                , timeout);
                var elapsedTime = stopwatch.Elapsed;

                Assert.IsNotNull(message, "Message should not be null");
                Log($"elapsed time: {elapsedTime}");

                CurrentCorrelationId = (string)message.correlationId;
                Log($"CorrelationId: {CurrentCorrelationId}");
                Assert.IsNotNull(CurrentCorrelationId, "CorrelationId should not be null");


                // Got here, so we got a message that looks ok...
                var targetTimeSpan = TimeSpan.FromSeconds(targetTime);
                if (elapsedTime > targetTimeSpan)
                {
                    AddTimeoutMessage($"Waited {elapsedTime} for message. Target: {targetTimeSpan}");
                }
            });
        }


        [Then]
        public void Then_the_messages_API_contains_the_command_response_ACK_for_the_state_request_for_device_DEVICEID_within_TARGETTIME_seconds_but_wait_up_to_TIMEOUT_seconds_to_verify(string deviceId, int targetTime, int timeout)
        {
            // target time - this is the elapsed time that we assert against
            // timeout - this is the maximum time that the test will wait. Useful to assess whether the message arrived or not

            RunAndBlock(async () =>
            {
                Assert.IsNotNull(CurrentCorrelationId, "Should have a correlationId saved from a previous step");
                Func<dynamic, bool> messagePredicate = m => ((string)m.type) == "CommandResponse"
                                && ((string)m.correlationId) == CurrentCorrelationId;

                var stopwatch = Stopwatch.StartNew();
                dynamic message = await RetryAsync(cancellationToken => FindMessageAsync(deviceId, messagePredicate, cancellationToken)
                                                , timeout);
                var elapsedTime = stopwatch.Elapsed;

                Assert.IsNotNull(message, $"No CommandResponse found with correlationId '{CurrentCorrelationId}'");

                Log($"elapsed time: {elapsedTime}");

                Assert.AreEqual("Acknowledged", (string)message.subtype, "Response SubType should be Acknowledged");

                // Got here, so we got a message that looks ok...
                var targetTimeSpan = TimeSpan.FromSeconds(targetTime);
                if (elapsedTime > targetTimeSpan)
                {
                    AddTimeoutMessage($"Waited {elapsedTime} for message. Target: {targetTimeSpan}");
                }
            });
        }

       
        [Then]
        public void Then_the_command_API_contains_the_command_for_the_state_request_for_device_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                var client = GetApiClient();
                Assert.IsFalse(string.IsNullOrEmpty(CurrentCorrelationId), "State request correlation id should be set before invoking this test method");
                Assert.IsFalse(string.IsNullOrEmpty(_commandUrl), "command url should be set before invoking this test method");

                var response = await client.GetAsync(_commandUrl);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                _command = await response.Content.ReadAsAsync<dynamic>();


                Assert.IsNotNull(_command, "Command should not be null");
                Assert.IsNotNull(_command.request, "Command.Request should not be null");

                Assert.IsNotNull(_command.id, "CorrelationId should not be null");
                Assert.AreEqual(CurrentCorrelationId, (string)_command.id, "Command id should match state request correlation id");
                Assert.AreEqual("CommandRequest", (string)_command.request.type, "Command request type should be CommandRequest");
                Assert.AreEqual("SetState", (string)_command.request.subtype, "Command request subtype should be SetState");
                Assert.IsTrue(_command.request.values != null, "Command request values should be set");
                Assert.IsTrue(_command.request.values.test != null, "Command request values test property should be set");
                Assert.AreEqual(TestStateValue, (int)_command.request.values.test, "Command request values test property should be the requested value");
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



        [Then]
        public void Then_the_messages_API_contains_no_messages_for_device_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                var client = GetApiClient();
                string messagesUrl = $"devices/{deviceId}/messages";
                var response = await client.GetAsync(messagesUrl);
                response.EnsureSuccessStatusCode();
                dynamic messages = await response.Content.ReadAsAsync<dynamic>();

                Assert.IsTrue(messages.values == null, "Response.values should be null");
            });
        }

        [Then]
        public void Then_the_message_API_returns_NotFound_for_device_DEVICEID_for_message_version_VERSION(string deviceId, int version)
        {
            RunAndBlock(async () =>
            {
                var client = GetApiClient();
                string messagesUrl = $"devices/{deviceId}/messages/{version}";
                var response = await client.GetAsync(messagesUrl);
                
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code for non-existent message should be NotFound");
            });
        }

        [Then]
        public void Then_the_commands_API_contains_no_commands_for_device_DEVICEID(string deviceId)
        {
            RunAndBlock(async () =>
            {
                var client = GetApiClient();
                string commandsUrl = $"devices/{deviceId}/commands";
                var response = await client.GetAsync(commandsUrl);
                response.EnsureSuccessStatusCode();
                dynamic messages = await response.Content.ReadAsAsync<dynamic>();

                object messageList = messages.values;
                Assert.IsNull(messageList, "Response.values should be null");
            });
        }

        [Then]
        public void Then_the_command_API_returns_NotFound_for_device_DEVICEID_for_command_id_COMMANDID(string deviceId, string commandId)
        {
            RunAndBlock(async () =>
            {
                var client = GetApiClient();
                string messagesUrl = $"devices/{deviceId}/commands/{commandId}";
                var response = await client.GetAsync(messagesUrl);

                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code for non-existent command should be NotFound");
            });
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Helpers
        //
        private static Task<dynamic> FindMessageAsync(string deviceId, Func<dynamic, bool> messagePredicate)
        {
            return FindMessageAsync(deviceId, messagePredicate, CancellationToken.None);
        }
        private static async Task<dynamic> FindMessageAsync(
            string deviceId,
            Func<dynamic, bool> messagePredicate,
            CancellationToken cancellationToken)
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
                var response = await client.GetAsync(messagesUrl, cancellationToken);
                response.EnsureSuccessStatusCode();
                dynamic messages = await response.Content.ReadAsAsync<dynamic>(cancellationToken);
                dynamic message = findMessageInMessagesList(messages.values);
                if (message != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return message;
                }
                messagesUrl = (string)((JToken)messages)["@nextLink"];
            }
            cancellationToken.ThrowIfCancellationRequested();
            return null;
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
