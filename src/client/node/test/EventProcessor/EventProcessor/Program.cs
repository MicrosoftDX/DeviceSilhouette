using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            string iotHubConnectionString = "HostName=ciscohackhub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=1WDxwhY0q0MXW8zhdVUjZFDEMGgjT/+Q0wmVmphtT9E=";
            string iotHubD2cEndpoint = "messages/events";
            SilhouetteEventProcessor.StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=hubbhub;AccountKey=CHc0MrCFtNtGrK0l9JsDONeKThGWn+O07PUrZsceHXkCuV1nJtO7DGMZwQK8O3reD6jEJIqPQ9UBs5bH40eMdg==";

            // The SilhouetteStateProcessor will be used to manage receiving and sending state updates from/to the device

            var stateProcessor = new SilhouetteStateProcessor();

            // Start the Event Processor Host to receive messages from the devices (D2C)

            string eventProcessorHostName = Guid.NewGuid().ToString();

            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint,
                EventHubConsumerGroup.DefaultGroupName, iotHubConnectionString, SilhouetteEventProcessor.StorageConnectionString, "test-silhouette-events");

            Console.WriteLine("Registering EventProcessor...");
            var factory = new SilhouetteEventProcessorFactory(stateProcessor);
            eventProcessorHost.RegisterEventProcessorFactoryAsync(factory).Wait();

            // Loop and wait for input

            Console.WriteLine("Receiving. q to quit, g to get state, u to update state");

            while (true)
            {
                var cmd = Console.ReadLine();

                if (cmd == "q")
                    break;

                switch (cmd)
                {
                    case "g":
                        Console.WriteLine("Sending C2D_GetState...");
                        stateProcessor.SendC2DGetState("silhouette1").Wait();
                        break;
                    case "u":
                        Console.WriteLine("Sending C2D_UpdateState...");
                        stateProcessor.SendC2DUpdateState("silhouette1").Wait();
                        break;
                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
            }

            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
