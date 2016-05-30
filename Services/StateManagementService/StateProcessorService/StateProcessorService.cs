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
using CommunicationProviders;

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
        private static Uri RepositoriUri = new Uri("fabric:/StateManagementService/DeviceRepositoryActorService");

        ICommunicationProvider commProvider;

        public StateProcessorService(StatelessServiceContext context)
            : base(context)
        {
            // init communicaition provider for Azure IoTHub
            commProvider = new CommProviderIoTHub();
        }

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

                // get message from comm provider                
                await prcoessCommunicationProviderMessages();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }        
        
        // process messages from communication provider - D2C endpoint 
        private async Task prcoessCommunicationProviderMessages()
        {
            string message = commProvider.ReceiveDeviceToCloudAsync().Result;
            if (!String.IsNullOrEmpty(message))
            {
                JObject StateMessageJSON = JObject.Parse(message);
                JsonState jsonState = (JsonState)StateMessageJSON.ToObject(typeof(JsonState));
                
                switch (jsonState.Status)
                {
                    case "Reported": // device reporting a state update
                        // TODO - add assert if device id exist. Create if not?
                        await internalUpdateDeviceState(jsonState.DeviceID, jsonState);
                        break;
                    case "Get": // device requesting last stored state
                        DeviceState deviceState = await GetStateAsync(jsonState.DeviceID);
                        var json = new JavaScriptSerializer().Serialize(deviceState);
                        await commProvider.SendCloudToDeviceAsync(jsonState.State.ToString(), deviceState.DeviceID);
                        break;
                }
            }        
        }

        // This API is used by the REST call
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
        // This API is used by the REST call
        // StateValue example: {"Xaxis":"0","Yaxis":"0","Zaxis":"0"}
        public async Task<DeviceState> SetStateValueAsync(string DeviceId, string StateValue)
        {
            //TODO: error handling - assert device id is not found
            ActorId actorId = new ActorId(DeviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoriUri);
            DeviceState deviceState = new DeviceState(actorId.GetStringId(), StateValue);
            deviceState.Timestamp = DateTime.UtcNow;
            deviceState.Status = "Requested";

            // update device with the new state (C2D endpoint)
            string json = new JavaScriptSerializer().Serialize(deviceState);
            await commProvider.SendCloudToDeviceAsync(json, DeviceId);
            // update device repository
            return await updateRepository(silhouette, deviceState);
        }

        private static async Task<DeviceState> updateRepository(IDeviceRepositoryActor silhouette, DeviceState deviceState)
        {
            await silhouette.SetDeviceStateAsync(deviceState);
            var newState = await silhouette.GetDeviceStateAsync();
            return newState;
        }

        // StateMessage example: {"DeviceID":"silhouette1","Timestamp":1464524365618,"Status":"Reported","State":{"Xaxis":"0","Yaxis":"0","Zaxis":"0"}}
        // update the device state in the repository. This is called by the Communication provider (the device is reporting a state update)  
        private async Task<DeviceState> internalUpdateDeviceState(string DeviceId, JsonState jsonState)
        {
            //TODO: error handling
            ActorId actorId = new ActorId(DeviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoriUri);

            DeviceState deviceState = new DeviceState(DeviceId, jsonState.State.ToString());
            deviceState.Status = jsonState.Status;
            deviceState.Timestamp = jsonState.Timestamp;

            // update device repository
            return await updateRepository(silhouette, deviceState);
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
