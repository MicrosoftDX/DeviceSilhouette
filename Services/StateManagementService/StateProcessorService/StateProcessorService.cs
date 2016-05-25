using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using DeviceStateNamespace;
using DeviceRepository.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Newtonsoft.Json.Linq;



namespace StateProcessorService
{

    public interface IStateProcessorRemoting : IService
    {
 
        Task<DeviceState> GetState(string DeviceId);
        Task<DeviceState> CreateState(string DeviceId, string StateValue);
    }

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StateProcessorService : StatelessService, IStateProcessorRemoting
    {
        private static Uri RepositoriUri = new Uri("fabric:/StateManagementService/DeviceRepositoryActorService");

        public StateProcessorService(StatelessServiceContext context)
            : base(context)
        { }

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
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        //public Task<DeviceState> GetState(string DeviceId)
        //{
        //    Latitude state = new Latitude("100", "-100", "50");
        //    JObject jsonState = JObject.FromObject(state);

        //    DeviceState deviceState = new DeviceState(DeviceId, jsonState.ToString());
        //    deviceState.Timestamp = DateTime.Now;
        //    deviceState.Version = "1.0.0";
        //    deviceState.Status = "Reported";
        //    return Task.FromResult(deviceState);
        //}

        public Task<DeviceState> GetState(string DeviceId)
        {
            ActorId actorId = new ActorId(DeviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoriUri);
            var newState = silhouette.GetDeviceStateAsync();
            return newState;
        }


        // For now it just create an actor in the repository with the DeviceID
        // TODO: Implement get the state from the device itself
        public Task<DeviceState> CreateState(string DeviceId, string StateValue)
        { 
            ActorId actorId = new ActorId(DeviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoriUri);
            DeviceState deviceState = new DeviceState(actorId.GetStringId(), StateValue);
            deviceState.Timestamp = DateTime.Now;
            deviceState.Version = 0;
            deviceState.Status = "Registered";

            Task.WaitAll(silhouette.SetDeviceStateAsync(deviceState));
            var newState = silhouette.GetDeviceStateAsync();
            return newState;
        }



    }
}
