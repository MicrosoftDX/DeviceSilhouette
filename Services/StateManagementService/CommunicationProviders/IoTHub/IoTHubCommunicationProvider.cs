using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    public class IoTHubCommunicationProvider: ICommunicationProvider
    {        
        string _iotHubConnectionString;         
        const string _iotHubD2cEndpoint = "messages/events";
        string _storageConnectionString;
        const string _container = "silhouette-events";

        private IoTStateProcessor _stateProcessor;

        public IoTHubCommunicationProvider(string IoTHubConnectionString, string StorageConnectionString)
        {
            _iotHubConnectionString = IoTHubConnectionString;
            _storageConnectionString = StorageConnectionString;

            _stateProcessor = new IoTStateProcessor(_iotHubConnectionString);

            string eventProcessorHostName = Guid.NewGuid().ToString();
            // Start the Event Processor Host to receive messages from the devices (D2C)
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, _iotHubD2cEndpoint,
                EventHubConsumerGroup.DefaultGroupName, _iotHubConnectionString, _storageConnectionString, _container);
            
            var factory = new IoTHubEventProcessorFactory(_stateProcessor);
            eventProcessorHost.RegisterEventProcessorFactoryAsync(factory).Wait();
        }

        public Task<string> ReceiveDeviceToCloudAsync()
        {
            return Task.FromResult(_stateProcessor.getMessage());
        }

        public Task SendCloudToDeviceAsync(string Message, string DeviceID)
        {
            return _stateProcessor.updateDevice(DeviceID, Message);
        }

    }
}
