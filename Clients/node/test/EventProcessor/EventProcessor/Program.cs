// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;
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

            string state = null;

            while (true)
            {
                var cmd = Console.ReadLine();

                if (cmd == "q")
                    break;

                // Poor man's arg parsing...
                string[] cmdargs = cmd.Split(' ');
                cmd = cmdargs[0];
                string arg = null;
                if (cmdargs.Length > 1)
                    arg = cmdargs[1];

                switch (cmd)
                {
                    case "g":
                        state = stateProcessor.GetState("silhouette1");
                        Console.WriteLine("State: " + state);
                        break;
                    case "u":
                        // Example: switch led on/off and set brightness
                        dynamic d = JObject.Parse(state);
                        d.state.led1 = (bool)(d.state.led1) ? false : true;
                        d.state.led1_level = int.Parse(arg);
                        state = JObject.FromObject(d).ToString();
                        // Update the state
                        stateProcessor.UpdateState("silhouette1", state);
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

