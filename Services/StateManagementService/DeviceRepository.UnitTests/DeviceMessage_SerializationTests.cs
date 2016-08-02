using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeviceRichState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;

namespace DeviceRichState.Tests
{
    [TestClass()]
    public class DeviceMessage_SerializationTests
    {
        private DeviceMessage OriginalMessage;
        private DeviceMessage DeserializedMessage;

        [TestInitialize]
        public void Initialize()
        {
            OriginalMessage = DeviceMessage.CreateCommandRequest(
                "deviceIdWibble",
                "{\"metadataValue\":123}",
                "{\"state\":123}",
                CommandRequestMessageSubType.SetState,
                123456,
                "ACorrelationId",
                DateTime.UtcNow.AddDays(-123)
                );
            OriginalMessage.Version = 67890;
            OriginalMessage.Persisted = true;


            var serializer = new DataContractSerializer(typeof(DeviceMessage)); // TODO - can we find out the settings that SF uses to align the tests with those?
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, OriginalMessage);
                stream.Seek(0, SeekOrigin.Begin);
                DeserializedMessage = (DeviceMessage) serializer.ReadObject(stream);
            }
        }

        [TestMethod()]
        public void WithDeviceMessage_WhenDeserializing_ThenAppMetadataMatches()
        {
            Assert.AreEqual(OriginalMessage.AppMetadata, DeserializedMessage.AppMetadata);
        }
        [TestMethod()]
        public void WithDeviceMessage_WhenDeserializing_ThenCorrelationIdMatches()
        {
            Assert.AreEqual(OriginalMessage.CorrelationId, DeserializedMessage.CorrelationId);
        }
        [TestMethod()]
        public void WithDeviceMessage_WhenDeserializing_ThenDeviceIdMatches()
        {
            Assert.AreEqual(OriginalMessage.DeviceId, DeserializedMessage.DeviceId);
        }
        [TestMethod()]
        public void WithDeviceMessage_WhenDeserializing_ThenMessageStatusMatches()
        {
            Assert.AreEqual(OriginalMessage.MessageSubType, DeserializedMessage.MessageSubType);
        }
        [TestMethod()]
        public void WithDeviceMessage_WhenDeserializing_ThenMessageTypeMatches()
        {
            Assert.AreEqual(OriginalMessage.MessageType, DeserializedMessage.MessageType);
        }
        [TestMethod()]
        public void WithDeviceMessage_WhenDeserializing_ThenPersistedMatches()
        {
            Assert.AreEqual(OriginalMessage.Persisted, DeserializedMessage.Persisted);
        }
        [TestMethod()]
        public void WithDeviceMessage_WhenDeserializing_ThenValuesMatches()
        {
            Assert.AreEqual(OriginalMessage.Values, DeserializedMessage.Values);
        }
        [TestMethod()]
        public void WithDeviceMessage_WhenDeserializing_ThenMessageTtlMsMatches()
        {
            Assert.AreEqual(OriginalMessage.MessageTtlMs, DeserializedMessage.MessageTtlMs);
        }
        [TestMethod()]
        public void WithDeviceMessage_WhenDeserializing_ThenVersionMatches()
        {
            Assert.AreEqual(OriginalMessage.Version, DeserializedMessage.Version);
        }
    }
}