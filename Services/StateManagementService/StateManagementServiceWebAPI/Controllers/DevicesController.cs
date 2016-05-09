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
        public string Get([FromUri]string DeviceId)
        {
        
            return  DeviceId;        

        }

       //  GET devices
        public string Get()
        {
            return "DeviceId";
        }
    }
}
