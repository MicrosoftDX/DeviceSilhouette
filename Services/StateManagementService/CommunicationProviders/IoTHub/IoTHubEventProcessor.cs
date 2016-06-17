using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    class IoTHubEventProcessor : IEventProcessor
    {
        IoTStateProcessor state;

        public IoTHubEventProcessor(IoTStateProcessor _state)
        {
            state = _state;
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);
            return Task.FromResult((object)null);
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine("EventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);
            return Task.FromResult((object)null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                byte[] data = eventData.GetBytes();
                string message = Encoding.UTF8.GetString(data);
                string messageType = eventData.Properties.Keys.Contains("MessageType") ? eventData.Properties["MessageType"].ToString() : "null";

                Console.WriteLine(string.Format("Message received.  Partition: '{0}', MessageType: '{1}'", context.Lease.PartitionId, messageType));

                // Dispatch message
                switch (messageType)
                {
                    case "State:Set":
                        state.processMessage(message);
                        break;
                    case "State:Get":
                        state.processMessage(message);
                        break;
                    default:
                        Console.WriteLine("Unknown MessageType.");
                        break;
                }
            }

            await context.CheckpointAsync();
        }
    }
}
