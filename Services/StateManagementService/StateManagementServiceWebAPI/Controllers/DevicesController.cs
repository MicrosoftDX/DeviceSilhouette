using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using DeviceStateNamespace;
using Newtonsoft.Json.Linq;

using StateProcessorService;
using System.Threading.Tasks;
using Swashbuckle.Swagger.Annotations;
using CommunicationProviderService;

namespace StateManagementServiceWebAPI.Controllers
{
    [RoutePrefix("devices")]
    public class DevicesController : ApiController
    {        
        private IStateProcessorRemoting StateProcessorClient = ServiceProxy.Create<IStateProcessorRemoting>(new Uri("fabric:/StateManagementService/StateProcessorService"));
        private ICommunicationProviderRemoting CommunicationProviderServiceClient = ServiceProxy.Create<ICommunicationProviderRemoting>(new Uri("fabric:/StateManagementService/CommunicationProviderService"));

        [Route("{deviceId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(DeviceState))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get([FromUri]string deviceId)
        {
            var deviceState = await StateProcessorClient.GetStateAsync(deviceId);

            // When no state the DeviceRepository returns an instance with default values
            // use the DeviceID to test if we have an actual result as that should always be set
            if (deviceState.DeviceID == null)
            {
                return NotFound();
            }
            return Ok(deviceState);
        }

        [Route("{deviceId}")]      
        public async Task DeepGet([FromUri]string deviceId, [FromUri] double timeToLiveMilliSec)
        {            
            await CommunicationProviderServiceClient.DeepGetStateAsync(deviceId, timeToLiveMilliSec);          
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
        // Body:
        // {
        //"Xaxis" : "1" ,
        //"Yaxis" : "2" ,
        //"Zaxis" : "3"
        // }
        [Route("{deviceId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(DeviceState))]
        public async Task<DeviceState> Put([FromUri]string deviceId, [FromUri]double timeToLiveMilliSec, [FromBody]JToken stateValue)
        {
            // TODO: add error handling. return HttpResponseException if StateValue is null (not well formated JSON)
            try
            {                
                var deviceState = await StateProcessorClient.SetStateValueAsync(deviceId, stateValue.ToString(Newtonsoft.Json.Formatting.None), timeToLiveMilliSec);
                return deviceState;
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }


    }
}
