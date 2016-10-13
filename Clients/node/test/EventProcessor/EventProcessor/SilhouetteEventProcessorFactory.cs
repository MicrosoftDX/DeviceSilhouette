// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventProcessor
{
    class SilhouetteEventProcessorFactory : IEventProcessorFactory
    {
        SilhouetteStateProcessor _processor;

        public SilhouetteEventProcessorFactory(SilhouetteStateProcessor processor)
        {
            _processor = processor;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new SilhouetteEventProcessor(_processor);
        }
    }
}

