using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeviceRichState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;

namespace DeviceRichState.Tests
{
    [TestClass()]
    public class DeviceState_SerializationTests
    {
        private DeviceState OriginalMessage;
        private DeviceState DeserializedMessage;

        [TestInitialize]
        public void Initialize()
        {
            OriginalMessage = new DeviceState(
                "deviceIdWibble",
                "{\"metadataValue\":123}",
                "{\"state\":123}",
                MessageType.Reported,
                MessageStatus.DeliveryCountExceeded,
                "ACorrelationId")
            {
                _timestamp = DateTime.UtcNow.AddDays(-123)
            };


            var serializer = new DataContractJsonSerializer(typeof(DeviceState)); // TODO - can we find out the settings that SF uses to align the tests with those?
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, OriginalMessage);
                stream.Seek(0, SeekOrigin.Begin);
                DeserializedMessage = (DeviceState) serializer.ReadObject(stream);
            }
        }

        [TestMethod()]
        public void AppMetadataMatches()
        {
            Assert.AreEqual(OriginalMessage.AppMetadata, DeserializedMessage.AppMetadata);
        }
        [TestMethod()]
        public void CorrelationIdMatches()
        {
            Assert.AreEqual(OriginalMessage.CorrelationId, DeserializedMessage.CorrelationId);
        }
        [TestMethod()]
        public void DeviceIdMatches()
        {
            Assert.AreEqual(OriginalMessage.DeviceId, DeserializedMessage.DeviceId);
        }
        [TestMethod()]
        public void MessageStatusMatches()
        {
            Assert.AreEqual(OriginalMessage.MessageStatus, DeserializedMessage.MessageStatus);
        }
                [TestMethod()]
        public void MessageTypeMatches()
        {
            Assert.AreEqual(OriginalMessage.MessageType, DeserializedMessage.MessageType);
        }
        [TestMethod()]
        public void PersistedMatches()
        {
            Assert.AreEqual(OriginalMessage.Persisted, DeserializedMessage.Persisted);
        }
        [TestMethod()]
        public void ValuesMatches()
        {
            Assert.AreEqual(OriginalMessage.Values, DeserializedMessage.Values);
        }
    }
}