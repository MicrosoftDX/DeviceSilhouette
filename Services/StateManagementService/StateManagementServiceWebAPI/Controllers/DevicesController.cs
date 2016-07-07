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

namespace StateManagementServiceWebAPI.Controllers
{
    [RoutePrefix("devices")]
    public class DevicesController : ApiController
    {
        private IStateProcessorRemoting StateProcessorClient = ServiceProxy.Create<IStateProcessorRemoting>(new Uri("fabric:/StateManagementService/StateProcessorService"));
        private ICommunicationProviderRemoting CommunicationProviderServiceClient = ServiceProxy.Create<ICommunicationProviderRemoting>(new Uri("fabric:/StateManagementService/CommunicationProviderService"));

        [Route("{deviceId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PublicDeviceState))]
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

            return Ok(new PublicDeviceState(deviceState));
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
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(PublicDeviceState))]
        public async Task<PublicDeviceState> Put([FromUri]string deviceId, [FromUri]double timeToLiveMilliSec, [FromBody]JToken state)
        {
            // TODO: add error handling. return HttpResponseException if StateValue is null (not well formated JSON)
            try
            {
                // Split state in metadata and values
                var jState = Newtonsoft.Json.Linq.JObject.Parse(state.ToString(Newtonsoft.Json.Formatting.None));
                JToken jsonOut;
                string appMetadata = "";
                string values = "";

                // Split state in metadata and values
                if (jState.TryGetValue("AppMetadata", out jsonOut))
                    appMetadata = jsonOut.ToString(Newtonsoft.Json.Formatting.None);

                if (jState.TryGetValue("Values", out jsonOut))
                    values = jsonOut.ToString(Newtonsoft.Json.Formatting.None);

                var deviceState = await StateProcessorClient.SetStateValueAsync(deviceId, appMetadata, values, timeToLiveMilliSec);
                
                return (new PublicDeviceState(deviceState));
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }


    }
}
