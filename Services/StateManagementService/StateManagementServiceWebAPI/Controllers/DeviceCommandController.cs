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
using StateManagementServiceWebAPI.Models.DeviceCommand;

namespace StateManagementServiceWebAPI.Controllers
{
    /// <summary>
    /// Get or manipulate commands for a device
    /// </summary>
    [RoutePrefix("v0.1/devices/{deviceId}/commands")]
    public class DeviceCommandController : ApiController
    {
        private IStateProcessorRemoting _stateProcessor;
        private ICommunicationProviderRemoting _communicationProvider;

        /// <summary>
        /// Lazy DI constructor ;-)
        /// </summary>
        public DeviceCommandController()
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
        public DeviceCommandController(IStateProcessorRemoting stateProcessor, ICommunicationProviderRemoting communicationProvider)
        {
            _stateProcessor = stateProcessor;
            _communicationProvider = communicationProvider;
        }

        /// <summary>
        /// Trigger a DeepGet (i.e. invoke getting the current state from the device
        /// NOTE: this endpoint is temporary and will likely change!
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="timeToLiveMilliSec"></param>
        /// <returns></returns>
        [Route("deepget")]
        [HttpPost]
        public async Task InvokeDeepRead([FromUri]string deviceId, [FromUri] long timeToLiveMilliSec)
        {
            // TODO - this is a temporary endpoint - it feels as though it should be just another command
            // TODO - this should return the Accepted Response

            var deviceMessage = new DeviceMessage(deviceId, null, null, MessageType.CommandRequest, MessageSubType.ReportState, timeToLiveMilliSec);

            await _communicationProvider.SendCloudToDeviceMessageAsync(deviceMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="commandId">The id of the command to retrieve (this is the correlation id for the command messages)</param>
        /// <returns></returns>
        [Route("{commandId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CommandModel))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get([FromUri] string deviceId,
            string commandId)
        {
            var messages = await _stateProcessor.GetMessagesByCorrelationIdAsync(deviceId, commandId);
            if (messages == null || messages.Length == 0)
            {
                return NotFound();
            }
            return Ok(new CommandModel(messages));
        }


        /// <summary>
        /// Add a new command
        /// NB Currently this only supports creating a state request command
        /// </summary>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="requestedState"></param>
        /// <returns></returns>
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DeviceStateRequestModel))] // TODO - should be a CommandResponse model
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ErrorModel))]
        [HandleInvalidModel]
        public async Task<IHttpActionResult> Post(
            [FromUri]string deviceId,
            [FromBody]DeviceStateRequestModel requestedState)
        {
            // TODO - this should return the Accepted Response

            // TODO: add error handling. return HttpResponseException if StateValue is null (not well formated JSON)
            var deviceMessage = await _stateProcessor.SetStateValueAsync(
                deviceId,
                requestedState.AppMetadata.ToString(),
                requestedState.Values.ToString(),
                requestedState.TimeToLiveMilliSec);

            return Created(
                "",
                new DeviceStateModel(deviceMessage)); // TODO - should be a CommandResponse model
        }
    }
}
