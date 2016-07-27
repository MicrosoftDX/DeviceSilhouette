using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using DeviceRichState;
using Newtonsoft.Json.Linq;

using StateProcessorService;
using System.Threading.Tasks;
using Swashbuckle.Swagger.Annotations;
using CommunicationProviderService;
using StateManagementServiceWebAPI.Models;
using System.Web.Http.Results;
using StateManagementServiceWebAPI.Filters;
using StateManagementServiceWebAPI.Helpers;
using StateManagementServiceWebAPI.Models.DeviceMessage;
using System.Linq;

namespace StateManagementServiceWebAPI.Controllers
{
    /// <summary>
    /// Get or manipulate commands for a device
    /// </summary>
    [RoutePrefix("v0.1/devices/{deviceId}/messages")]
    public class DeviceMessageController : ApiController
    {
        private readonly IStateProcessorRemoting _stateProcessor;
        private readonly ICommunicationProviderRemoting _communicationProvider;

        private const int MessageResultPageSize = 10;

        /// <summary>
        /// Lazy DI constructor ;-)
        /// </summary>
        public DeviceMessageController()
            : this(
                  stateProcessor: ServiceProxy.Create<IStateProcessorRemoting>(new Uri("fabric:/StateManagementService/StateProcessorService")),
                  communicationProvider: ServiceProxy.Create<ICommunicationProviderRemoting>(new Uri("fabric:/StateManagementService/CommunicationProviderService"))
                  )
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateProcessor"></param>
        /// <param name="communicationProvider"></param>
        public DeviceMessageController(IStateProcessorRemoting stateProcessor, ICommunicationProviderRemoting communicationProvider)
        {
            _stateProcessor = stateProcessor;
            _communicationProvider = communicationProvider;
        }

        /// <summary>
        /// Get a specific message by device id and version
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [Route("{version:int}")]
        public async Task<IHttpActionResult> GetMessage(string deviceId, int version)
        {
            IHttpActionResult result;
            var message = await _stateProcessor.GetMessageAsync(deviceId, version);
            if (message == null)
            {
                result = this.NotFound(new ErrorModel
                {
                    Code = ErrorCode.InvalidDeviceId,
                    Message = ErrorMessage.InvalidDeviceId(deviceId)
                });
            }
            else
            {
                result = Ok(ToMessageModel(message));
            }

            return result;
        }

        /// <summary>
        /// Get the last state reported by the device
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        [Route("", Name = "GetMessages")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(MessageListModel))]
        [SwaggerResponse(HttpStatusCode.NotFound, Type = typeof(ErrorModel))]
        public async Task<IHttpActionResult> GetMessages(string deviceId, [FromUri]int? continuationToken = null)
        {
            IHttpActionResult result;
            var messages = await _stateProcessor.GetMessagesAsync(deviceId, MessageResultPageSize, continuationToken);
            if (messages == null)
            {
                result = this.NotFound(new ErrorModel
                {
                    Code = ErrorCode.InvalidDeviceId,
                    Message = ErrorMessage.InvalidDeviceId(deviceId)
                });
            }
            else
            {
                var resultModel = new MessageListModel
                {
                    Values = messages.Messages.Select(ToMessageModel),
                    NextLink = Url.Link("GetMessages", new { deviceId, continuationToken = messages.Continuation })
                };
                result = Ok(resultModel);

            }
            return result;
        }

        private static MessageModel ToMessageModel(DeviceMessage message)
        {
            return new MessageModel
            {
                DeviceId = message.DeviceId,
                Version = message.Version,
                TimeStamp = message.Timestamp,
                MessageType = message.MessageType.ToString(),
                MessageSubType = message.MessageSubType.ToString(),
                CorrelationId = message.CorrelationId,
                AppMetadata = message.AppMetadata,
                MessageTtlMs = message.MessageTtlMs,
                Values = message.Values
            };
        }
    }
}
