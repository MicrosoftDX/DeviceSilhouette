using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    public class CommProviderIoTHub: ICommunicationProvider
    {
        private IoTStateProcessor stateProcessor;

        public CommProviderIoTHub()
        {
            // TODO: export configuration. all configuration is hard codded for now
            string iotHubConnectionString = "HostName=SilhouetteHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=VJJSbrsOPUx9oOklDs2iQGNx6rpG62wyIyGQ5wLO+6c=";
            string iotHubD2cEndpoint = "messages/events";
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=silhouetteiotstore;AccountKey=Kqs/iRhopvVwfzIJ4+J6koASVAKPxx4dvRXCxqe3cqhHdlEQEWzRTZuSIRSDNubjH/sIm+Ym92iOgTYpbbCD7Q==";
            string container = "silhouette-events";

            stateProcessor = new IoTStateProcessor();

            // Start the Event Processor Host to receive messages from the devices (D2C)

            string eventProcessorHostName = Guid.NewGuid().ToString();

            // TODO: check that the container exist
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint,
                EventHubConsumerGroup.DefaultGroupName, iotHubConnectionString, storageConnectionString, container);

            Console.WriteLine("Registering EventProcessor...");
            var factory = new IoTHubEventProcessorFactory(stateProcessor);
            eventProcessorHost.RegisterEventProcessorFactoryAsync(factory).Wait();
        }

        public Task<string> ReceiveDeviceToCloudAsync()
        {           
            return Task.Delay(1).ContinueWith(t => stateProcessor.getMessage());
        }

        public Task SendCloudToDeviceAsync(string message, string DeviceID)
        {
            throw new NotImplementedException();
        }

    }
}
