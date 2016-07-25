using DeviceRichState;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    public class IoTHubMessageReceiver : IMessageReceiver
    {
        string _iotHubConnectionString;
        const string _iotHubD2cEndpoint = "messages/events";
        string _storageConnectionString;
        const string _container = "silhouette-events";

        private readonly Func<MessageInfo, Task> _messageHandler;
        private readonly EventProcessorHost _eventProcessorHost;
        private readonly IoTHubEventProcessorFactory _factory;

        public IoTHubMessageReceiver(
            string IoTHubConnectionString,
            string StorageConnectionString,
            Func<MessageInfo, Task> messageHandler)
        {
            _iotHubConnectionString = IoTHubConnectionString;
            _storageConnectionString = StorageConnectionString;
            _messageHandler = messageHandler;

            string eventProcessorHostName = Guid.NewGuid().ToString();
            // Start the Event Processor Host to receive messages from the devices (D2C)
            _eventProcessorHost = new EventProcessorHost(eventProcessorHostName, _iotHubD2cEndpoint,
                EventHubConsumerGroup.DefaultGroupName, _iotHubConnectionString, _storageConnectionString, _container);

            _factory = new IoTHubEventProcessorFactory(messageHandler);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await _eventProcessorHost.RegisterEventProcessorFactoryAsync(_factory);

            cancellationToken.WaitHandle.WaitOne();
        }
    }
}



