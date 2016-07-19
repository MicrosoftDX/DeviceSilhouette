using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceRichState;

namespace StorageProviders
{
    public interface IHistoryStorage
    {
        Task StoreStateMessage(DeviceState stateMessage);

        Task StoreStateMessages(DeviceState[] stateMessages);
    }

}
