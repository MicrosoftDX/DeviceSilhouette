// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.IO;

namespace DotNetClient
{
    public class DeviceMessage
    {

        public DeviceMessage(Message message)
        {
            Message = message;

            CorrelationId = message.CorrelationId;

            MessageType = message.Properties["MessageType"];
            MessageSubType = message.Properties["MessageSubType"];

            EnqueuedTimeUtc = message.EnqueuedTimeUtc;

            using (var reader = new StreamReader(message.BodyStream, Encoding.UTF8))
            {
                Body = reader.ReadToEnd();
            }
        }

        public string Body { get; private set; }
        public string CorrelationId { get; internal set; }
        public DateTime EnqueuedTimeUtc { get; set; }
        public string MessageType { get; internal set; }
        public string MessageSubType { get; internal set; }


        internal Message Message { get; set; }
    }
}

