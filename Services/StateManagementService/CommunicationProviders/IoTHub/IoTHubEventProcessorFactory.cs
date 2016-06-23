using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    class IoTHubEventProcessorFactory : IEventProcessorFactory
    {
        private readonly Func<string, Task> _messageHandler;

        public IoTHubEventProcessorFactory(Func<string, Task> messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new IoTHubEventProcessor(_messageHandler);
        }
    }
}
