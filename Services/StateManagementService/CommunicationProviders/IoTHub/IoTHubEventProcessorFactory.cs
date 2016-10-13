// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
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
        private readonly Func<MessageInfo, Task> _messageHandler;

        public IoTHubEventProcessorFactory(Func<MessageInfo, Task> messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new IoTHubEventProcessor(_messageHandler);
        }
    }
}

