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
        Task StoreStateMessageAsync(DeviceMessage stateMessage);

        Task StoreStateMessagesAsync(List<DeviceMessage> stateMessages);
    }

}
