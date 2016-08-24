using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silhouette.DeviceSimulator
{
    public class ReceiveMessageEventArgs : EventArgs
    {
        public DeviceMessage Message { get; private set; }
        public ReceiveMessageAction Action { get; set; }

        public ReceiveMessageEventArgs(DeviceMessage deviceMessage)
        {
            Message = deviceMessage;
            Action = ReceiveMessageAction.None;
        }
    }

    public enum ReceiveMessageAction
    {
        None,
        Complete,
        Reject,
        Abandon
    }
}
