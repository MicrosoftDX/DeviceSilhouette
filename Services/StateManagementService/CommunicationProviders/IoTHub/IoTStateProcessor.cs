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

        internal async Task updateDevice(string DeviceId, string MessageType, string Message)
        {
            Message commandMessage;
            commandMessage = new Message(System.Text.Encoding.UTF8.GetBytes(Message));            
            commandMessage.Properties.Add("MessageType", MessageType);
            await _serviceClient.SendAsync(DeviceId, commandMessage);
        }
    }
}