using DeviceRichState;
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
        private readonly Func<MessageInfo, Task> _messageHandler;

        public IoTHubEventProcessor(Func<MessageInfo, Task> messageHandler)
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
                .Select(ToMessageInfo)
                .Where(m=>m!=null)
                .Select(_messageHandler);

            await Task.WhenAll(processingTasks);

            await context.CheckpointAsync();
        }

        private static MessageInfo ToMessageInfo(EventData eventData)
        {
            string deviceId;
            if (!TryGetStringValue(eventData.SystemProperties, "iothub-connection-device-id", out deviceId))
            {
                // TODO - log failure. (also move to alternative queue for handling?)
                return null;
            }

            string correlationId;
            TryGetStringValue(eventData.SystemProperties, "correlation-id", out correlationId); // OK if correlationId doesn't exist!
            // TODO SL - get the message Id - assume that this will become the correlation id for responses if correlationId not set

            string messageTypeString;
            if (!TryGetStringValue(eventData.Properties, "MessageType", out messageTypeString))
            {
                // TODO - log failure. (also move to alternative queue for handling?)
                return null;
            }
            MessageType messageType;
            if (!Enum.TryParse(messageTypeString, ignoreCase: true, result:out messageType))
            {
                // TODO - log failure. (also move to alternative queue for handling?)
                return null;
            }

            string messageSubTypeString;
            if (!TryGetStringValue(eventData.Properties, "MessageSubType", out messageSubTypeString))
            {
                // TODO - log failure. (also move to alternative queue for handling?)
                return null;
            }
            MessageSubType messageSubType;
            if (!Enum.TryParse(messageSubTypeString, ignoreCase: true, result: out messageSubType))
            {
                // TODO - log failure. (also move to alternative queue for handling?)
                return null;
            }

            var message = new MessageInfo
            {
                DeviceId = deviceId,
                CorrelationId = correlationId,
                MessageType = messageType,
                MessageSubType = messageSubType,
                EnqueuedTimeUtc = eventData.EnqueuedTimeUtc,
                Body = GetMessageStringFromEvent(eventData),
                RawProperties = eventData.Properties,
            };

            return message;
        }
        private static bool TryGetStringValue(IDictionary<string, object> dictionary, string key, out string value)
        {
            object tempValue;
            if (dictionary.TryGetValue(key, out tempValue))
            {
                value = (string)tempValue;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
        private static string GetMessageStringFromEvent(EventData eventData)
        {
            byte[] data = eventData.GetBytes();
            string message = Encoding.UTF8.GetString(data);
            return message;
        }
    }
}
