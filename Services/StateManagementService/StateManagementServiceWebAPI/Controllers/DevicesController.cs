using System;
using System.Collections.Generic;
using System.Web.Http;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using DeviceStateNamespace;
using Newtonsoft.Json.Linq;

using StateProcessorService;

namespace StateManagementServiceWebAPI.Controllers
{
    public class DevicesController : ApiController
    {

        private IStateProcessorRemoting StateProcessorClient = ServiceProxy.Create<IStateProcessorRemoting>(new Uri("fabric:/StateManagementService/StateProcessorService"));



        // POST devices/{DeviceId} 
        public void Post([FromUri]string DeviceId, [FromBody]JToken value)
        {
        }

        // PUT devices/{DeviceId} 
        // http://localhost:9013/devices/{DeviceId}
        //User-Agent: Fiddler
        //Host: localhost:9013
        //Content-type: application/json
        public DeviceState Put([FromUri]string DeviceId, [FromBody]JToken value)
        {
            var myTask = StateProcessorClient.CreateState(DeviceId);
            DeviceState deviceState = myTask.Result;
            return deviceState;
        }

        // DELETE devices/{DeviceId}  
        public void Delete([FromUri]string DeviceId)
        {
        }


        // GET devices/{DeviceId}
        public DeviceState Get([FromUri]string DeviceId)
        {

            // TODO: add error handling. return HttpResponseException is the deviceID does not exist

            var myTask = StateProcessorClient.GetState(DeviceId);
            DeviceState deviceState = myTask.Result;                             
            return deviceState;      

        }



    }
}
