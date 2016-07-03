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

        Task<DeviceState> GetStateAsync(string DeviceId);        
        Task<DeviceState> SetStateValueAsync(string DeviceId, string StateValue);
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

        // This API is used by the REST call
        public async Task<DeviceState> GetStateAsync(string deviceId)
        {
            //TODO: error handling
            //TODO: Check if ActorId(DeviceId) exist - if not through exception and dont create it
            IDeviceRepositoryActor silhouette = GetDeviceActor(deviceId);
            var newState = await silhouette.GetDeviceStateAsync();
            return newState;
        }

        // For now it just create an actor in the repository with the DeviceID
        // TODO: Implement get the state from the device itself
        // This API is used by the REST call
        // StateValue example: {"Xaxis":"0","Yaxis":"0","Zaxis":"0"}
        public async Task<DeviceState> SetStateValueAsync(string deviceId, string stateValue)
        {
            //TODO: error handling - assert device id is not found
            IDeviceRepositoryActor silhouette = GetDeviceActor(deviceId);
            DeviceState deviceState = new DeviceState(deviceId, stateValue,Types.Requested)
            {
                //Timestamp = DateTime.UtcNow,
                //Status = "Requested",
            };

            // update device with the new state (C2D endpoint)           
            await CommunicationProviderServiceClient.SendCloudToDeviceAsync(deviceState, "State:Set");
            // update device repository
            return await silhouette.SetDeviceStateAsync(deviceState);
        }

        private static async Task<DeviceState> UpdateRepositoryAsync(IDeviceRepositoryActor silhouette, DeviceState deviceState)
        {
            await silhouette.SetDeviceStateAsync(deviceState);
            var newState = await silhouette.GetDeviceStateAsync();
            return newState;
        }        

        private static IDeviceRepositoryActor GetDeviceActor(string deviceId)
        {
            ActorId actorId = new ActorId(deviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoryUri);
            return silhouette;
        }        
    }
}
