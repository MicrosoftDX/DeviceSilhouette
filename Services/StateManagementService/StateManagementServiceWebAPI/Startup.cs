using System.Web.Http;
using Owin;
using Swashbuckle.Application;
using Newtonsoft.Json.Serialization;

namespace StateManagementServiceWebAPI
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

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
