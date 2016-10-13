// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
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
using StateManagementServiceWebAPI.Helpers;
using StateManagementServiceWebAPI.Models.DeviceMessage;
using System.Linq;
using CommonUtils;

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

        private const int CommandResultPageSize = 10;

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
        /// 
        /// </summary>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="commandId">The id of the command to retrieve (this is the correlation id for the command messages)</param>
        /// <returns></returns>
        [Route("{commandId}", Name="GetCommand")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CommandModel))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get([FromUri] string deviceId,
            string commandId)
        {
            var messages = await _stateProcessor.GetMessagesWithCorrelationIdAsync(deviceId, commandId);
            if (messages == null || messages.Length == 0)
            {
                return this.NotFound(new ErrorModel
                {
                    Code = ErrorCode.EntityNotFound,
                    Message = ErrorMessage.EntityNotFound_DeviceCommand(deviceId, commandId)
                });
            }
            return Ok(new CommandModel(messages));
        }

        /// <summary>
        /// Get messages reported by the device
        /// </summary>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        [Route("", Name = "GetCommands")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ListModel<CommandModel>))]
        public async Task<IHttpActionResult> GetCommands(string deviceId, [FromUri]string continuationToken = null)
        {
            IHttpActionResult result;
            var commandMessages = await _stateProcessor.GetCommandMessagessAsync(deviceId, CommandResultPageSize, continuationToken);
            var resultModel = new ListModel<CommandModel>
            {
                Values = commandMessages?.Messages?.Select(m=> new CommandModel(m)) ?? Enumerable.Empty<CommandModel>(),
                NextLink = continuationToken == null ? null : Url.Link("getCommands", new { deviceId, continuationToken = commandMessages.Continuation })
            };
            result = Ok(resultModel);
            return result;
        }



        /// <summary>
        /// Add a new command
        /// </summary>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="command"></param>
        /// <returns></returns>
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CommandModel))] 
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ErrorModel))]
        [HandleInvalidModel]
        public async Task<IHttpActionResult> Post(
            [FromUri]string deviceId,
            [FromBody] Models.CreateCommandRequestModel command) 
        {
            var deviceMessage = DeviceMessage.CreateCommandRequest(
                                                    deviceId, 
                                                    metadata: command.AppMetadata?.ToString(), 
                                                    values: command.Values?.ToString(),
                                                    messageSubType: command.Subtype.Value,
                                                    messageTtlMs: command.TimeToLiveMilliSec);

            deviceMessage = await _communicationProvider.SendCloudToDeviceMessageAsync(deviceMessage);

            return Created(
                Url.Link("GetCommand", new { commandId = deviceMessage.CorrelationId }),
                new CommandModel(new[] { deviceMessage })); 
        }
    }
}

