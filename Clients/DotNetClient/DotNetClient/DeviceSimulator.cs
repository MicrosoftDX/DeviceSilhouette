using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetClient
{
    
    // TODO move this into a class library :-)
    // TODO - handle receiving messages
    // TODO - online/offline
    public class DeviceSimulator
    {
        private string _connectionString;
        private string _deviceId;
        private DeviceClient _deviceClient = null;

        public DeviceSimulator(string connectionString, string deviceId)
        {
            _connectionString = connectionString;
            _deviceId = deviceId;
        }
        

        /// <summary>
        /// Initialize. Ensure registered in IoT Hub
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            var registryManager = RegistryManager.CreateFromConnectionString(_connectionString);
            var device = await registryManager.GetDeviceAsync(_deviceId);
            if (device == null)
            {
                Console.WriteLine($"Add device: {_deviceId}");
                device = await registryManager.AddDeviceAsync(new Device(_deviceId));
            }

            string hostname = GetHostNameFromConnectionString(_connectionString);
            _deviceClient = DeviceClient.Create(
                hostname, 
                new DeviceAuthenticationWithRegistrySymmetricKey(_deviceId, device.Authentication.SymmetricKey.PrimaryKey)
                );
        }


        public async Task SendStateMessageAsync(object state)
        {
            var stateString = JsonConvert.SerializeObject(state);
            await SendStateMessageAsync(stateString);
        }
        public async Task SendStateMessageAsync(string state)
        {
            var stateBuf = Encoding.UTF8.GetBytes(state);

            var message = new Microsoft.Azure.Devices.Client.Message(stateBuf)
            {
                Properties =
                {
                    { "MessageType", "Report" },
                    { "MessageSubType", "State" },
                },
                //CorrelationId = "asdas" // TODO - use this when responding to messages!
            };
            await _deviceClient.SendEventAsync(message);
        }


        private static string GetHostNameFromConnectionString(string connectionString)
        {
            var builder = Microsoft.Azure.Devices.IotHubConnectionStringBuilder.Create(connectionString);
            return builder.HostName;
        }
    }
}
