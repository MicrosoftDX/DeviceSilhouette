using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    public class CommProviderIoTHub: ICommunicationProvider
    {        
        string iotHubConnectionString;         
        string iotHubD2cEndpoint = "messages/events";
        string storageConnectionString;
        string container = "silhouette-events";

        private IoTStateProcessor stateProcessor;

        public CommProviderIoTHub(string IoTHubConnectionString, string StorageConnectionString)
        {
            iotHubConnectionString = IoTHubConnectionString;
            storageConnectionString = StorageConnectionString;

            stateProcessor = new IoTStateProcessor(iotHubConnectionString);

            string eventProcessorHostName = Guid.NewGuid().ToString();
            // Start the Event Processor Host to receive messages from the devices (D2C)
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint,
                EventHubConsumerGroup.DefaultGroupName, iotHubConnectionString, storageConnectionString, container);
            
            var factory = new IoTHubEventProcessorFactory(stateProcessor);
            eventProcessorHost.RegisterEventProcessorFactoryAsync(factory).Wait();
        }

        public Task<string> ReceiveDeviceToCloudAsync()
        {           
            return Task.Delay(1).ContinueWith(t => stateProcessor.getMessage());
        }

        public Task SendCloudToDeviceAsync(string Message, string DeviceID)
        {
            return stateProcessor.updateDevice(DeviceID, Message);
        }

    }
}
