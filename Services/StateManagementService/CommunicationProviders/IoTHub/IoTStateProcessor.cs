using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    internal class IoTStateProcessor
    {
        private ConcurrentQueue<string> d2cMessages = new ConcurrentQueue<string>();

        internal void processMessage(string message)
        {
            d2cMessages.Enqueue(message);
        }

        internal string getMessage()
        {
            string message;
            d2cMessages.TryDequeue(out message);
            return message;
        }
    }
}