using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using DeviceRepository.Interfaces;
using DeviceRichState;
using Microsoft.ServiceFabric.Data;

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
        public Task<string> GetDeviceStatus()
        {
            var conStatus = StateManager.TryGetStateAsync<string>("deviceStatus").Result;
            if (conStatus.HasValue)
                return Task.FromResult(conStatus.Value);
            else
                return Task.FromResult(String.Empty);
        }

        public Task SetDeviceStatus(string status)
        {
            StateManager.SetStateAsync<string>("deviceStatus", status);
            return Task.FromResult(true);
        }

        public async Task<DeviceState> GetLastKnownReportedState()
        {
            // search in silhouetteMessages
            var stateMessages = await GetDeviceStateMessagesAsync();
            var orderedMessages = stateMessages.OrderByDescending(m => m.Timestamp).Where(m => m.MessageType ==Types.Report);
            return orderedMessages.First();
        }

        public async Task<DeviceState> GetLastKnownRequestedState()
        {
            // search in silhouetteMessages
            var stateMessages = await GetDeviceStateMessagesAsync();
            var orderedMessages = stateMessages.OrderByDescending(m => m.Timestamp).Where(m => m.MessageType == Types.Request && m.MessageStatus == Status.New);
            return orderedMessages.First();
        }

        public async Task<DeviceState> GetDeviceStateAsync()
        {
           var state = await StateManager.GetStateAsync<DeviceState>("silhouetteMessage");
           return state;
        }

        public async Task<List<DeviceState>> GetDeviceStateMessagesAsync()
        {
            var stateMessages = await StateManager.GetStateAsync<List<DeviceState>>("silhouetteMessages");
            return stateMessages ;
        }

        public async Task<DeviceState> SetDeviceStateAsync(DeviceState state)
        {
            // check if this state is for this actor : DeviceID == ActorId
            if (state.DeviceId == this.GetActorId().ToString())
            {
                var lastState = await StateManager.TryGetStateAsync<DeviceState>("silhouetteMessage");

                if (lastState.HasValue)
                    state.Version = (lastState.Value.Version < Int32.MaxValue) ? (lastState.Value.Version + 1) : 1;

                await AddDeviceMessageAsync(state);

                await StateManager.SetStateAsync("silhouetteMessage", state);
                return state;
            }
            else
            {
                ActorEventSource.Current.ActorMessage(this, "State invalid, device is {0} silhouette is {1}.",state.DeviceId,this.GetActorId().ToString());
                return null;
            }

        }

        async Task AddDeviceMessageAsync(DeviceState state)
        {
            ConditionalValue<List<DeviceState>> messagesInState = await StateManager.TryGetStateAsync<List<DeviceState>>("silhouetteMessages");
            List<DeviceState> messages = messagesInState.HasValue ? messagesInState.Value : new List<DeviceState>();

            messages.Add(state);
            await StateManager.SetStateAsync("silhouetteMessages", messages);
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

            //await StateManager.TryAddStateAsync("silhouetteMessage", new DeviceState());
        }

    }
}
