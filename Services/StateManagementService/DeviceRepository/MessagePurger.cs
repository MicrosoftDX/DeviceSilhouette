using DeviceRichState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceRepository
{
    public class MessagePurger
    {
        private readonly double _messagesRetentionMilliseconds;

        public MessagePurger(double messagesRetentionMilliseconds)
        {
            _messagesRetentionMilliseconds = messagesRetentionMilliseconds;
        }

        public int GetIndexOfLastPurgeableMessage(List<DeviceState> messages)
        {
            var latestReportedStateMessageIndex = messages.FindLastIndex(m => m.MessageType == MessageType.Report);
            var latestMessageTimeStampToPurge = SystemTime.UtcNow().AddMilliseconds(-_messagesRetentionMilliseconds);
            var latestPersistedMessageBeforeRetentionTimeWindow = messages.FindLastIndex(m =>
            {
                return m.Persisted
                        && m.Timestamp < latestMessageTimeStampToPurge;
            });

            if (latestReportedStateMessageIndex < 0)
            {
                // no reported state messages
                return latestPersistedMessageBeforeRetentionTimeWindow;
            }
            else
            {
                return Math.Min(latestReportedStateMessageIndex, latestPersistedMessageBeforeRetentionTimeWindow);
            }

            // TODO - handle requirements in https://github.com/dx-ted-emea/pudding/wiki/3.7-Long-term-persistency-and-analytics
        }
    }
}
