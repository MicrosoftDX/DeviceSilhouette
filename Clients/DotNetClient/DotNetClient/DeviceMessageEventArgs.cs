using System;

namespace DotNetClient
{
    public class ReceiveMessageEventArgs : EventArgs
    {
        public DeviceMessage Message { get; private set; }
        public ReceiveMessageAction Action { get; set; }

        public ReceiveMessageEventArgs(DeviceMessage deviceMessage)
        {
            Message = deviceMessage;
            Action = ReceiveMessageAction.Complete;
        }
    }

    public enum ReceiveMessageAction
    {
        Complete,
        Reject,
        Abandon
    }
}