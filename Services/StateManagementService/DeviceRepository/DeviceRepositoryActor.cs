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
using CommonUtils;

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
        //private const string StateName = "silhouetteMessages";
        private const string StateName = "silhouetteMessage";
        private readonly IStorageProviderRemoting StorageProviderServiceClient = ServiceProxy.Create<IStorageProviderRemoting>(new Uri("fabric:/StateManagementService/StorageProviderService"));
        private readonly MessagePurger _messagePurger;
        private readonly long _messagesRetentionMilliseconds;
        private readonly int _messagesTimerInterval;
        private readonly int _minMessagesToKeep;

        // timer used for persisting and perging messages
        private IActorTimer _messagesTimer;        

        public DeviceRepositoryActor(long messagesRetentionMilliseconds, int messagesTimerInterval, int minMessagesToKeep)
        {
            _messagesRetentionMilliseconds = messagesRetentionMilliseconds;
            _messagesTimerInterval = messagesTimerInterval;
            _minMessagesToKeep = minMessagesToKeep;
            _messagePurger = new MessagePurger(_messagesRetentionMilliseconds, _minMessagesToKeep);
        }

        /// <summary>
        /// Process Messages by the messages timer - persist and purge messages
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task ProcessMessages(object arg)
        {
           var stateMessages = await StateManager.TryGetStateAsync<List<DeviceMessage>>(StateName);
            if (stateMessages.HasValue)
            {              
                var messages = stateMessages.Value;
                PersistMessages(messages);
                _messagePurger.Purge(messages);
            }
        }

        private void PersistMessages(List<DeviceMessage> messages)
        {
            List<DeviceMessage> toPersist = messages.Where(m => !m.Persisted).ToList<DeviceMessage>();
            StorageProviderServiceClient.StoreStateMessagesAsync(toPersist);
            toPersist.ForEach(m => m.Persisted = true);
        }        

        public async Task<DeviceMessage> GetLastKnownReportedStateAsync()
        {
            DeviceMessage state = null;
            // search in silhouetteMessages
            var messages = await GetDeviceMessagesAsync();
            if (messages != null)
            {
                state = messages.OrderByDescending(m => m.Timestamp)
                                                    .Where(m => m.MessageType == MessageType.Report)
                                                    .FirstOrDefault();
            }
            return state;
        }

        public async Task<DeviceMessage> GetLastKnownRequestedStateAsync()
        {
            DeviceMessage state = null;
            // search in silhouetteMessages
            var messages = await GetDeviceMessagesAsync();
            if (messages != null)
            {
                state = messages.OrderByDescending(m => m.Timestamp)
                                        .Where(m => {
                                            return (m.MessageType == MessageType.CommandRequest
                                                        && m.CommandRequestMessageSubType() == CommandRequestMessageSubType.SetState)
                                                    || (m.MessageType == MessageType.Report && m.ReportMessageSubType() == ReportMessageSubType.State);
                                            })
                                        .FirstOrDefault();
            }
            return state;
        }


        public async Task<List<DeviceMessage>> GetDeviceMessagesAsync()
        {
            var stateMessages = await StateManager.TryGetStateAsync<List<DeviceMessage>>(StateName);
            if (stateMessages.HasValue)
                return stateMessages.Value;
            else
                return null;
        }

        public async Task<DeviceMessage> GetMessageByVersionAsync(int version)
        {
            var messages = await GetDeviceMessagesAsync();
            if (messages == null)
            {
                return null;
            }
            return messages.FirstOrDefault(m => m.Version == version);

        }
        public async Task<MessageList> GetMessagesAsync(int pageSize, int? continuation)
        {
            var messages = await GetDeviceMessagesAsync();
            if (messages == null)
            {
                return null;
            }
            var messagesToReturn = messages
                                    .OrderBy(m => m.Timestamp)
                                    .SkipUntil(m => m.Version == continuation || continuation == null)
                                    .Take(pageSize + 1) // take one extra to get the next continuation token!
                                    .ToList();

            int? newContinuation = null;
            if (messagesToReturn.Count > pageSize)
            {
                newContinuation = messagesToReturn[pageSize].Version; // set next continuation value to be the Version of the next item in the list
                messagesToReturn.RemoveAt(pageSize);
            }
            return new MessageList
            {
                Messages = messagesToReturn,
                Continuation = newContinuation
            };

        }
        public async Task<DeviceMessage[]> GetMessagesWithCorrelationIdAsync(string correlationId)
        {
            var messages = await GetDeviceMessagesAsync();
            if (messages == null)
            {
                return null;
            }
            return  messages
                        .Where(m=>m.CorrelationId == correlationId)
                        .OrderBy(m => m.Timestamp)
                        .ToArray();
        }
        public async Task<CommandList> GetCommandMessagesAsync(int pageSize, string continuationToken)
        {
            var messages = await GetDeviceMessagesAsync();
            if (messages == null)
            {
                return null;
            }
            var responseLookup = new Dictionary<string, DeviceMessage>(); // key = correlationId, value= = command response message
            var messagesToReturn = new List<DeviceMessage[]>(pageSize+1);
            bool reachedContinuationToken = string.IsNullOrEmpty(continuationToken); // if no continuationToken then start taking messages

            // iterate messages in reverse
            // store responses by correlation id as we go
            // skip command requests until we find the continuation token
            // then take (pagesize + 1) requests (with correlated response)
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                var message = messages[i];
                if (message.MessageType == MessageType.CommandRequest)
                {
                    if (reachedContinuationToken || message.CorrelationId == continuationToken)
                    {
                        reachedContinuationToken = true;
                        DeviceMessage responseMessage;
                        responseLookup.TryGetValue(message.CorrelationId, out responseMessage);
                        messagesToReturn.Add(new[] { message, responseMessage });

                        if (messagesToReturn.Count == pageSize+1) // take one extra to get the next continuation token!
                        {
                            break; 
                        }
                    }

                }
                else if (message.MessageType == MessageType.CommandResponse)
                {
                    responseLookup.Add(message.CorrelationId, message);
                }
            }

            string newContinuation = null;
            if (messagesToReturn.Count > pageSize)
            {
                newContinuation = messagesToReturn[pageSize][0].CorrelationId; // set next continuation value to be the Version of the next item in the list
                messagesToReturn.RemoveAt(pageSize);
            }
            return new CommandList
            {
                Messages = messagesToReturn,
                Continuation = newContinuation
            };
        }


        public async Task<DeviceMessage> StoreDeviceMessageAsync(DeviceMessage message)
        {
            // check if this state is for this actor : DeviceID == ActorId
            if (message.DeviceId == this.GetActorId().ToString())
            {
                message.Version = await GetNextMessageVersionAsync();                
                await AddDeviceMessageToMessageListAsync(message);
                return message;
            }
            else
            {
                ActorEventSource.Current.ActorMessage(this, "State invalid, device is {0} silhouette is {1}.", message.DeviceId, this.GetActorId().ToString());
                return null;
            }

        }

        private async Task<int> GetNextMessageVersionAsync()
        {
            var messagesConditional = await StateManager.TryGetStateAsync<List<DeviceMessage>>(StateName);
            int nextMessageVersion;
            if (messagesConditional.HasValue && messagesConditional.Value.Count>0)
            {
                var messages = messagesConditional.Value;
                nextMessageVersion = messages[messages.Count-1].Version + 1;
            }
            else
            {
                nextMessageVersion = 1;
            }
            return nextMessageVersion;
        }        

        private async Task AddDeviceMessageToMessageListAsync(DeviceMessage state)
        {
            var stateMessages = await StateManager.TryGetStateAsync<List<DeviceMessage>>(StateName);
            var messages = stateMessages.HasValue ? stateMessages.Value : new List<DeviceMessage>();
            
            messages.Add(state);
            await StateManager.SetStateAsync(StateName, messages);
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            _messagesTimer = RegisterTimer(ProcessMessages, null, TimeSpan.FromMinutes(_messagesTimerInterval), TimeSpan.FromMinutes(_messagesTimerInterval));
            return Task.FromResult((object)null);
        }

        protected override Task OnDeactivateAsync()
        {
            if (_messagesTimer != null)
            {
                UnregisterTimer(_messagesTimer);
            }
            return Task.FromResult((object)null);
        }

    }
}
