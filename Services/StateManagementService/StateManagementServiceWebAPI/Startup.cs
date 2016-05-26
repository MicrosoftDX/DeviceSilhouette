using System.Web.Http;
using Owin;
using Swashbuckle.Application;

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

            config.MapHttpAttributeRoutes();

            config.EnableSwagger(c =>
            {
                //c.IncludeXmlComments("docs.xml");
                c.SingleApiVersion("1.0", "StateManagementService");
            }).EnableSwaggerUi();


            appBuilder.UseWebApi(config);
        }
    }
}
