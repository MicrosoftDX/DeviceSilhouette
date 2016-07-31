using DeviceRichState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceRepository
{
    public class MessagePurger : IMessagePurger
    {
        private readonly double _messagesRetentionMilliseconds;

        public MessagePurger(double messagesRetentionMilliseconds)
        {
            _messagesRetentionMilliseconds = messagesRetentionMilliseconds;
        }        

        /// <summary>
        /// Return a list of messages to purge based on purge conditions. 
        /// Requirements in https://github.com/dx-ted-emea/pudding/wiki/3.7-Long-term-persistency-and-analytics        
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>

        public List<DeviceMessage> GetPurgableMessages(List<DeviceMessage> messages)
        {
            List<DeviceMessage> purgeMessages = new List<DeviceMessage>();
            
            // last reported message
            DeviceMessage lastReported = messages.OrderByDescending(m => m.Timestamp)
                                                    .Where(m => m.MessageType == MessageType.Report)
                                                    .FirstOrDefault();

            var latestMessageTimeStampToPurge = SystemTime.UtcNow().AddMilliseconds(-_messagesRetentionMilliseconds);

            messages.ForEach(m => checkMessage(m, lastReported, latestMessageTimeStampToPurge, messages, purgeMessages));
                       
            // if retaining a message than all messages with same correlation id should be retained and all later messages
            var retained = messages.Except(purgeMessages).ToList<DeviceMessage>();
            retained.ForEach(m => purgeMessages.RemoveAll(p => retainMessage(m, p)));            
           
            return purgeMessages;
        }

        private bool retainMessage(DeviceMessage retained, DeviceMessage purged)
        {
            return (retained.CorrelationId == purged.CorrelationId) && (purged.Timestamp >= retained.Timestamp);
        }

        private void checkMessage(DeviceMessage message, DeviceMessage lastReported, DateTime latestMessageTimeStampToPurge, List<DeviceMessage> messages, List<DeviceMessage> purgeMessages)
        {
            // message was persisted, message is older than the retention time limit and this is not the latest reorted state   
            if (!message.Equals(lastReported) && message.Persisted && (message.Timestamp < latestMessageTimeStampToPurge))
            {
                // persist command request with no response
                if (message.MessageType.Equals(MessageType.CommandRequest))
                {
                    // find all messages with same correlation id and type == response
                    var res = messages.FindAll(m => (m.CorrelationId == message.CorrelationId) && m.MessageType.Equals(MessageType.CommandResponse));
                    if (res.Count != 0)
                    {
                        purgeMessages.Add(message);
                    }
                }
                else
                {
                    purgeMessages.Add(message);
                }
            }
        }
       

        public int GetIndexOfLastPurgeableMessage(List<DeviceMessage> messages)
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
