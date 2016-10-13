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
using StateManagementServiceWebAPI.Helpers;

namespace StateManagementServiceWebAPI.Controllers
{
    /// <summary>
    /// Get state information for a device
    /// </summary>
    [RoutePrefix("v0.1/devices/{deviceId}/state")]
    public class DeviceStateController : ApiController
    {
        private readonly IStateProcessorRemoting _stateProcessor;

        /// <summary>
        /// Lazy DI constructor ;-)
        /// </summary>
        public DeviceStateController()
            : this(
                  stateProcessor: ServiceProxy.Create<IStateProcessorRemoting>(new Uri("fabric:/StateManagementService/StateProcessorService"))
                  )
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateProcessor"></param>
        public DeviceStateController(IStateProcessorRemoting stateProcessor)
        {
            _stateProcessor = stateProcessor;
        }

        /// <summary>
        /// Get the last state reported by the device
        /// </summary>
        /// <param name="deviceId">The id of the device</param>
        /// <returns></returns>
        [Route("latest-reported")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DeviceStateModel))]
        [SwaggerResponse(HttpStatusCode.NotFound, Type = typeof(ErrorModel))]
        public async Task<IHttpActionResult> GetLastReportedState([FromUri]string deviceId)
        {
            IHttpActionResult result;
            var state = await _stateProcessor.GetLastReportedStateAsync(deviceId);
            if (state == null)
            {
                result = Ok();
            }
            else
            {
                result = Ok(new DeviceStateModel(state));
            }

            return result;
        }

        /// <summary>
        /// Get the last state reported by the device or requested by the client (but not negatively acknowledged, expired, ...)
        /// </summary>
        /// <param name="deviceId">The id of the device</param>
        /// <returns></returns>
        [Route("latest-requested")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DeviceStateModel))]
        [SwaggerResponse(HttpStatusCode.NotFound, Type = typeof(ErrorModel))]
        public async Task<IHttpActionResult> GetLastRequestedState([FromUri]string deviceId)
        {
            IHttpActionResult result;
            var state = await _stateProcessor.GetLastRequestedStateAsync(deviceId);
            if (state == null)
            {
                result = Ok();
            }
            else
            {
                result = Ok(new DeviceStateModel(state));
            }
            return result;
        }
    }
}

