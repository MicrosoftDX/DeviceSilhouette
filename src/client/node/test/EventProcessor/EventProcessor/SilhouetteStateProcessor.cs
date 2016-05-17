using Microsoft.Azure.Devices;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventProcessor
{
    internal class SilhouetteStateProcessor
    {
        string iotHubConnectionString = "HostName=ciscohackhub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=1WDxwhY0q0MXW8zhdVUjZFDEMGgjT/+Q0wmVmphtT9E=";
        ServiceClient serviceClient;

        public SilhouetteStateProcessor()
        {
            // The Service Client is used to send messages to the device (C2D)
            serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
        }

        /*
         * Silhouette methods
         * - ProcessD2C* will process messages received from the device.
         * - SendC2D* will send messages to the devices.
         */

        public async Task ProcessD2CUpdateState(string deviceId, string data)
        {
            Console.WriteLine("Update State.  New state: {0}", data);
        }

        public async Task ProcessD2CGetState(string deviceId, string data)
        {
            Console.WriteLine("Get State");
        }

        public async Task SendC2DGetState(string deviceId)
        {
            // TODO: request delivery feedback?
            Message commandMessage;

            // Message body should not be empty! Otherwise does not trigger on Node.JS client side.
            commandMessage = new Message(Encoding.ASCII.GetBytes("{}"));
            commandMessage.Properties.Add("MessageType", "C2D_GetState");
            await serviceClient.SendAsync(deviceId, commandMessage);
        }

        public async Task SendC2DUpdateState(string deviceId)
        {
            // TODO: request delivery feedback?
            Message commandMessage;

            commandMessage = new Message(Encoding.ASCII.GetBytes("{\"state\":{\"foo\":\"bar\"}}"));
            commandMessage.Properties.Add("MessageType", "C2D_UpdateState");
            await serviceClient.SendAsync(deviceId, commandMessage);
        }
    }
}