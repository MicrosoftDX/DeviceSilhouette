using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using DeviceRepository.Interfaces;
using DeviceStateNamespace;

namespace DeviceRepository
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class DeviceRepositoryActor : Actor, IDeviceRepositoryActor
    {
        public Task<DeviceState> GetDeviceStateAsync()
        {
           return this.StateManager.GetStateAsync<DeviceState>("silhouette");
        }

        public Task<List<DeviceState>> GetDeviceStateMessagesAsync()
        {

            return StateManager.GetStateAsync<List<DeviceState>>("silhouetteMessages");
        }

        public Task SetDeviceStateAsync(DeviceState state)
        {
            var lastState = this.StateManager.GetStateAsync<DeviceState>("silhouette").Result;

            if (Convert.ToUInt64(lastState.Version) < Int64.MaxValue)
            {
                state.Version = (Convert.ToInt64(lastState.Version) + 1).ToString();
            }
            else
            {
                state.Version = "1";
            }
            state.Timestamp = DateTime.Now;
            state.DeviceID = this.GetActorId().ToString();

            AddDeviceMessageAsync(state);
            
            return this.StateManager.SetStateAsync("silhouette", state);
        }

        Task AddDeviceMessageAsync(DeviceState state)
        {
            List<DeviceState> messages = new List<DeviceState>();
            Microsoft.ServiceFabric.Data.ConditionalValue<List<DeviceState>> _messages;
            _messages = StateManager.TryGetStateAsync<List<DeviceState>>("silhouetteMessages").Result;
            if (_messages.HasValue) { messages = _messages.Value; };

            messages.Add(state);
            return StateManager.SetStateAsync<List<DeviceState>>("silhouetteMessages", messages);

        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see http://aka.ms/servicefabricactorsstateserialization

            return this.StateManager.TryAddStateAsync("silhouette", new DeviceState());
        }

    }
}
