using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventProcessor
{
    internal class SilhouetteStateProcessor
    {
        string iotHubConnectionString = "HostName=ciscohackhub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=1WDxwhY0q0MXW8zhdVUjZFDEMGgjT/+Q0wmVmphtT9E=";
        ServiceClient serviceClient;

        // Let's keep the device states in a simple dictionary
        Dictionary<string, string> deviceStates = new Dictionary<string, string>();

        public SilhouetteStateProcessor()
        {
            // The Service Client is used to send messages to the device (C2D)
            serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
        }

        /*
         * Silhouette public API exposed to external classes
         */

        public string GetState(string deviceId)
        {
            return deviceStates[deviceId];
        }

        public void UpdateState(string deviceId, string state)
        {
            deviceStates[deviceId] = state;
            SendC2DUpdateState(deviceId).Wait();
        }

        /*
         * Silhouette state processing methods
         * - ProcessD2C* will process messages received from the device.
         * - SendC2D* will send messages to the devices.
         */

        internal async Task ProcessD2CUpdateState(string deviceId, string data)
        {
            Console.WriteLine("Update State.  New state: {0}", data);
            deviceStates[deviceId] = data;
        }

        internal async Task ProcessD2CGetState(string deviceId)
        {
            Console.WriteLine("Get State");
            await SendC2DUpdateState(deviceId);
        }

        async Task SendC2DGetState(string deviceId)
        {
            Console.WriteLine("Sending C2D_GetState...");

            // TODO: request delivery feedback?
            Message commandMessage;

            // Message body should not be empty! Otherwise does not trigger on Node.JS client side.
            commandMessage = new Message(Encoding.ASCII.GetBytes("{}"));
            commandMessage.Properties.Add("MessageType", "C2D_GetState");
            await serviceClient.SendAsync(deviceId, commandMessage);
        }

        async Task SendC2DUpdateState(string deviceId)
        {
            Console.WriteLine("Sending C2D_UpdateState...");

            // TODO: request delivery feedback?
            Message commandMessage;

            commandMessage = new Message(Encoding.ASCII.GetBytes(deviceStates[deviceId]));
            commandMessage.Properties.Add("MessageType", "C2D_UpdateState");
            await serviceClient.SendAsync(deviceId, commandMessage);
        }
    }
}