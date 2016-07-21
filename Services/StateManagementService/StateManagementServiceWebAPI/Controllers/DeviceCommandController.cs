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

namespace StateManagementServiceWebAPI.Controllers
{
    /// <summary>
    /// Get or manipulate commands for a device
    /// </summary>
    [RoutePrefix("v0.1/devices/{deviceId}/commands")]
    public class DeviceCommandController : ApiController
    {
        private IStateProcessorRemoting StateProcessorClient = ServiceProxy.Create<IStateProcessorRemoting>(new Uri("fabric:/StateManagementService/StateProcessorService"));
        private ICommunicationProviderRemoting CommunicationProviderServiceClient = ServiceProxy.Create<ICommunicationProviderRemoting>(new Uri("fabric:/StateManagementService/CommunicationProviderService"));


        // POST devices/{DeviceId} /commands/deepget
        // Used to invoke Get current state from the device
        // The current state updates the Silhouette
        // It has no return value

        /// <summary>
        /// Trigger a DeepGet.
        /// NOTE: this endpoint is temporary and will likely change!
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="timeToLiveMilliSec"></param>
        /// <returns></returns>
        [Route("deepget")] // TODO - this is a temporary endpoint - it feels as though it should be just another command
        [HttpPost]
        public async Task InvokeDeepRead([FromUri]string deviceId, [FromUri] double timeToLiveMilliSec)
        {
            await CommunicationProviderServiceClient.InvokeDeepReadStateAsync(deviceId, timeToLiveMilliSec);
        }

        /*
         PUT devices/{DeviceId}
         To call using Swagger UI: http://localhost:9013/swagger/ui/index
         Or to call from fiddler:
         Method: PUT
         Host: http://localhost:9013/devices/{DeviceId}
         Headers:
         User-Agent: Fiddler
         Host: localhost:9013
         Content-type: application/json
         body:
          {
              "appMetadata": {"origin" : "sensor"},
              "values": {"Xaxis" : 0, "Yaxis" : 0, "Zaxis" : 0},
              "timeToLiveMilliSec": 5000
           }
        */
        /// <summary>
        /// Add a new command
        /// NB Currently this only supports creating a state request command
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="requestedState"></param>
        /// <returns></returns>
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DeviceStateRequestModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(ErrorModel))]
        [HandleInvalidModel]
        public async Task<IHttpActionResult> Post(
            [FromUri]string deviceId,
            [FromBody]DeviceStateRequestModel requestedState)
        {

            // TODO: add error handling. return HttpResponseException if StateValue is null (not well formated JSON)
            try
            {
                var deviceState = await StateProcessorClient.SetStateValueAsync(
                    deviceId,
                    requestedState.AppMetadata.ToString(),
                    requestedState.Values.ToString(),
                    requestedState.TimeToLiveMilliSec);

                return Ok(new DeviceStateModel(deviceState));
            }
            catch (Exception e) // TODO - filter the exceptions that we catch, add logging, ...
            {
                throw;
            }

        }
    }
}
