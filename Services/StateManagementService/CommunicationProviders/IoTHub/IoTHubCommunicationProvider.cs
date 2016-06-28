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

        private readonly Func<string, Task> _messageHandler;
        private readonly EventProcessorHost _eventProcessorHost;
        private readonly IoTHubEventProcessorFactory _factory;

        public IoTHubMessageReceiver(
            string IoTHubConnectionString,
            string StorageConnectionString,
            Func<string, Task> messageHandler)
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

    public class IotHubMessageSender : IMessageSender
    {
        private readonly ServiceClient _serviceClient;

        public IotHubMessageSender(string iotHubConnectionString)
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
        }
        public async Task SendCloudToDeviceAsync(string DeviceId, string MessageType, string Message)
        {
            Message commandMessage;
            commandMessage = new Message(System.Text.Encoding.UTF8.GetBytes(Message));
            commandMessage.Properties.Add("MessageType", MessageType);
            // get full acknowledgement on message delivery
            commandMessage.Ack = DeliveryAcknowledgement.Full;            
            await _serviceClient.SendAsync(DeviceId, commandMessage);
        }
    }
}
