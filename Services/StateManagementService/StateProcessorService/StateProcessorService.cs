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
using CommunicationProviders.IoTHub;
using System.Web.Script.Serialization;

namespace StateProcessorService
{

    public interface IStateProcessorRemoting : IService
    {
 
        Task<DeviceState> GetStateAsync(string DeviceId);
        Task<DeviceState> SetStateValueAsync(string DeviceId, string StateValue);
        Task<DeviceState> SetStateObjectAsync(string DeviceId, string DevicState);
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

            // init communicaition provider
            CommProviderIoTHub hub = new CommProviderIoTHub();

            long iterations = 0;
          
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();                                                              
                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", ++iterations);

                // get message from comm provider                
                await prcoessIoTHubMessages(hub);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private async Task prcoessIoTHubMessages(CommProviderIoTHub hub)
        {
            string message = hub.ReceiveDeviceToCloudAsync().Result;
            if (!String.IsNullOrEmpty(message))
            {
                JObject StateMessageJSON = JObject.Parse(message);
                JsonState jsonState = (JsonState)StateMessageJSON.ToObject(typeof(JsonState));
                
                switch (jsonState.Status)
                {
                    case "Reported":
                        // TODO - add assert if device id exist. Create if not?
                        await internalSetStateObject(jsonState.DeviceID, jsonState);
                        break;
                    case "Get":
                        DeviceState deviceState = await GetStateAsync(jsonState.DeviceID);
                        var json = new JavaScriptSerializer().Serialize(deviceState);
                        await hub.SendCloudToDeviceAsync(json, deviceState.DeviceID);
                        break;
                }
            }        
        }
       

        public async Task<DeviceState> GetStateAsync(string DeviceId)
        {
            //TODO: error handling
            //TODO: Check if ActorId(DeviceId) exist - if not through exception and dont create it
            ActorId actorId = new ActorId(DeviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoriUri);
            var newState = await silhouette.GetDeviceStateAsync();
            return newState;
        }


        // For now it just create an actor in the repository with the DeviceID
        // TODO: Implement get the state from the device itself
        // StateValue example: {"Xaxis":"0","Yaxis":"0","Zaxis":"0"}
        public async Task<DeviceState> SetStateValueAsync(string DeviceId, string StateValue)
        { 
            //TODO: error handling
            ActorId actorId = new ActorId(DeviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoriUri);
            DeviceState deviceState = new DeviceState(actorId.GetStringId(), StateValue);
            //deviceState.Timestamp = DateTime.Now.Millisecond;
            deviceState.Timestamp = DateTime.UtcNow;
            deviceState.Version = 0;
            deviceState.Status = "Registered";

            await silhouette.SetDeviceStateAsync(deviceState);
            var newState = await silhouette.GetDeviceStateAsync();
            return newState;
        }

        // StateMessage example: {"DeviceID":"silhouette1","Timestamp":1464524365618,"Status":"Reported","State":{"Xaxis":"0","Yaxis":"0","Zaxis":"0"}} 
        public async Task<DeviceState> SetStateObjectAsync(string DeviceId, string DeviceState)
        {
            JObject StateMessageJSON = JObject.Parse(DeviceState);
            JsonState jsonState = (JsonState)StateMessageJSON.ToObject(typeof(JsonState));

            return await internalSetStateObject(DeviceId, jsonState);            
        }

        private async Task<DeviceState> internalSetStateObject(string DeviceId, JsonState jsonState)
        {
            //TODO: error handling
            ActorId actorId = new ActorId(DeviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoriUri);

            DeviceState deviceState = new DeviceState(DeviceId, jsonState.State.ToString());
            deviceState.Status = jsonState.Status;
            deviceState.Timestamp = jsonState.Timestamp;

            await silhouette.SetDeviceStateAsync(deviceState);
            var newState = await silhouette.GetDeviceStateAsync();
            return newState;
        }

        private class JsonState
        {
            public string DeviceID { get; set; }
            public DateTime Timestamp { get; set; }
            public int Version { get; set; }
            public string Status { get; set; }
            public Object State { get; set; }
        }
    }
}
