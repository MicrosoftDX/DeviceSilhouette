using System;
using System.Collections.Generic;
using System.Fabric;
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
using CommunicationProviders.IoTHub;
using CommunicationProviders;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StateProcessorService
{

    public interface IStateProcessorRemoting : IService
    {

        Task<DeviceState> GetStateAsync(string DeviceId);
        Task DeepGetStateAsync(string DeviceId);
        Task<DeviceState> SetStateValueAsync(string DeviceId, string StateValue);
    }

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StateProcessorService : StatelessService, IStateProcessorRemoting
    {
        private static Uri RepositoryUri = new Uri("fabric:/StateManagementService/DeviceRepositoryActorService");

        ICommunicationProvider _communicationProvider;
        private readonly JsonSerializer _jsonSerializer;

        public StateProcessorService(StatelessServiceContext context)
            : base(context)
        {
            // init communicaition provider for Azure IoTHub
            string iotHubConnectionString = ConfigurationManager.AppSettings["iotHubConnectionString"];
            string storageConnectionString = ConfigurationManager.AppSettings["storageConnectionString"];
            _communicationProvider = new IoTHubCommunicationProvider(iotHubConnectionString, storageConnectionString);

            _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                CheckAdditionalContent = true,
            });
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
                await ProcessCommunicationProviderMessagesAsync();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        // process messages from communication provider - D2C endpoint 
        private async Task ProcessCommunicationProviderMessagesAsync()
        {
            string message = await _communicationProvider.ReceiveDeviceToCloudAsync();
            if (!String.IsNullOrEmpty(message))
            {
                try
                {
                    JsonState jsonState = _jsonSerializer.Deserialize<JsonState>(message);

                    switch (jsonState.Status)
                    {
                    case "Reported": // device reporting a state update
                        // TODO - add assert if device id exist. Create if not?
                        await InternalUpdateDeviceStateAsync(jsonState);
                        break;
                    case "Get": // device requesting last stored state
                        DeviceState deviceState = await GetStateAsync(jsonState.DeviceId);
                        if (! String.IsNullOrEmpty(deviceState.DeviceID)) 
                            await _communicationProvider.SendCloudToDeviceAsync(deviceState.State, "State:Get", deviceState.DeviceID);
                        break;
                }
                }
                catch (Exception e)
                {
                    // TODO: better error handling                    
                }
            }
        }

        // Send a get request directly from the device, not going through the device repository
        // The device with send the state in a sepetate call
        // message example:
        //{
            //"DeviceID" : "Device1",
            //"Timestamp" : "2009-06-15T13:45:30",
            //"Status" : "GetInfo" 
        //}
        public async Task DeepGetStateAsync(string DeviceId)
        {
            JsonState state = new JsonState();
            state.DeviceId = DeviceId;
            state.Timestamp = DateTime.Now;
            state.Status = "GetInfo";
            string message = _jsonSerializer.Serialize(state);            

            await _communicationProvider.SendCloudToDeviceAsync(DeviceId, "State:Get", message);                        
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
            DeviceState deviceState = new DeviceState(deviceId, stateValue)
            {
                Timestamp = DateTime.UtcNow,
                Status = "Requested",
            };

            // update device with the new state (C2D endpoint)
            string json = _jsonSerializer.Serialize(deviceState);
            await _communicationProvider.SendCloudToDeviceAsync(json, "State:Set", deviceId);
            // update device repository
            return await silhouette.SetDeviceStateAsync(deviceState);
        }

        private static async Task<DeviceState> UpdateRepositoryAsync(IDeviceRepositoryActor silhouette, DeviceState deviceState)
        {
            await silhouette.SetDeviceStateAsync(deviceState);
            var newState = await silhouette.GetDeviceStateAsync();
            return newState;
        }

        // StateMessage example: {"DeviceID":"silhouette1","Timestamp":1464524365618,"Status":"Reported","State":{"Xaxis":"0","Yaxis":"0","Zaxis":"0"}}
        // update the device state in the repository. This is called by the Communication provider (the device is reporting a state update)  
        private async Task<DeviceState> InternalUpdateDeviceStateAsync(JsonState jsonState)
        {
            //TODO: error handling
            var deviceId = jsonState.DeviceId;
            IDeviceRepositoryActor silhouette = GetDeviceActor(deviceId);

            DeviceState deviceState = new DeviceState(deviceId, jsonState.State.ToString())
            {
                Status = jsonState.Status,
                Timestamp = jsonState.Timestamp,
            };

            // update device repository
            return await silhouette.SetDeviceStateAsync(deviceState);
        }

        private static IDeviceRepositoryActor GetDeviceActor(string deviceId)
        {
            ActorId actorId = new ActorId(deviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoryUri);
            return silhouette;
        }

        private class JsonState
        {
            public string DeviceId { get; set; }
            public DateTime Timestamp { get; set; }
            public int Version { get; set; }
            public string Status { get; set; }
            public Object State { get; set; }
        }
    }
}
