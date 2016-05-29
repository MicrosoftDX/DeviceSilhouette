using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationProviders
{
    
    public interface ICommunicationProvider
    {
 
        Task<string> ReceiveDeviceToCloudAsync();

        Task SendCloudToDeviceAsync(string message, string DeviceID);
    }
}
