using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventProcessor
{
    class TestEventProcessor : IEventProcessor
    {
        public static string StorageConnectionString;

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);
        }

        public async Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine("EventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                byte[] data = eventData.GetBytes();
                string deviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();
                string messageType = eventData.Properties.Keys.Contains("MessageType") ? eventData.Properties["MessageType"].ToString() : "null";

                Console.WriteLine(string.Format("Message received.  Partition: '{0}', DeviceID: '{1}', MessageType: '{2}', Data: '{3}'",
                    context.Lease.PartitionId, deviceId, messageType, Encoding.UTF8.GetString(data)));

                // Dispatch message
                switch (messageType)
                {
                    case "D2C_UpdateState":
                        await ProcessD2CUpdateState(deviceId, Encoding.UTF8.GetString(data));
                        break;
                    case "D2C_GetState":
                        await ProcessD2CGetState(deviceId, Encoding.UTF8.GetString(data));
                        break;
                    default:
                        Console.WriteLine("Unknown MessageType.");
                        break;
                }
            }
        }

        public async Task ProcessD2CUpdateState(string deviceId, string data)
        {
            Console.WriteLine("Update State");
        }

        public async Task ProcessD2CGetState(string deviceId, string data)
        {
            Console.WriteLine("Get State");
        }

    }
}
