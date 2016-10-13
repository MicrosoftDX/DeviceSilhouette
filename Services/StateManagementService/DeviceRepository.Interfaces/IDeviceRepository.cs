// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using DeviceRichState;

namespace DeviceRepository.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IDeviceRepositoryActor : IActor
    {
        Task<DeviceMessage> GetLastKnownReportedStateAsync();
        Task<DeviceMessage> GetLastKnownRequestedStateAsync();

        /// <summary>
        /// This method stores a device message in the Silhouette
        /// </summary>
        /// <param name="message">the message to store</param>
        /// <returns></returns>
        Task<DeviceMessage> StoreDeviceMessageAsync(DeviceMessage message);

        /// <summary>
        /// This method reads the available message history from the Silhouette
        /// </summary>
        /// <returns>List of DeviceMessage objects</returns>
        Task<List<DeviceMessage>> GetDeviceMessagesAsync();
        /// <summary>
        /// Get a specific message by version number
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<DeviceMessage> GetMessageByVersionAsync(int version);
        /// <summary>
        /// Get a paged set of messages from the actor
        /// </summary>
        /// <param name="pageSize">The maximum number of messages to return</param>
        /// <param name="continuation">A token that indicates the next message to start at</param>
        /// <returns></returns>
        Task<MessageList> GetMessagesAsync(int pageSize, int? continuation);
        /// <summary>
        /// Get all messages by correlationId
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        Task<DeviceMessage[]> GetMessagesWithCorrelationIdAsync(string correlationId);
        /// <summary>
        /// Get command messages
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        Task<CommandList> GetCommandMessagesAsync(int pageSize, string continuationToken);
    }

    public class MessageList
    {
        /// <summary>
        /// The set of messages
        /// </summary>
        public List<DeviceMessage> Messages { get; set; }
        /// <summary>
        /// The continuation token to pass to retrieve the next set of messages
        /// </summary>
        public int? Continuation { get; set; }
    }

    public class CommandList
    {
        /// <summary>
        /// The command messages. Each item in the list is an array containing the messages for a command
        /// </summary>
        public List<DeviceMessage[]> Messages { get; set; }
        /// <summary>
        /// The continuation token to pass to retrieve the next set of commands
        /// </summary>
        public string Continuation { get; set; }
    }
}

