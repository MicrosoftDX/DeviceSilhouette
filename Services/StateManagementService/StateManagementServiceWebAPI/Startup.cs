using System.Web.Http;
using Owin;

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

            appBuilder.UseWebApi(config);
        }
    }
}
