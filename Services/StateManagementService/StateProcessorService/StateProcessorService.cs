using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using DeviceRichState;
using DeviceRepository.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using CommunicationProviderService;

namespace StateProcessorService
{

    public interface IStateProcessorRemoting : IService
    {

        Task<DeviceMessage> GetLastRequestedStateAsync(string deviceId);
        Task<DeviceMessage> GetLastReportedStateAsync(string deviceId);
        Task<DeviceMessage> SetStateValueAsync(string deviceId, string metadata, string values, long timeToLiveMs);
        /// <summary>
        /// Get specific message by device id and message version
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<DeviceMessage> GetMessageAsync(string deviceId, int version);
        /// <summary>
        /// Get Paged messages
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="pageSize"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        Task<MessageList> GetMessagesAsync(string deviceId, int pageSize, int? continuation);
        /// <summary>
        /// Get the messages with the specified correlationId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        Task<DeviceMessage[]> GetMessagesByCorrelationIdAsync(string deviceId, string correlationId);
    }

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StateProcessorService : StatelessService, IStateProcessorRemoting
    {
        private static Uri RepositoryUri = new Uri("fabric:/StateManagementService/DeviceRepositoryActorService");

        // proxy to communication provider service - to handle C2D endpoint request
        private ICommunicationProviderRemoting CommunicationProviderServiceClient = ServiceProxy.Create<ICommunicationProviderRemoting>(new Uri("fabric:/StateManagementService/CommunicationProviderService"));

        public StateProcessorService(StatelessServiceContext context)
            : base(context)
        {}

        /// <summary>
        /// Override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context =>
            this.CreateServiceRemotingListener(context)) };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", ++iterations);               

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }                

        public async Task<DeviceMessage> GetLastRequestedStateAsync(string deviceId)
        {
            //TODO: error handling
            //TODO: Check if ActorId(DeviceId) exist - if not through exception and dont create it
            IDeviceRepositoryActor silhouette = GetDeviceActor(deviceId);
            var newState = await silhouette.GetLastKnownRequestedStateAsync();
            return newState;
        }

        public async Task<DeviceMessage> GetLastReportedStateAsync(string deviceId)
        {
            //TODO: error handling
            //TODO: Check if ActorId(DeviceId) exist - if not through exception and dont create it
            IDeviceRepositoryActor silhouette = GetDeviceActor(deviceId);
            var newState = await silhouette.GetLastKnownReportedStateAsync();
            return newState;
        }

        public async Task<DeviceMessage> GetMessageAsync(string deviceId, int version)
        {
            IDeviceRepositoryActor silhouette = GetDeviceActor(deviceId);
            return await silhouette.GetMessageByVersionAsync(version);
        }

        public async Task<MessageList> GetMessagesAsync(string deviceId, int pageSize, int? continuation)
        {
            //TODO: error handling
            //TODO: Check if ActorId(DeviceId) exist - if not through exception and dont create it
            IDeviceRepositoryActor silhouette = GetDeviceActor(deviceId);
            return await silhouette.GetMessagesAsync(pageSize, continuation);
        }

        public async Task<DeviceMessage[]> GetMessagesByCorrelationIdAsync(string deviceId, string correlationId)
        {
            IDeviceRepositoryActor silhouette = GetDeviceActor(deviceId);
            return await silhouette.GetMessagesByCorrelationIdAsync(correlationId);
        }



        // This API is used by the REST call
        // StateValue example: {"Xaxis":"0","Yaxis":"0","Zaxis":"0"}
        public async Task<DeviceMessage> SetStateValueAsync(string deviceId, string metadata, string values, long timeToLive)
        {
            //TODO: error handling - assert device id is not found
            IDeviceRepositoryActor silhouette = GetDeviceActor(deviceId);
            var deviceMessage = DeviceMessage.CreateCommandRequest(deviceId, metadata, values, CommandRequestMessageSubType.SetState, timeToLive);
            
            // update device with the new state (C2D endpoint)           
            return await CommunicationProviderServiceClient.SendCloudToDeviceMessageAsync(deviceMessage);
        }

        private static IDeviceRepositoryActor GetDeviceActor(string deviceId)
        {
            ActorId actorId = new ActorId(deviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoryUri);
            return silhouette;
        }

    }
}
