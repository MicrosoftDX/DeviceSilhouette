using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationProviders
{
    public interface IMessageReceiver
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
    public interface IMessageSender
    { 
        Task SendCloudToDeviceAsync(string DeviceId, string MessageType, string Message);
    }
}
