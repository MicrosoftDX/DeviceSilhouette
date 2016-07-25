using DeviceRichState;
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
        // read messages from the communication provider endpoint
        Task RunAsync(CancellationToken cancellationToken);
    }
    public interface IMessageSender
    {
        /* send messages to the communication provider endpoint
         * DeviceId - device it to set message to
         * MessageType - State:Set or State:Get
         * Meesage - message json string         
        */
        Task SendCloudToDeviceAsync(DeviceState silhouetteMessage);
    }

    public interface IFeedbackReceiver
    {
        // process feedbacks from C2D messages
        Task ReceviceFeedback(CancellationToken cancellationToken);
    }
}
