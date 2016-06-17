using Microsoft.Azure.Devices;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    internal class IoTStateProcessor
    {        
        // TODO: replace with service bus queue 
        private ConcurrentQueue<string> _d2cMessages = new ConcurrentQueue<string>();
        private ServiceClient _serviceClient;
       
        public IoTStateProcessor(string iotHubConnectionString)
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
        }

        internal void processMessage(string message)
        {
            _d2cMessages.Enqueue(message);
        }

        internal string getMessage()
        {
            string message;
            _d2cMessages.TryDequeue(out message);
            return message;
        }

        internal async Task updateDevice(string deviceID, string message)
        {
            Message commandMessage;
            commandMessage = new Message(System.Text.Encoding.UTF8.GetBytes(message));
            // TODO: check the message and send messagetype according to it. Now it always sends State:Set
            commandMessage.Properties.Add("MessageType", "State:Set");
            await _serviceClient.SendAsync(deviceID, commandMessage);
        }
    }
}