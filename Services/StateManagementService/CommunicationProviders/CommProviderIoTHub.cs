using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationProviders
{
    class CommProviderIoTHub: ICommunicationProvider
    {
        public Task<IMessage> ReceiveDeviceToCloudAsync()
        {
            throw new NotImplementedException();
        }

        Task ICommunicationProvider.SendCloudToDeviceAsync(IMessage message, string DeviceID)
        {
            throw new NotImplementedException();
        }

    }
}
