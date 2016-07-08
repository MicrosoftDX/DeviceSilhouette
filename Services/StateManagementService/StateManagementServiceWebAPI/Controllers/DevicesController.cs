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

namespace StateManagementServiceWebAPI.Controllers
{
    [RoutePrefix("devices")]
    public class DevicesController : ApiController
    {
        private IStateProcessorRemoting StateProcessorClient = ServiceProxy.Create<IStateProcessorRemoting>(new Uri("fabric:/StateManagementService/StateProcessorService"));
        private ICommunicationProviderRemoting CommunicationProviderServiceClient = ServiceProxy.Create<ICommunicationProviderRemoting>(new Uri("fabric:/StateManagementService/CommunicationProviderService"));

        [Route("{deviceId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DeviceStateModel))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get([FromUri]string deviceId)
        {
            var deviceState = await StateProcessorClient.GetStateAsync(deviceId);

            // When no state the DeviceRepository returns an instance with default values
            // use the DeviceID to test if we have an actual result as that should always be set
            if (deviceState.DeviceId == null)
            {
                return NotFound();
            }

            return Ok(new DeviceStateModel(deviceState));
        }

        // POST devices/{DeviceId} 
        // Used to invoke Get current state from the device
        // The current state updates the Silhouette
        // It has no return value
        [Route("{deviceId}")]
        public async Task InvokeDeepRead([FromUri]string deviceId, [FromUri] double timeToLiveMilliSec)
        {
            await CommunicationProviderServiceClient.InvokeDeepReadStateAsync(deviceId, timeToLiveMilliSec);
        }

        // PUT devices/{DeviceId} 
        // To call using Swagger UI: http://localhost:9013/swagger/ui/index
        // Or to call from fiddler:
        // Method: PUT
        // Host: http://localhost:9013/devices/{DeviceId}
        // Headers:
        // User-Agent: Fiddler
        // Host: localhost:9013
        // Content-type: application/json
        // body:
        //  {               
        //      "AppMetadata": {"origin" : "sensor"},
        //      "Values": {"Xaxis" : 0, "Yaxis" : 0, "Zaxis" : 0}
        //  }
        // 
        [Route("{deviceId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(DeviceStateModel))]
        public async Task<IHttpActionResult> Put(
            [FromUri]string deviceId, 
            [FromUri]double timeToLiveMilliSec, 
            [FromBody]DeviceStateModel requestedState)
        {
            // TODO: add error handling. return HttpResponseException if StateValue is null (not well formated JSON)
            try
            {
                var deviceState = await StateProcessorClient.SetStateValueAsync(
                    deviceId, 
                    requestedState.AppMetadata.ToString(), 
                    requestedState.Values.ToString(), 
                    timeToLiveMilliSec);
                
                return Ok(new DeviceStateModel(deviceState));
            }
            catch (Exception e) // TODO - filter the exceptions that we catch, add logging, ...
            {
                throw e;
            }
            
        }


    }
}
