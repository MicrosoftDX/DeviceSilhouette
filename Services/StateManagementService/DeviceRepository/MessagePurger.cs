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
        private readonly long _messagesRetentionMilliseconds;
        private readonly int _minMessagesToKeep;

        public MessagePurger(long messagesRetentionMilliseconds, int minMessagesToKeep)
        {
            _messagesRetentionMilliseconds = messagesRetentionMilliseconds;
            _minMessagesToKeep = minMessagesToKeep;
        }

        public void Purge(List<DeviceMessage> messages)
        {            
            var indexOfLastPurgeableMessage = GetIndexOfLastPurgeableMessage(messages);
            if (indexOfLastPurgeableMessage >= 0)
            {
                messages.RemoveRange(0, indexOfLastPurgeableMessage + 1);
            }
        }

        public int GetIndexOfLastPurgeableMessage(List<DeviceMessage> messages)
        {
            // Handle requirements in https://github.com/dx-ted-emea/pudding/wiki/3.7-Long-term-persistency-and-analytics


            if (messages.Count == 0 || messages.Count <= _minMessagesToKeep)
            {
                return -1;
            }

            int latestReportedStateMessageIndex = -1;

            DateTime latestMessageTimeStampToPurge = SystemTime.UtcNow().AddMilliseconds(-_messagesRetentionMilliseconds);
            bool gotMessageInRetentionTimeWindow = false;
            int latestMessageBeforeRetentionTimeWindowIndex = -1;
            int latestMessageBeforeMinimumMessagesIndex = messages.Count - _minMessagesToKeep - 1;

            if (messages[0].Timestamp > latestMessageTimeStampToPurge)
            {
                // fail fast!
                return -1;
            }

            // Can't persist after the latest persisted message in a chain of persisted messages
            // e.g. in the following sequence lastPersistedMessageInSequenceIndex would be 2 as that is the 
            //      last message that is persisted before the non-persisted message
            //   index     0 1 2 3 4 5 6
            //   persisted y y y n y y y
            bool gotNonPersistedMessage = false;
            int lastPersistedMessageInSequenceIndex = -1;

            // Track the first index for a message with a given correlation Id
            // key = correlation Id, value = earliest index
            var earliestIndexForCorrelationIdLookup = new Dictionary<string, int>();
            int earliestCorrelatedMessageIndex = messages.Count;

            // Track index for command requests. Remove when we encounter the response
            // key = correlation Id, value = index of command request
            var commandRequestIndices = new Dictionary<string, int>();
            int earliestCommandRequestWithoutResponseIndex = -1;

            //
            // Loop over messages tracking states above
            //
            for (int messageIndex = 0; messageIndex < messages.Count; messageIndex++)
            {
                var message = messages[messageIndex];

                // latestReportedStateMessageIndex
                if (message.MessageType == MessageType.Report && message.ReportMessageSubType() == ReportMessageSubType.State)
                {
                    latestReportedStateMessageIndex = messageIndex;
                }

                // lastPersistedMessageInSequenceIndex
                if (!gotNonPersistedMessage && !message.Persisted)
                {
                    gotNonPersistedMessage = true;
                    lastPersistedMessageInSequenceIndex = messageIndex - 1;
                }

                // latestMessageBeforeRetentionTimeWindowIndex
                if (message.Timestamp > latestMessageTimeStampToPurge
                     && !gotMessageInRetentionTimeWindow)
                {
                    gotMessageInRetentionTimeWindow = true;
                    latestMessageBeforeRetentionTimeWindowIndex = messageIndex - 1;
                }

                // earliestIndexForCorrelationId
                if (!string.IsNullOrEmpty(message.CorrelationId))
                {
                    if (message.Timestamp > latestMessageTimeStampToPurge
                        || messageIndex > latestMessageBeforeMinimumMessagesIndex)
                    {
                        // we're in the retention window (time or message count)
                        // we don't need to track the correlation IDs any more, 
                        // but we need to check that we retain messages with this correlation ID outside the retention window
                        int earliestIndexForCurrentCorrelationId;
                        if (earliestIndexForCorrelationIdLookup.TryGetValue(message.CorrelationId, out earliestIndexForCurrentCorrelationId))
                        {
                            earliestCorrelatedMessageIndex = Math.Min(earliestCorrelatedMessageIndex, earliestIndexForCurrentCorrelationId);
                        }
                    }
                    else
                    {
                        if (!earliestIndexForCorrelationIdLookup.ContainsKey(message.CorrelationId))
                        {
                            // this correlation id isn't in the dictionary, so this is the first index
                            earliestIndexForCorrelationIdLookup[message.CorrelationId] = messageIndex;
                        }
                    }
                }

                // earliestCommandRequestWithoutResponseIndex
                if (message.MessageType == MessageType.CommandRequest)
                {
                    // store request
                    commandRequestIndices[message.CorrelationId] = messageIndex;
                }
                else if (message.MessageType == MessageType.CommandResponse)
                {
                    // remove request as we have a response
                    commandRequestIndices.Remove(message.CorrelationId);
                }
            }


            //
            // Now combine the states
            //
            earliestCommandRequestWithoutResponseIndex = commandRequestIndices.Count == 0
                                                            ? messages.Count
                                                            : commandRequestIndices.Values.Min();
            if (!gotNonPersistedMessage)
            {
                lastPersistedMessageInSequenceIndex = messages.Count - 1;
            }
            if (latestReportedStateMessageIndex < 0)
            {
                latestReportedStateMessageIndex = messages.Count - 1;
            }
            if (!gotMessageInRetentionTimeWindow)
            {
                latestMessageBeforeRetentionTimeWindowIndex = messages.Count - 1;
            }

            int result = Min(
                latestReportedStateMessageIndex,
                latestMessageBeforeRetentionTimeWindowIndex,
                latestMessageBeforeMinimumMessagesIndex,
                lastPersistedMessageInSequenceIndex,
                earliestCorrelatedMessageIndex - 1,
                earliestCommandRequestWithoutResponseIndex - 1
            );

            return result;
        }

        public int Min(int value1, int value2, int value3, int value4, int value5, int value6)
        {
            return Min(Min(value1, value2, value3, value4), Min(value5, value6));
        }
        public int Min(int value1, int value2, int value3, int value4, int value5)
        {
            return Min(Min(value1, value2, value3, value4), value5);
        }
        public int Min(int value1, int value2, int value3, int value4)
        {
            return Min(Min(value1, value2), Min(value3, value4));
        }
        public int Min(int value1, int value2, int value3)
        {
            return Min(Min(value1, value2), value3);
        }
        public int Min(int value1, int value2)
        {
            return Math.Min(value1, value2);
        }
    }
}

