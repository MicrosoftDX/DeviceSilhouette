// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
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
    /// <summary>
    /// Extensions to ApiControllers for customising the returned Response
    /// </summary>
    public static class ApiControllerResponseExtensions
    {
        /// <summary>
        /// Send a NotFound (404) with body content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static IHttpActionResult NotFound<T>(this ApiController controller, T content)
        {
            return new NegotiatedContentResult<T>(HttpStatusCode.NotFound, content, controller);
        }
    }
}

