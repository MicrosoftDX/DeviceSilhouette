using System.Web.Http;
using Owin;
using Swashbuckle.Application;
using Newtonsoft.Json.Serialization;
using System.Web.Http.ExceptionHandling;
using StateManagementServiceWebAPI.Helpers;

namespace StateManagementServiceWebAPI
{
    /// <summary>
    /// API pipeline configuration
    /// </summary>
    public static class Startup
    {
        /// <summary>
        /// This code configures Web API. The Startup class is specified as a type parameter in the WebApp.Start method.
        /// </summary>
        /// <param name="appBuilder"></param>
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.Services.Replace(typeof(IExceptionHandler), new UnhandledErrorModelExceptionHandler());

            config.MapHttpAttributeRoutes();

            config.EnableSwagger(c =>
            {
                c.IncludeXmlComments(GetXmlCommentsPath());
                c.SingleApiVersion("0.1", "StateManagementService");
            }).EnableSwaggerUi();


            appBuilder.UseWebApi(config);

            config.EnsureInitialized();
        }

        private static string GetXmlCommentsPath()
        {
            return string.Format(@"{0}\XmlDocComments.xml",
                System.AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
