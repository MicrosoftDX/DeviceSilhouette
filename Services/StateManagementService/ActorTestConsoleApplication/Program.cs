using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using DeviceRepository.Interfaces;
using DeviceStateNamespace;


namespace ActorTestConsoleApplication
{
    class Program
    {
        private static Uri serviceUri = new Uri("fabric:/StateManagementService/DeviceRepositoryActorService");
        

        static void Main(string[] args)
        {

            ActorId actorId = new ActorId("Device1");
            IDeviceRepositoryActor silhouette = ActorProxy.Create<IDeviceRepositoryActor>(actorId, serviceUri);


            //use interface?
            DeviceState state = new DeviceState(actorId.GetStringId(), "test");

            silhouette.SetDeviceStateAsync(state);
            var currentstate = silhouette.GetDeviceStateAsync().Result;
            Console.WriteLine(currentstate.CustomState);
            Console.ReadLine();

    }
    }
}
