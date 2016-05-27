using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using DeviceRepository.Interfaces;
using DeviceStateNamespace;
using Microsoft.ServiceFabric.Actors.Query;
using System.Threading;

namespace ActorTestConsoleApplication
{
    class Program
    {
        private static Uri serviceUri = new Uri("fabric:/StateManagementService/DeviceRepositoryActorService");
        

        static void Main(string[] args)
        {

            bool cont = true;

            while (cont)
            {
                Console.WriteLine("Enter deviceID");
                string deviceID = Console.ReadLine();

                Console.WriteLine("Put, Set or Get?");
                string method = Console.ReadLine();

                ActorId actorId = new ActorId(deviceID);

                if (method == "Put" || (method == "Get" && DoesActorExist(deviceID)) || (method == "Set" && DoesActorExist(deviceID)) )
                {
                    IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, serviceUri);

                    if (method == "Set")
                    {
                        //use interface?
                        DeviceState state = new DeviceState(actorId.GetStringId(), "test");
                        state.Status = "Requested";
                        silhouette.SetDeviceStateAsync(state).Wait();
                    }

                    //Display device silhouette state
                    var currentstate = silhouette.GetDeviceStateAsync().Result;
                    Console.WriteLine("Device : {0}", currentstate.DeviceID);
                    Console.WriteLine("Data version : {0}", currentstate.Version);
                    Console.WriteLine("Data timestamp : {0}", currentstate.Timestamp);
                    Console.WriteLine("Custom data : {0}", currentstate.CustomState);

                    if (method == "Get")
                    {
                        //State history
                        var stateMessages = silhouette.GetDeviceStateMessagesAsync().Result;
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
                    Console.WriteLine("Actor {0} does not exist, use Put", deviceID);
                }

                Console.WriteLine("Press Enter to continue or 'stop' to exit");
                cont = (Console.ReadLine() != "stop");
            }

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
