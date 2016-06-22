using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.ObjectModel;
using System.Fabric.Description;

namespace CommunicationProviderService
{
    internal static class Program
    {
        private static CommunicationProviderService CreateCommuncationProviderService(StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            KeyedCollection<string, ConfigurationProperty> clusterConfigParameters = configurationPackage.Settings.Sections["CommunicationProviderServiceSettings"].Parameters;

            string iotHubConnectionString = clusterConfigParameters["IotHubConnectionString"].Value;
            string storageConnectionString = clusterConfigParameters["StorageConnectionString"].Value;

            return new CommunicationProviderService(context, iotHubConnectionString, storageConnectionString);
        }

        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.


                ServiceRuntime.RegisterServiceAsync("CommunicationProviderServiceType",
                    context => CreateCommuncationProviderService(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(CommunicationProviderService).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
