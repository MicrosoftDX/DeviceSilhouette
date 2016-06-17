using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using DeviceStateNamespace;

namespace DeviceRepository.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IDeviceRepositoryActor : IActor
    {
        /// <summary>
        /// This method reads the current state from the Silhouette
        /// </summary>
        /// <returns>Object that contains the stored requested or reported device state</returns>
        Task<DeviceState> GetDeviceStateAsync();

        /// <summary>
        /// This method sets the current state of the Silhouette
        /// </summary>
        /// <param name="state">Object that contains the requested or reported device state</param>
        /// <returns></returns>
        Task<DeviceState> SetDeviceStateAsync(DeviceState state);

        /// <summary>
        /// This method reads the available state history from the Silhouette
        /// </summary>
        /// <returns>List of DeviceState objects</returns>
        Task<List<DeviceState>> GetDeviceStateMessagesAsync();
    }
}
