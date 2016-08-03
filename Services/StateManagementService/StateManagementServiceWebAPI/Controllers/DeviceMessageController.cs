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
using CommonUtils;

namespace StateManagementServiceWebAPI.Controllers
{
    /// <summary>
    /// Get or manipulate commands for a device
    /// </summary>
    [RoutePrefix("v0.1/devices/{deviceId}/messages")]
    public class DeviceMessageController : ApiController
    {
        private readonly IStateProcessorRemoting _stateProcessor;

        private const int MessageResultPageSize = 10;

        /// <summary>
        /// Lazy DI constructor ;-)
        /// </summary>
        public DeviceMessageController()
            : this(
                  stateProcessor: ServiceProxy.Create<IStateProcessorRemoting>(new Uri("fabric:/StateManagementService/StateProcessorService"))
                  )
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateProcessor"></param>
        public DeviceMessageController(IStateProcessorRemoting stateProcessor)
        {
            _stateProcessor = stateProcessor;
        }

        /// <summary>
        /// Get a specific message by device id and version
        /// </summary>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="version">The version (message identifier) for the message to return</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(MessageModel))]
        [SwaggerResponse(HttpStatusCode.NotFound, Type = typeof(ErrorModel))]
        [Route("{version:int}")]
        public async Task<IHttpActionResult> GetMessage(string deviceId, int version)
        {
            IHttpActionResult result;
            var message = await _stateProcessor.GetMessageAsync(deviceId, version);
            if (message == null)
            {
                result = this.NotFound(new ErrorModel
                {
                    Code = ErrorCode.EntityNotFound,
                    Message = ErrorMessage.EntityNotFound_DeviceMessage(deviceId, version)
                });
            }
            else
            {
                result = Ok(ToMessageModel(message));
            }

            return result;
        }

        /// <summary>
        /// Get messages reported by the device
        /// </summary>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        [Route("", Name = "GetMessages")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ListModel<MessageModel>))]
        public async Task<IHttpActionResult> GetMessages(string deviceId, [FromUri]int? continuationToken = null)
        {
            IHttpActionResult result;
            var messages = await _stateProcessor.GetMessagesAsync(deviceId, MessageResultPageSize, continuationToken);
            var resultModel = new ListModel<MessageModel>
            {
                Values = messages?.Messages?.Select(ToMessageModel) ?? Enumerable.Empty<MessageModel>(),
                NextLink = continuationToken == null ? null : Url.Link("GetMessages", new { deviceId, continuationToken = messages.Continuation })
            };
            result = Ok(resultModel);
            return result;
        }

        private static MessageModel ToMessageModel(DeviceMessage message)
        {
            return new MessageModel
            {
                DeviceId = message.DeviceId,
                Version = message.Version,
                TimeStamp = message.Timestamp,
                Type = message.MessageType.ToString(),
                Subtype = message.MessageSubType.ToString(),
                CorrelationId = message.CorrelationId,
                AppMetadata = string.IsNullOrEmpty(message.AppMetadata) ? null : JToken.Parse(message.AppMetadata),
                MessageTtlMs = message.MessageTtlMs,
                Values = string.IsNullOrEmpty(message.Values) ? null : JToken.Parse(message.Values)
            };
        }
    }
}
