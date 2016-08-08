using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeviceRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceRichState;

namespace DeviceRepository.Tests
{
    [TestClass()]
    public class MessagePurgerTests_GetIndexOfLastPurgeableMessage
    {

        // TODO - add tests for scenarios to cover requirements in https://github.com/dx-ted-emea/pudding/wiki/3.7-Long-term-persistency-and-analytics

        [TestInitialize]
        public void Initialize()
        {
            SystemTime.Reset();
            _messageRetentionInMilliseconds = -1;            
            _minMessagesToKeep = -1;

        _messages = null;
        }



        [TestMethod()]
        public void WithMessagePurger_WhenNoMessagesInTheList_ThenNoMessagesAreIdentifiedToPurge()
        {
            var baseDateTime = new DateTime(2016, 07, 22, 10, 00, 00, DateTimeKind.Utc);
            var messages = new List<DeviceMessage>();

            WithSystemTimeUtc(baseDateTime);
            WithMessageRetentionOf(10 * Minutes);
            WithMinMessagesToKeep(3);
            WithMessages(messages);
            ExpectLastPurgeIndexToBe(-1);
        }

        [TestMethod()]
        public void WithMessagePurger_WhenNoMessagesAreOutsideTheRetentionPeriod_ThenNoMessagesAreIdentifiedToPurge()
        {
            var baseDateTime = new DateTime(2016, 07, 22, 10, 00, 00, DateTimeKind.Utc);

            WithSystemTimeUtc(baseDateTime);
            WithMessageRetentionOf(10 * Minutes);
            WithMinMessagesToKeep(3);
            var messages = new List<DeviceMessage>
            {
                /* index 0 */ ReportedState(baseDateTime + TimeSpan.FromMinutes(-9), persisted:true),
                /* index 1 */ ReportedState(baseDateTime + TimeSpan.FromMinutes(-8), persisted:true),
            };
            WithMessages(messages);
            ExpectLastPurgeIndexToBe(-1);
        }

        [TestMethod()]
        public void WithMessagePurger_WhenNoMessagesArePersisted_ThenNoMessagesAreIdentifiedToPurge()
        {
            var baseDateTime = new DateTime(2016, 07, 22, 10, 00, 00, DateTimeKind.Utc);

            WithSystemTimeUtc(baseDateTime);
            WithMessageRetentionOf(10 * Minutes);
            WithMinMessagesToKeep(3);
            var messages = new List<DeviceMessage>
            {
                /* index 0 */ ReportedState(baseDateTime + TimeSpan.FromMinutes(-20), persisted:false), // not persisted
                /* index 1 */ ReportedState(baseDateTime + TimeSpan.FromMinutes(-9), persisted:false),  // in retention window
            };
            WithMessages(messages);
            ExpectLastPurgeIndexToBe(-1);
        }

        [TestMethod()]
        public void WithMessagePurger_WhenAllMessagesArePersisted_ThenOnlyMessagesOutsideTheRetentionWindowArePurged()
        {
            var baseDateTime = new DateTime(2016, 07, 22, 10, 00, 00, DateTimeKind.Utc);

            WithSystemTimeUtc(baseDateTime);
            WithMessageRetentionOf(10 * Minutes);
            WithMinMessagesToKeep(2);
            var messages = new List<DeviceMessage>
            {
                // Index 0 is before the retention window, is persisted, and has later StateReport => can be purged
                /* index 0 */ ReportedState(baseDateTime + TimeSpan.FromMinutes(-20), persisted:true), 
                /* index 1 */ ReportedState(baseDateTime + TimeSpan.FromMinutes(-9), persisted:true), // in retention window
                /* index 2 */ ReportedState(baseDateTime + TimeSpan.FromMinutes(-6), persisted:true), // in retention window
            };
            WithMessages(messages);
            ExpectLastPurgeIndexToBe(0);
        }


        [TestMethod()]
        public void WithMessagePurger_WhenAnEarlierMessageIsNotPersisted_ThenLaterMessagesAreNotPurged()
        {
            var baseDateTime = new DateTime(2016, 07, 22, 10, 00, 00, DateTimeKind.Utc);

            WithSystemTimeUtc(baseDateTime);
            WithMessageRetentionOf(10 * Minutes);
            WithMinMessagesToKeep(3);
            var messages = new List<DeviceMessage>
            {
                // All messages are outside the retention window, but the initial message isn't persisted so can't be purged
                // There is a constraint that for any message retained, later messages must be retained
                // So none are purgeable in this scenario
                /* index 0 */ ReportedState(baseDateTime + TimeSpan.FromMinutes(-20), persisted:false), 
                /* index 1 */ ReportedState(baseDateTime + TimeSpan.FromMinutes(-19), persisted:true), // would be purgeable, but the earlier message isn't persisted
                /* index 2 */ ReportedState(baseDateTime + TimeSpan.FromMinutes(-16), persisted:true), // last state report => not purgeable
            };
            WithMessages(messages);
            ExpectLastPurgeIndexToBe(-1);
        }

        [TestMethod()]
        public void WithMessagePurger_WhenACommandMessageHasNoResponse_ThenTheCommandMessageIsNotPurged()
        {
            var baseDateTime = new DateTime(2016, 07, 22, 10, 00, 00, DateTimeKind.Utc);

            WithSystemTimeUtc(baseDateTime);
            WithMessageRetentionOf(10 * Minutes);
            WithMinMessagesToKeep(3);
            var messages = new List<DeviceMessage>
            {
                // Whilst the command is persisted and outside the retention window, it doesn't have a response so cannot be purged
                /* index 0 */ ReportedState (baseDateTime + TimeSpan.FromMinutes(-20), persisted:true), // safe to purge
                /* index 1 */ Command       (baseDateTime + TimeSpan.FromMinutes(-19), persisted:true, correlationId: "correlation1"),
                /* index 2 */ ReportedState (baseDateTime + TimeSpan.FromMinutes(-16), persisted:true), 
                /* index 3 */ ReportedState (baseDateTime + TimeSpan.FromMinutes(-11), persisted:true),                 
            };
            WithMessages(messages);
            ExpectLastPurgeIndexToBe(0);
        }


        [TestMethod()]
        public void WithMessagePurger_WhenAMessageIsNotPurged_ThenOtherMessagesWithTheSameCorrelationIdAreNotPurged()
        {
            var baseDateTime = new DateTime(2016, 07, 22, 10, 00, 00, DateTimeKind.Utc);

            WithSystemTimeUtc(baseDateTime);
            WithMessageRetentionOf(10 * Minutes);
            WithMinMessagesToKeep(3);
            var messages = new List<DeviceMessage>
            {
                // Whilst the command is persisted and outside the retention window, 
                // it has a response that is in the retention window so can't be purged
                /* index 0 */ ReportedState (baseDateTime + TimeSpan.FromMinutes(-20), persisted:true), // safe to purge
                /* index 1 */ Command       (baseDateTime + TimeSpan.FromMinutes(-19), persisted:true, correlationId: "correlation1"),
                /* index 2 */ Response      (baseDateTime + TimeSpan.FromMinutes(-9), persisted:true, correlationId: "correlation1"), 
                /* index 3 */ ReportedState (baseDateTime + TimeSpan.FromMinutes(-8), persisted:true),
            };
            WithMessages(messages);
            ExpectLastPurgeIndexToBe(0);
        }

        #region helpers
        private int _messageRetentionInMilliseconds;        
        private int _minMessagesToKeep;
        private List<DeviceMessage> _messages;

        private const int Seconds = 1000;
        private const int Minutes = 60 * Seconds;

        private const string DeviceId = "a-device";        

        private DeviceMessage ReportedState(DateTime timestamp, bool persisted)
        {
            var message = DeviceMessage.CreateReport(
                DeviceId,
                "{}",
                ReportMessageSubType.State,
                timestamp: timestamp
                );
            message.Persisted = persisted;
            return message;
        }
        private DeviceMessage Command(DateTime timestamp, bool persisted, string correlationId)
        {
            var message = DeviceMessage.CreateCommandRequest(
                DeviceId,
                "{}",
                "{}",
                CommandRequestMessageSubType.SetState,
                -1,
                correlationId,
                timestamp
                );
            message.Persisted = persisted;
            return message;
        }
        private DeviceMessage Response(DateTime timestamp, bool persisted, string correlationId)
        {
            var message = DeviceMessage.CreateCommandResponse(
                DeviceId,
                "{}",
                "{}",
                CommandResponseMessageSubType.Acknowledged,
                -1,
                correlationId,
                timestamp
                );
            message.Persisted = persisted;
            return message;
        }
        private void WithSystemTimeUtc(DateTime systemDateTime)
        {
            SystemTime.UtcNow = () => systemDateTime;
        }

        private void WithMessageRetentionOf(int messageRetentionInMilliseconds)
        {
            _messageRetentionInMilliseconds = messageRetentionInMilliseconds;
        }
        private void WithMinMessagesToKeep(int minMessagesToKeep)
        {
            _minMessagesToKeep = minMessagesToKeep;
        }
        private void WithMessages(List<DeviceMessage> messages)
        {
            _messages = messages;
        }
        private void ExpectLastPurgeIndexToBe(int expectedIndex)
        {
            var messagePurger = new MessagePurger(_messageRetentionInMilliseconds, _minMessagesToKeep);
            int actualIndex = messagePurger.GetIndexOfLastPurgeableMessage(_messages);
            Assert.AreEqual(expectedIndex, actualIndex);
        }

        #endregion
    }
}