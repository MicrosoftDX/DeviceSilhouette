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
    [RoutePrefix("v0.1/devices/{deviceId}")]
    public class DevicesController : ApiController
    {
        private IStateProcessorRemoting StateProcessorClient = ServiceProxy.Create<IStateProcessorRemoting>(new Uri("fabric:/StateManagementService/StateProcessorService"));
        private ICommunicationProviderRemoting CommunicationProviderServiceClient = ServiceProxy.Create<ICommunicationProviderRemoting>(new Uri("fabric:/StateManagementService/CommunicationProviderService"));

        [Route("states/latest-reported")]
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
                result = NotFound(new ErrorModel
                {
                    Code = ErrorCode.InvalidDeviceId,
                    Message = ErrorMessage.InvalidDeviceId(deviceId)
                });
            }
            return result;
        }

        [Route("state/latest-requested")]
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
                result = NotFound(new ErrorModel
                {
                    Code = ErrorCode.InvalidDeviceId,
                    Message = ErrorMessage.InvalidDeviceId(deviceId)
                });
            }
            return result;
        }

        // POST devices/{DeviceId} 
        // Used to invoke Get current state from the device
        // The current state updates the Silhouette
        // It has no return value
        [Route("")]
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
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(DeviceStateRequestModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type =typeof(ErrorModel))]
        [HandleInvalidModel]
        public async Task<IHttpActionResult> Put(
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

        // TODO - move to helper class?
        public IHttpActionResult NotFound<T>(T content)
        {
            return new NegotiatedContentResult<T>(HttpStatusCode.NotFound, content, this);
        }
    }
}
