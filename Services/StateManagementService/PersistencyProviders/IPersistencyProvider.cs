using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersistencyProviders
{
    public interface IHistoryStorage
    {
        Task StoreStateMessage(DeviceState stateMessage);

        Task StoreStateMessages(DeviceState[] stateMessages);
    }
}
