using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using CommunicationProviders.IoTHub;
using CommunicationProviders;
using System.Configuration;
using DeviceRichState;
using DeviceRepository.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Newtonsoft.Json;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using CommonUtils;

namespace CommunicationProviderService
{
    public interface ICommunicationProviderRemoting : IService
    {
        /// <summary>
        /// Send a message to the device (and log it)
        /// </summary>
        /// <param name="deviceMessage"></param>
        /// <returns></returns>
        Task<DeviceMessage> SendCloudToDeviceMessageAsync(DeviceMessage deviceMessage);
    }

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CommunicationProviderService : StatelessService, ICommunicationProviderRemoting
    {
        private static Uri RepositoryUri = new Uri("fabric:/StateManagementService/DeviceRepositoryActorService");
        private readonly JsonSerializer _jsonSerializer;
        private readonly IMessageReceiver _messageReceiver;
        private readonly IotHubMessageSender _messageSender;

        public CommunicationProviderService(StatelessServiceContext context,
            string iotHubConnectionString,
            string storageConnectionString)
            : base(context)
        {
            // init communicaition provider for Azure IoTHub
            _messageReceiver = new IoTHubMessageReceiver(
                iotHubConnectionString,
                storageConnectionString,
                ProcessCommunicationProviderMessageAsync
                );

            _messageSender = new IotHubMessageSender(iotHubConnectionString);

            _jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                CheckAdditionalContent = true,
            });
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
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
            await _messageReceiver.RunAsync(cancellationToken);
        }

        // process messages from communication provider - D2C endpoint 
        private async Task ProcessCommunicationProviderMessageAsync(MessageInfo message)
        {
            // TODO: error handling!
            DeviceMessage deviceMessage = ToDeviceMessage(message);
            await StoreMessageInActorAsync(deviceMessage);

            // Any actions to take on the message (should this be handled here?)
            switch (message.MessageType)
            {
                case MessageType.Inquiry: // device requesting last stored state
                    if (message.InquiryMessageSubType() == InquiryMessageSubType.GetState)
                    {
                        await SendLastReportedStateToDeviceAsync(message);
                    }
                    break;
            }
        }


        // send a message to the device using the C2D endpoint. called when a device is requesting the last stored state
        private async Task SendLastReportedStateToDeviceAsync(MessageInfo message)
        {
            // TODO: error handling!

            IDeviceRepositoryActor actor = GetDeviceActor(message.DeviceId);
            DeviceMessage lastReportedStateMessage = await actor.GetLastKnownReportedStateAsync();

            // create a new DeviceState with new correlation id, send to the device and store in the repository   
            var newState = DeviceMessage.CreateCommandRequest(
                    message.DeviceId,
                    null,
                    lastReportedStateMessage?.Values, // send null if no lastReportedStateMessage
                    CommandRequestMessageSubType.LatestState,
                    5000 // TODO - need to have a configurable TTL for InquiryResponse
                );
            await SendCloudToDeviceMessageAsync(newState);
        }


        public async Task<DeviceMessage> SendCloudToDeviceMessageAsync(DeviceMessage deviceMessage)
        {
            deviceMessage = await StoreMessageInActorAsync(deviceMessage);

            // update C2D end point with the request to state update
            await _messageSender.SendCloudToDeviceAsync(deviceMessage);

            return deviceMessage;
        }

        private async Task<DeviceMessage> StoreMessageInActorAsync(DeviceMessage deviceMessage)
        {
            // update the state repository with the new message
            IDeviceRepositoryActor actor = GetDeviceActor(deviceMessage.DeviceId);
            return await actor.StoreDeviceMessageAsync(deviceMessage);
        }
        private static IDeviceRepositoryActor GetDeviceActor(string deviceId)
        {
            ActorId actorId = new ActorId(deviceId);
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, RepositoryUri);
            return silhouette;
        }

        private DeviceMessage ToDeviceMessage(MessageInfo message)
        {
            switch (message.MessageType)
            {
                // Currently only expect Report (State) or Inquiry messages here (D2C context, ACK/NAK handled in feedback service)
                case MessageType.Inquiry:
                    var inquirySubtype = EnumUtils.ConstrainedParse<InquiryMessageSubType>(message.MessageSubType);
                    return DeviceMessage.CreateInquiry(message.DeviceId, message.Body, inquirySubtype, message.CorrelationId, message.EnqueuedTimeUtc);
                case MessageType.Report:
                    var reportSubtype = EnumUtils.ConstrainedParse<ReportMessageSubType>(message.MessageSubType);
                    return DeviceMessage.CreateReport(message.DeviceId, message.Body, reportSubtype, message.CorrelationId, message.EnqueuedTimeUtc);
            }
            throw new Exception($"Unexpected MessageType '{message.MessageType}'");
        }
    }
}
