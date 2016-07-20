﻿using System;
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
    [RoutePrefix("v0.1/devices/{deviceId}/state")]
    public class DeviceStateController : ApiController
    {
        private IStateProcessorRemoting StateProcessorClient = ServiceProxy.Create<IStateProcessorRemoting>(new Uri("fabric:/StateManagementService/StateProcessorService"));
        private ICommunicationProviderRemoting CommunicationProviderServiceClient = ServiceProxy.Create<ICommunicationProviderRemoting>(new Uri("fabric:/StateManagementService/CommunicationProviderService"));

        [Route("latest-reported")]
        [SwaggerResponse(HttpStatusCode.NotFound, Type = typeof(ErrorModel))]
        public async Task<IHttpActionResult> GetLastReportedState([FromUri]string deviceId)
        {
            IHttpActionResult result;
            try
            {
                var state = await StateProcessorClient.GetLastReportedStateAsync(deviceId);
                result = Ok(new DeviceStateModel(state));
            }
            catch (Exception e)
            {
                // TODO: return different response according to exception. For now assuming deviceId not found.
                result = this.NotFound(new ErrorModel
                {
                    Code = ErrorCode.InvalidDeviceId,
                    Message = ErrorMessage.InvalidDeviceId(deviceId)
                });
            }
            return result;
        }

        [Route("latest-requested")]
        [SwaggerResponse(HttpStatusCode.NotFound, Type = typeof(ErrorModel))]
        public async Task<IHttpActionResult> GetLastRequestedState([FromUri]string deviceId)
        {
            IHttpActionResult result;
            try
            {
                var state = await StateProcessorClient.GetLastRequestedStateAsync(deviceId);
                result = Ok(new DeviceStateModel(state));
            }
            catch (Exception e)
            {
                // TODO: return different response according to exception. For now assuming deviceId not found.
                result = this.NotFound(new ErrorModel
                {
                    Code = ErrorCode.InvalidDeviceId,
                    Message = ErrorMessage.InvalidDeviceId(deviceId)
                });
            }
            return result;
        }
    }
}
