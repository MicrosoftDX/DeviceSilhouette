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
        private double _messagesRetention;

        private IActorTimer _purgeTimer;

        public DeviceRepositoryActor(int maxMessages, double messagesRetention)
        {
            _maxMessages = maxMessages;
            _messagesRetention = messagesRetention;
        }       

        private async Task PurgeStates(object arg)
        {           
            var stateMessages = await StateManager.TryGetStateAsync<List<DeviceState>>(StateName);
            if (stateMessages.HasValue)
            {                                              
                var messages = stateMessages.Value;
                var lastReprted = await GetLastKnownReportedState();
                messages.RemoveAll(item => isPurge(item, lastReprted));
            }            
        }

        private bool isPurge(DeviceState item, DeviceState lastReported)
        {
            // check for messages older than the retention, that were persisted 
            // make sure to keep the last Reported message in any case  
            return !item.Equals(lastReported) && item.Persisted && item.Timestamp.CompareTo(DateTime.Now.ToUniversalTime().AddMilliseconds(-_messagesRetention)) < 0;            
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

                // persist the message and add to actor state (in parallel)
                await Task.WhenAll(
                    PersistMessage(state),
                    AddDeviceMessageAsync(state)
                    );

                await StateManager.SetStateAsync("silhouetteMessage", state);
                return state;
            }
            else
            {
                ActorEventSource.Current.ActorMessage(this, "State invalid, device is {0} silhouette is {1}.", state.DeviceId, this.GetActorId().ToString());
                return null;
            }

        }

        private async Task PersistMessage(DeviceState state)
        {
            if (!state.Persisted)
            {
                await StorageProviderServiceClient.StoreStateMessageAsync(state);
                state.Persisted = true;
            }
        }

        async Task AddDeviceMessageAsync(DeviceState state)
        {
            var stateMessages = await StateManager.TryGetStateAsync<List<DeviceState>>(StateName);
            var messages = stateMessages.HasValue ? stateMessages.Value : new List<DeviceState>();

            messages.Add(state);
            await StateManager.SetStateAsync(StateName, messages);
        }        

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            _purgeTimer = RegisterTimer(PurgeStates, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        protected override async Task OnDeactivateAsync()
        {
            if (_purgeTimer != null)
            {
                UnregisterTimer(_purgeTimer);
            }
        }

    }
}
