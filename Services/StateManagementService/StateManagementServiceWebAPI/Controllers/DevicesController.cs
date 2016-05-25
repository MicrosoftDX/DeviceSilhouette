using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http;
using System.Net;
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
        public void Post([FromUri]string DeviceId, [FromBody]JToken StateValue)
        {
            //TODO: implement
        }

        // PUT devices/{DeviceId} 
        // To call from fiddler:
        // Method: PUT
        // Host: http://localhost:9013/devices/{DeviceId}
        // Headers:
        // User-Agent: Fiddler
        // Host: localhost:9013
        // Content-type: application/json
        // Body:
        // {
        //"Xaxis" : "1" ,
        //"Yaxis : "2" ,
        //"Zaxis" : "3"
        // }
        public DeviceState Put([FromUri]string DeviceId, [FromBody]JToken StateValue)
        {

            // TODO: add error handling. return HttpResponseException if the deviceID already exist or StateValue is null (not well formated JSON)

            var myTask = StateProcessorClient.CreateState(DeviceId, StateValue.ToString());
            DeviceState deviceState = myTask.Result;
            return deviceState;
        }

        // DELETE devices/{DeviceId}  
        public void Delete([FromUri]string DeviceId)
        {
            //TODO: implement
        }


        // GET devices/{DeviceId}
        public DeviceState Get([FromUri]string DeviceId)
        {


            var myTask = StateProcessorClient.GetState(DeviceId);
            DeviceState deviceState = myTask.Result;                             
            return deviceState;

            // TODO: add error handling. return HttpResponseException if the deviceID does not exist
            //var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
            //{
            //    Content = new StringContent(string.Format("No DeviceId = {0}", DeviceId)),
            //    ReasonPhrase = "DeviceId Not Found"
            //};
            //throw new HttpResponseException(resp);

        }



    }
}
