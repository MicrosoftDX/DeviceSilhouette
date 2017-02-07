// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Silhouette.ServiceFabricUtilities;

namespace DeviceRepository
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // This line registers an Actor Service to host your actor class with the Service Fabric runtime.
                // The contents of your ServiceManifest.xml and ApplicationManifest.xml files
                // are automatically populated when you build this project.
                // For more information, see http://aka.ms/servicefabricactorsplatform

                ActorRuntime.RegisterActorAsync<DeviceRepositoryActor>(CreateActorService).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static ActorService CreateActorService(StatefulServiceContext context, ActorTypeInformation actorType)
        {
            var configurationSection = context.GetConfigurationSection("DeviceRepositorySettings");
            long retention = long.Parse(configurationSection["MessagesRetentionMilliseconds"]);
            int messagesTimerInterval = int.Parse(configurationSection["MessagesTimerInterval"]);
            int minMessagesToKeep = int.Parse(configurationSection["MinMessagesToKeep"]);

            return new ActorService(context, actorType, (service, id) => new DeviceRepositoryActor(retention, messagesTimerInterval, minMessagesToKeep, service, id));
           


        }
    }
}

