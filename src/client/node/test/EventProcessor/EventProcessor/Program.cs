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
            TestEventProcessor.StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=hubbhub;AccountKey=CHc0MrCFtNtGrK0l9JsDONeKThGWn+O07PUrZsceHXkCuV1nJtO7DGMZwQK8O3reD6jEJIqPQ9UBs5bH40eMdg==";
            ServiceClient serviceClient;

            // The Service Client is used to send messages to the device (C2D)
            serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);

            string eventProcessorHostName = Guid.NewGuid().ToString();

            // Start the Event Processor Host to receive messages from the devices (D2C)
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint,
                EventHubConsumerGroup.DefaultGroupName, iotHubConnectionString, TestEventProcessor.StorageConnectionString, "test-silhouette-events");

            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<TestEventProcessor>().Wait();

            Console.WriteLine("Receiving. q to quit, g to get state, u to update state");

            while (true)
            {
                var cmd = Console.ReadLine();

                if (cmd == "q")
                    break;

                // TODO: request delivery feedback?
                Message commandMessage;

                switch (cmd)
                {
                    case "g":
                        Console.WriteLine("Sending C2D_GetState...");
                        // Message body should not be empty! Otherwise does not trigger on Node.JS client side.
                        commandMessage = new Message(Encoding.ASCII.GetBytes("{}"));
                        commandMessage.Properties.Add("MessageType", "C2D_GetState");
                        serviceClient.SendAsync("silhouette1", commandMessage);
                        break;
                    case "u":
                        Console.WriteLine("Sending C2D_UpdateState...");
                        commandMessage = new Message(Encoding.ASCII.GetBytes("{\"state\":{\"foo\":\"bar\"}}"));
                        commandMessage.Properties.Add("MessageType", "C2D_UpdateState");
                        serviceClient.SendAsync("silhouette1", commandMessage);
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
