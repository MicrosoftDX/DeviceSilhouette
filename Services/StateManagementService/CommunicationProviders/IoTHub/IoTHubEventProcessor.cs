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
        private readonly Func<string, Task> _messageHandler;

        public IoTHubEventProcessor(Func<string, Task> messageHandler)
        {
            _messageHandler = messageHandler;
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
            var processingTasks = messages
                .Select(GetMessageStringFromEvent)
                .Select(_messageHandler);

            await Task.WhenAll(processingTasks);

            await context.CheckpointAsync();
        }

        private static string GetMessageStringFromEvent(EventData eventData)
        {
            byte[] data = eventData.GetBytes();
            string message = Encoding.UTF8.GetString(data);
            return message;
        }
    }
}
