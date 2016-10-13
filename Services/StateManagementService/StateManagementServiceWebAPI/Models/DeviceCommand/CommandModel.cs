// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceRichState;
using Newtonsoft.Json.Linq;

namespace StateManagementServiceWebAPI.Models.DeviceCommand
{
    // TODO - add links

    /// <summary>
    /// Representation of a command with request and response (if received)
    /// </summary>
    public class CommandModel
    {
        /// <summary>
        /// The command Id - this is the correlation Id for the messages
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The device Id the command relates to
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// The Request specific properties
        /// </summary>
        public CommandRequestModel Request { get; set; }
        /// <summary>
        /// The Response specific properties. Will be null if there is no response
        /// </summary>
        public CommandResponseModel Response { get; set; }

        /// <summary>
        /// Create the command model from DeviceMessages. 
        /// </summary>
        /// <param name="messages">The request and (optional) response. Request should be the first message</param>
        public CommandModel(DeviceRichState.DeviceMessage[] messages)
        {
            if (messages == null || messages.Length < 1 || messages.Length > 2)
            {
                throw new ArgumentException("messages should be an array with 1 or 2 elements");
            }
            var request = messages.First(m => m != null && m.MessageType == MessageType.CommandRequest);
            var response = messages.FirstOrDefault(m => m != null && m.MessageType == MessageType.CommandResponse);

            Id = request.CorrelationId;
            DeviceId = request.DeviceId;
            Request = new CommandRequestModel(request);
            if (response == null)
            {
                Response = null;
            }
            else
            {
                Response = new CommandResponseModel(response);
            }
        }



    }


    /// <summary>
    /// Represent the request portion of a command
    /// </summary>
    public class CommandRequestModel
    {
        /// <summary>
        /// The 'version' for the message - this is a unique id within a device's messages
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// The enqueued time for the message (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The mesasge type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The message subtype
        /// </summary>
        public string Subtype { get; set; }
        /// <summary>
        /// The application-specific metadata stored with the command request
        /// </summary>
        public JToken AppMetadata { get; set; }
        /// <summary>
        /// The values/body for the command request message
        /// </summary>
        public JToken Values { get; set; }

        /// <summary>
        /// Construct the reponse representation from the response message
        /// </summary>
        /// <param name="request"></param>
        public CommandRequestModel(DeviceRichState.DeviceMessage request)
        {
            Version = request.Version;
            Timestamp = request.Timestamp;
            Type = request.MessageType.ToString();
            Subtype = request.MessageSubType.ToString();
            AppMetadata = string.IsNullOrEmpty(request.AppMetadata) ? null : JToken.Parse(request.AppMetadata);
            Values = string.IsNullOrEmpty(request.Values) ? null : JToken.Parse(request.Values);
        }
    }
    /// <summary>
    /// Represent the request portion of a command
    /// </summary>
    public class CommandResponseModel
    {
        /// <summary>
        /// The 'version' for the message - this is a unique id within a device's messages
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// The enqueued time for the message (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The mesasge type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The message subtype
        /// </summary>
        public string Subtype { get; set; }

        /// <summary>
        /// Construct the request representation from the request message
        /// </summary>
        /// <param name="response"></param>
        public CommandResponseModel(DeviceRichState.DeviceMessage response)
        {
            Version = response.Version;
            Timestamp = response.Timestamp;
            Type = response.MessageType.ToString();
            Subtype = response.MessageSubType.ToString();
        }
    }
}

