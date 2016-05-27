using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationProviders
{

    public interface IMessage
    {
    }
    public interface ICommunicationProvider
    {
 
        Task<IMessage> ReceiveDeviceToCloudAsync();

        Task SendCloudToDeviceAsync(IMessage message, string DeviceID);
    }
}
