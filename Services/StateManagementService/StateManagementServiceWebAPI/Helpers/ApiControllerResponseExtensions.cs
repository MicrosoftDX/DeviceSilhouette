using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace StateManagementServiceWebAPI.Helpers
{
    public static class ApiControllerResponseExtensions
    {
        public static IHttpActionResult NotFound<T>(this ApiController controller, T content)
        {
            return new NegotiatedContentResult<T>(HttpStatusCode.NotFound, content, controller);
        }
    }
}
