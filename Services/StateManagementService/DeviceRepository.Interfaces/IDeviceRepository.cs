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
        /// This method reads the most recent state message from the Silhouette
        /// </summary>
        /// <returns>Object that contains the last stored requested or reported device state</returns>
        Task<DeviceMessage> GetDeviceStateAsync();

        /// <summary>
        /// This method stores a state message in the Silhouette
        /// </summary>
        /// <param name="state">Object that contains the requested or reported device state</param>
        /// <returns></returns>
        Task<DeviceMessage> SetDeviceStateAsync(DeviceMessage state);

        /// <summary>
        /// This method reads the available state history from the Silhouette
        /// </summary>
        /// <returns>List of DeviceState objects</returns>
        Task<List<DeviceMessage>> GetDeviceStateMessagesAsync();
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
}
