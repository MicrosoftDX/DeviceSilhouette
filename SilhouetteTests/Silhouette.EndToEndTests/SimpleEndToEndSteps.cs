using DotNetClient;
using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Silhouette.EndToEndTests
{
    [Binding]
    public class SimpleEndToEndSteps
    {
        [Given]
        public void Given_a_registered_and_connected_device_with_id_DEVICEID(string deviceId)
        {
            ScenarioContext.Current.Pending();
        }

        [When]
        public void When_the_device_reports_its_state()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then]
        public void Then_the_reported_state_API_should_contain_the_reported_state()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then]
        public void Then_the_messages_API_should_contain_the_reported_state_message()
        {
            ScenarioContext.Current.Pending();
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

    }
}
