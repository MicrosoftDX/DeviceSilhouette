using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using DeviceRepository.Interfaces;
using Microsoft.ServiceFabric.Actors.Query;
using System.Threading;
using DeviceRichState;

namespace ActorTestConsoleApplication
{
    class Program
    {
        private static Uri serviceUri = new Uri("fabric:/StateManagementService/DeviceRepositoryActorService");


        static void Main(string[] args)
        {
            var jsonState = @"{ ""silhouetteProperties"": { },""appMetadata"": { },""deviceValues"": { } }";
            var deviceId = "RichSilhouette1";
            var deviceState = new DeviceState(deviceId, "", "", Types.Report);

            var id = deviceState.CorrelationId;
            bool cont = true;

            while (cont)
            {
                Console.WriteLine("Enter device id");
                deviceId = Console.ReadLine();

                Console.WriteLine("Put, Set or Get?");
                string method = Console.ReadLine();

                ActorId actorId = new ActorId(deviceId);

                if (method == "Put" || (method == "Get" && DoesActorExist(deviceId)) || (method == "Set" && DoesActorExist(deviceId)))
                {
                    IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, serviceUri);
                    DeviceState currentstate = null;

                    if (method == "Put" || method == "Set")
                    {
                        DeviceState state = new DeviceState(actorId.GetStringId(), "", "", Types.Report);
                        currentstate = silhouette.SetDeviceStateAsync(state).Result;
                    }

                    if (method == "Get")
                    {
                        currentstate = silhouette.GetDeviceStateAsync().Result;

                        //State history
                        var stateMessages = silhouette.GetDeviceStateMessagesAsync().Result;
                    }

                    //Display device silhouette state
                    Console.WriteLine("Current state");
                    if (currentstate != null)
                    {
                        Console.WriteLine("Device : {0}", currentstate.DeviceId);
                        Console.WriteLine("Data version : {0}", currentstate.Version);
                        Console.WriteLine("Data timestamp : {0}", currentstate.Timestamp);
                        Console.WriteLine("Custom data : {0}", currentstate.Values);
                    }
                    else
                    {
                        Console.WriteLine("State not set");
                    }

                    //List all actors in partition
                    var actors = GetActors(actorId.GetPartitionKey());

                    Console.WriteLine("Registered devices :");
                    foreach (ActorInformation actor in actors)
                    {
                        Console.WriteLine(actor.ActorId.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("Actor {0} does not exist, use Put", deviceId);
                }

                Console.WriteLine("Press Enter to continue or 'stop' to exit");
                cont = (Console.ReadLine() != "stop");
            }
            Console.ReadLine();
            }

        static bool DoesActorExist(string deviceID)
        {
            //loop over partitions ... dev only has 1 partition
            var actors = GetActors(1);
            foreach (ActorInformation actor in actors)
            {
                if (actor.ActorId.ToString() == deviceID) { return true; };
            }
            return false;
        }

        static List<ActorInformation> GetActors(long partition)
        {
            IActorService actorServiceProxy = ActorServiceProxy.Create(serviceUri, partition);

            CancellationToken cancellationToken;
            ContinuationToken continuationToken = null;
            List<ActorInformation> activeActors = new List<ActorInformation>();

            do
            {
                PagedResult<ActorInformation> page = actorServiceProxy.GetActorsAsync(continuationToken, cancellationToken).Result;

                activeActors.AddRange(page.Items.Where(x => x.IsActive));

                continuationToken = page.ContinuationToken;
            }
            while (continuationToken != null);

            return activeActors;

        }
    }
}
