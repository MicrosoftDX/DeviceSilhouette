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
           return StateManager.GetStateAsync<DeviceState>("silhouette");
        }

        public Task<List<DeviceState>> GetDeviceStateMessagesAsync()
        {
            return StateManager.GetStateAsync<List<DeviceState>>("silhouetteMessages");
        }

        public async Task SetDeviceStateAsync(DeviceState state)
        {
            var lastState = await StateManager.GetStateAsync<DeviceState>("silhouette");

            if (lastState.Version < Int32.MaxValue)
            {
                state.Version = lastState.Version++;
            }
            else
            {
                state.Version = 1;
            }
            state.Timestamp = DateTime.Now;
            state.DeviceID = this.GetActorId().ToString();

            await AddDeviceMessageAsync(state);

            await StateManager.SetStateAsync("silhouette", state);
        }

        async Task AddDeviceMessageAsync(DeviceState state)
        {
            List<DeviceState> messages = new List<DeviceState>();
            Microsoft.ServiceFabric.Data.ConditionalValue<List<DeviceState>> _messages;
            _messages = await StateManager.TryGetStateAsync<List<DeviceState>>("silhouetteMessages");
            if (_messages.HasValue) { messages = _messages.Value; };

            messages.Add(state);
            await StateManager.SetStateAsync<List<DeviceState>>("silhouetteMessages", messages);
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see http://aka.ms/servicefabricactorsstateserialization

            await StateManager.TryAddStateAsync("silhouette", new DeviceState());
        }

    }
}
