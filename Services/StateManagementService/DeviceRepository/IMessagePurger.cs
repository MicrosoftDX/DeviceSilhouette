using DeviceRichState;
using System.Collections.Generic;

namespace DeviceRepository
{
    public interface IMessagePurger
    {
        List<DeviceMessage> GetPurgableMessages(List<DeviceMessage> messages);
    }
}