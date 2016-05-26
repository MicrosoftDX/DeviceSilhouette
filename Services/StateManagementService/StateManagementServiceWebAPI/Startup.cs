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

            //create the route for http://<endpoint>/devices/{DeviceId}
            config.Routes.MapHttpRoute(
               // name: "DefaultApi",
                name: "DevicesApi",
                routeTemplate: "{controller}/{DeviceId}",
                defaults: new { DeviceId = RouteParameter.Optional }
                
            );
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
