// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using StateManagementServiceWebAPI.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace StateManagementServiceWebAPI.Filters
{
    /// <summary>
    /// If model state is invalid, automatically return an ErrorModel
    /// </summary>
    public class HandleInvalidModelAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                var errorModel = new InvalidRequestErrorModel
                {
                    Code = ErrorCode.InvalidRequest,
                    Message = ErrorMessage.InvalidRequest(),
                    ValidationMessages = actionContext.ModelState
                                                .Select(kvp => new ValidationMessage
                                                {
                                                    PropertyName = StripInitialPrefix(kvp.Key),
                                                    Messages = kvp.Value
                                                                .Errors
                                                                .Select(e => e.ErrorMessage)
                                                                .ToList()
                                                })
                                                .ToList()
                };
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    errorModel);
            }
            base.OnActionExecuting(actionContext);
        }

        /// <summary>
        /// The key in the ModelState includes the variable name. E.g. "requestedState.Value" rather than "value"
        /// This method strips it out so that the message makes more sense to the API consumer
        /// This is a bit of a hack, but works for now
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string StripInitialPrefix(string name)
        {
            int index = name.IndexOf('.');
            name = char.ToLower(name[index + 1]) + name.Substring(index + 2);
            return name;
        }
    }
}

