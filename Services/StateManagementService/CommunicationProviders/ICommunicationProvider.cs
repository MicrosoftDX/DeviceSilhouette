using CommonUtils;
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
        Task SendCloudToDeviceAsync(DeviceMessage silhouetteMessage);
    }

    public interface IFeedbackReceiver
    {
        // process feedbacks from C2D messages
        Task ReceviceFeedbackAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Abstraction over the provider message format
    /// </summary>
    public class MessageInfo
    {
        public string Body { get; set; }
        public string CorrelationId { get; internal set; }
        public string DeviceId { get; internal set; }
        public DateTime EnqueuedTimeUtc { get; set; }
        public MessageType MessageType { get; internal set; }
        public string MessageSubType { get; internal set; }
        public IDictionary<string, object> RawProperties { get; set; }

        public ReportMessageSubType ReportMessageSubType()
        {
            if (MessageType != MessageType.Report)
            {
                throw new InvalidOperationException($"Can only call {nameof(ReportMessageSubType)} when MessageType is {nameof(MessageType.Report)}");
            }
            return EnumUtils.ConstrainedParse<ReportMessageSubType>(MessageSubType);
        }
        public InquiryMessageSubType InquiryMessageSubType()
        {
            if (MessageType != MessageType.Inquiry)
            {
                throw new InvalidOperationException($"Can only call {nameof(InquiryMessageSubType)} when MessageType is {nameof(MessageType.Inquiry)}");
            }
            return EnumUtils.ConstrainedParse<InquiryMessageSubType>(MessageSubType);
        }
        public CommandRequestMessageSubType CommandRequestMessageSubType()
        {
            if (MessageType != MessageType.CommandRequest)
            {
                throw new InvalidOperationException($"Can only call {nameof(CommandRequestMessageSubType)} when MessageType is {nameof(MessageType.CommandRequest)}");
            }
            return EnumUtils.ConstrainedParse<CommandRequestMessageSubType>(MessageSubType);
        }
        public CommandResponseMessageSubType CommandResponseMessageSubType()
        {
            if (MessageType != MessageType.CommandResponse)
            {
                throw new InvalidOperationException($"Can only call {nameof(CommandResponseMessageSubType)} when MessageType is {nameof(MessageType.CommandResponse)}");
            }
            return EnumUtils.ConstrainedParse<CommandResponseMessageSubType>(MessageSubType);
        }
    }
}
