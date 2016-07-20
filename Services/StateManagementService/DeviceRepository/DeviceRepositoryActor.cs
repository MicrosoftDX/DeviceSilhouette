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
using StorageProviderService;
using Microsoft.ServiceFabric.Services.Remoting.Client;

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
        private const string StateName = "silhouetteMessages";
        private IStorageProviderRemoting StorageProviderServiceClient = ServiceProxy.Create<IStorageProviderRemoting>(new Uri("fabric:/StateManagementService/StorageProviderService"));
        private int _maxMessages;
        DateTime _lastPersist = DateTime.Now;

        public DeviceRepositoryActor(int maxMessages)
        {
            _maxMessages = maxMessages;
            // TODO: create a task to check elapsed time and persist all messages older than the time limit
            Task.Run(() => storeMessages());
        }

        private async void storeMessages()
        {
            while (true)
            {
                if (StateManager != null)
                {
                    var stateMessages = StateManager.TryGetStateAsync<List<DeviceState>>(StateName);
                    var messages = stateMessages.Result.HasValue ? stateMessages.Result.Value : new List<DeviceState>();
                    await persistMessages(messages);
                }

                Thread.Sleep(1000);
            }
        }

        public Task<string> GetDeviceStatus()
        {
            var status = StateManager.TryGetStateAsync<string>("deviceStatus").Result;
            if (status.HasValue)
                return Task.FromResult(status.Value);
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
            DeviceState state = null;
            // search in silhouetteMessages
            var stateMessages = await GetDeviceStateMessagesAsync();
            if (stateMessages != null)
            {
                var orderedMessages = stateMessages.OrderByDescending(m => m.Timestamp).Where(m => m.MessageType == MessageType.Reported);
                if (orderedMessages != null)
                    state = orderedMessages.First();
            }

            return state;
        }

        public async Task<DeviceState> GetLastKnownRequestedState()
        {
            DeviceState state = null;
            // search in silhouetteMessages
            var stateMessages = await GetDeviceStateMessagesAsync();
            if (stateMessages != null)
            {
                var orderedMessages = stateMessages.OrderByDescending(m => m.Timestamp).Where(m => m.MessageType == MessageType.Requested && m.MessageStatus == MessageStatus.New);
                if (orderedMessages != null)
                    state = orderedMessages.First();
            }

            return state;
        }

        public async Task<DeviceState> GetDeviceStateAsync()
        {
            var stateMessage = await StateManager.TryGetStateAsync<DeviceState>("silhouetteMessage");

            if (stateMessage.HasValue)
                return stateMessage.Value;
            else
                return null;
        }

        public async Task<List<DeviceState>> GetDeviceStateMessagesAsync()
        {
            var stateMessages = await StateManager.TryGetStateAsync<List<DeviceState>>(StateName);
            if (stateMessages.HasValue)
                return stateMessages.Value;
            else
                return null;
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
                ActorEventSource.Current.ActorMessage(this, "State invalid, device is {0} silhouette is {1}.", state.DeviceId, this.GetActorId().ToString());
                return null;
            }

        }

        async Task AddDeviceMessageAsync(DeviceState state)
        {
            var stateMessages = await StateManager.TryGetStateAsync<List<DeviceState>>(StateName);
            var messages = stateMessages.HasValue ? stateMessages.Value : new List<DeviceState>();

            messages.Add(state);
            await StateManager.SetStateAsync(StateName, messages);
        }

        private async Task persistMessages(List<DeviceState> messages)
        {
            // check if #of messages requires storing in persistance storage
            if (messages != null && messages.Count() >= _maxMessages)
            {
                await StorageProviderServiceClient.StoreStateMessagesAsync(messages);
                // add a message cursor to know what message was last persisted, to avoid perging all the messages
                // TODO - add a smart purging engine
                await StateManager.TryRemoveStateAsync(StateName);                
            }
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
        }

    }
}
