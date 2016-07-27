using StateManagementServiceWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace StateManagementServiceWebAPI.Helpers
{
    /// <summary>
    /// Catch-all handler as a last resort
    /// </summary>
    public class UnhandledErrorModelExceptionHandler : ExceptionHandler
    {
        /// <summary>
        /// convert the exception into a result object
        /// </summary>
        /// <param name="context"></param>
        public override void Handle(ExceptionHandlerContext context)
        {
            string message;
            if (context.Request.RequestUri.IsLoopback)
            {
                message = context.Exception.ToString();
            }
            else
            {
                message = context.Exception.Message;
            }
            context.Result = new NegotiatedContentResult<UnhandledErrorModel>(
                    HttpStatusCode.InternalServerError,
                    new UnhandledErrorModel
                    {
                        Code = ErrorCode.UnhandledError,
                        Message = ErrorMessage.UnhandledError(),
                        InnerError = new InnerErrorModel
                        {
                            Message = message
                        }
                    },
                    (ApiController)context.ExceptionContext.ControllerContext.Controller);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return true;
        }
    }
}
