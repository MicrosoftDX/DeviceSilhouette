using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceRichState;

namespace PersistencyProviders
{
    public interface IHistoryStorage
    {
        Task StoreStateMessageAsync(DeviceState stateMessage);

        Task StoreStateMessagesAsync(List<DeviceState> stateMessages);
    }

}
