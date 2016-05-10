using System;
using System.Collections.Generic;
using System.Web.Http;

namespace StateManagementServiceWebAPI.Controllers
{
    public class DevicesController : ApiController
    {


        //// POST devices/{DeviceId} 
        //public void Post([FromUri]string DeviceId, [FromBody]string value)
        //{
        //}

        //// PUT devices/{DeviceId} 
        //public void Put([FromUri]string DeviceId, [FromBody]string value)
        //{
        //}

        //// DELETE devices/{DeviceId}  
        //public void Delete([FromUri]string DeviceId)
        //{
        //}


        // GET devices/{DeviceId}
        public DeviceState Get([FromUri]string DeviceId)
        {

            // TODO: add error handling. return HttpResponseException is the deviceID does not exist

            // return a fake response
            Latitude state = new Latitude("100", "-100", "50");
            DeviceState deviceState = new DeviceState(DeviceId, state);
            deviceState.Timestamp = DateTime.Now;
            deviceState.Version = "1.0.0";
            deviceState.Status = "Reported";

            return deviceState;      

        }

    }
}
