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
        IoTStateProcessor state;

        public IoTHubEventProcessorFactory(IoTStateProcessor _state)
        {
            state = _state;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new IoTHubEventProcessor(state);
        }
    }
}
