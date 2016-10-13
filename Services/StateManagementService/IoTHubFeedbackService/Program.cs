// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;
using Silhouette.ServiceFabricUtilities;

namespace IoTHubFeedbackService
{
    internal static class Program
    {
        private static IoTHubFeedbackService CreateIoTHubFeedbackService(StatelessServiceContext context)
        {
            var configurationSection = context.GetConfigurationSection("IoTHubFeedbackServiceSettings");
            string iotHubConnectionString = configurationSection["IotHubConnectionString"];
            return new IoTHubFeedbackService(context, iotHubConnectionString);
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


                ServiceRuntime.RegisterServiceAsync("IoTHubFeedbackServiceType", CreateIoTHubFeedbackService)
                    .GetAwaiter()
                    .GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(IoTHubFeedbackService).Name);

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

