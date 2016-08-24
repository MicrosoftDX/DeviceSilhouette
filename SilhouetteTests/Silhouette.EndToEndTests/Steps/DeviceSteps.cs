using DotNetClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Silhouette.EndToEndTests.Steps
{
    [Binding]
    public sealed class DeviceSteps : StepsBase
    {
        private readonly Random _random = new Random();

        private DeviceSimulator _device;

        private List<DeviceMessage> _deviceReceivedMessages;
        private DeviceMessage _deviceReceivedMessage;
        private Task _deviceReceivedMessageTask;
        private TaskCompletionSource<int> _deviceReceivedMessageTaskCompletionSource;


        public string CurrentCorrelationId
        {
            get { return (string)ScenarioContext.Current["CurrentCorrelationId"]; }
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
        [Given]
        public void Given_a_registered_and_connected_device_with_id_DEVICEID(string deviceId)
        {
            this.RunAndBlock(async () =>
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
            this.RunAndBlock(async () =>
            {
                TestStateValue = _random.Next(1, 1000000);
                Log($"Device reporting state: test value {TestStateValue}");

                await _device.SendStateMessageAsync(new { test = TestStateValue });
            });
        }

        [When(@"the device requests its state")]
        public void WhenTheDeviceRequestsItsState()
        {
            this.RunAndBlock(async () =>
            {                
                Log($"Device requesting state");

                await _device.RequestStateMessageAsync();
            });
        }


        [When]
        public void When_the_device_accepts_the_state_request()
        {
            this.RunAndBlock(async () =>
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
        public void Then_the_device_receieves_the_state_request_within_TARGETTIME_seconds_but_wait_up_to_TIMEOUT_seconds_to_verify(int targetTime, int timeout)
        {
            // target time - this is the elapsed time that we assert against
            // timeout - this is the maximum time that the test will wait. Useful to assess whether the message arrived or not

            this.RunAndBlock(async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeout));
                var task = await Task.WhenAny(_deviceReceivedMessageTask, timeoutTask);
                if (task == timeoutTask)
                {
                    Assert.Fail("Timed out waiting for device to receive message");
                }
                var elapsedTime = stopwatch.Elapsed;


                Assert.AreEqual(1, _deviceReceivedMessages.Count, "Device should have received one message");

                _deviceReceivedMessage = _deviceReceivedMessages[0];

                Assert.AreEqual("CommandRequest", _deviceReceivedMessage.MessageType, "The message received by the device should have type CommandRequest");
                Assert.AreEqual("SetState", _deviceReceivedMessage.MessageSubType, "The message received by the device should have subtype SetState");

                dynamic body = JToken.Parse(_deviceReceivedMessage.Body);
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(body.test != null, "The message received by the device should have a test property");
                Assert.AreEqual(TestStateValue, (int)body.test, "The message received by the device should hace the same property value as the requested state");

                Log($"Elapsed time: {elapsedTime}");
                // Got here, so the message looks ok
                var targetTimeSpan = TimeSpan.FromSeconds(targetTime);
                if (elapsedTime > targetTimeSpan)
                {
                    AddTimeoutMessage($"Waited {elapsedTime} for message. Target: {targetTimeSpan}");
                }
            });
        }

        [Then(@"the device receieves the get state command within (.*) seconds but wait up to (.*) seconds to verify")]
        public void ThenTheDeviceReceievesTheGetStateCommandWithinSecondsButWaitUpToSecondsToVerify(int targetTime, int timeout)
        {
            // target time - this is the elapsed time that we assert against
            // timeout - this is the maximum time that the test will wait. Useful to assess whether the message arrived or not

            this.RunAndBlock(async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeout));
                var task = await Task.WhenAny(_deviceReceivedMessageTask, timeoutTask);
                if (task == timeoutTask)
                {
                    Assert.Fail("Timed out waiting for device to receive message");
                }
                var elapsedTime = stopwatch.Elapsed;


                Assert.AreEqual(1, _deviceReceivedMessages.Count, "Device should have received one message");

                _deviceReceivedMessage = _deviceReceivedMessages[0];

                Assert.AreEqual("CommandRequest", _deviceReceivedMessage.MessageType, "The message received by the device should have type CommandRequest");
                Assert.AreEqual("ReportState", _deviceReceivedMessage.MessageSubType, "The message received by the device should have subtype ReportState");

                Log($"Elapsed time: {elapsedTime}");
                // Got here, so the message looks ok
                var targetTimeSpan = TimeSpan.FromSeconds(targetTime);
                if (elapsedTime > targetTimeSpan)
                {
                    AddTimeoutMessage($"Waited {elapsedTime} for message. Target: {targetTimeSpan}");
                }
            });
        }


        [When(@"the device receieves the state update within (.*) seconds but wait up to (.*) seconds to verify")]
        public void WhenTheDeviceReceievesTheStateUpdateWithinSecondsButWaitUpToSecondsToVerify(int targetTime, int timeout)
        {
            // target time - this is the elapsed time that we assert against
            // timeout - this is the maximum time that the test will wait. Useful to assess whether the message arrived or not

            this.RunAndBlock(async () =>
            {
                var stopwatch = Stopwatch.StartNew();

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeout));
                var task = await Task.WhenAny(_deviceReceivedMessageTask, timeoutTask);
                if (task == timeoutTask)
                {
                    Assert.Fail("Timed out waiting for device to receive message");
                }
                var elapsedTime = stopwatch.Elapsed;


                Assert.AreEqual(1, _deviceReceivedMessages.Count, "Device should have received one message");

                _deviceReceivedMessage = _deviceReceivedMessages[0];

                Assert.AreEqual("CommandRequest", _deviceReceivedMessage.MessageType, "The message received by the device should have type CommandRequest");
                Assert.AreEqual("LatestState", _deviceReceivedMessage.MessageSubType, "The message received by the device should have subtype LatestState");

                Log($"Elapsed time: {elapsedTime}");
                // Got here, so the message looks ok
                var targetTimeSpan = TimeSpan.FromSeconds(targetTime);
                if (elapsedTime > targetTimeSpan)
                {
                    AddTimeoutMessage($"Waited {elapsedTime} for message. Target: {targetTimeSpan}");
                }
            });
        }


        [Then]
        public void Then_the_device_message_matches_the_messages_API_correlationId()
        {
            Assert.IsNotNull(_deviceReceivedMessage, "This step requires a received message");
            Assert.IsNotNull(CurrentCorrelationId, "This step requires that the correlation id has been retrieved");

            Assert.AreEqual(CurrentCorrelationId, _deviceReceivedMessage.CorrelationId, "Device Message choudl match correlation id in API");
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // Helpers
        //

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
    }
}
