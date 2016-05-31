using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    public class CommProviderIoTHub: ICommunicationProvider
    {
        // TODO: export configuration. all configuration is hard codded for now
        string iotHubConnectionString = "HostName=SilhouetteHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=VJJSbrsOPUx9oOklDs2iQGNx6rpG62wyIyGQ5wLO+6c=";
        //string iotHubConnectionString = "HostName=silhouette-tests.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=3GP77YoaR0+OXw8WX8DLKJiHUfbmr27XcSVSRi7Qi3s=";
        string iotHubD2cEndpoint = "messages/events";
        string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=silhouetteiotstore;AccountKey=Kqs/iRhopvVwfzIJ4+J6koASVAKPxx4dvRXCxqe3cqhHdlEQEWzRTZuSIRSDNubjH/sIm+Ym92iOgTYpbbCD7Q==";
        //string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=iotteststore;AccountKey=q+DEpESjnsfza7umztyEVfgUU54RpHCn6uowWYzo78cjEugjCplAjPuKeMYS5AnlDKAZ0Q/ic8ImEGSQw54TKg==";
        string container = "silhouette-events";

        private IoTStateProcessor stateProcessor;

        public CommProviderIoTHub()
        {            
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
