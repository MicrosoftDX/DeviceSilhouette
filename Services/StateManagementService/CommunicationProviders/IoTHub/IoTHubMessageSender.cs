// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using DeviceRichState;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    public class IotHubMessageSender : IMessageSender
    {
        private readonly ServiceClient _serviceClient;

        public IotHubMessageSender(string iotHubConnectionString)
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
        }
        public async Task SendCloudToDeviceAsync(DeviceMessage silhouetteMessage)
        {
            var messageBody = silhouetteMessage.Values == null ? new byte[0] : Encoding.UTF8.GetBytes(silhouetteMessage.Values);
            var commandMessage = new Message(messageBody)
            {
                Properties = {
                    { "MessageType", silhouetteMessage.MessageType.ToString() },
                    { "MessageSubType", silhouetteMessage.MessageSubType.ToString() }
                },
                // get full acknowledgement on message delivery
                Ack = DeliveryAcknowledgement.Full,
                ExpiryTimeUtc = DateTime.UtcNow.AddMilliseconds(silhouetteMessage.MessageTtlMs),
                MessageId = silhouetteMessage.CorrelationId,
                CorrelationId = silhouetteMessage.CorrelationId
            };

            await _serviceClient.SendAsync(silhouetteMessage.DeviceId, commandMessage);
        }
    }
}




