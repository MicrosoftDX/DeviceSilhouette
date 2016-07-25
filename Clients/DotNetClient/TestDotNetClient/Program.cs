using DotNetClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDotNetClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            try
            {
                MainAsync(args).Wait();
            }
            catch (AggregateException ae)
            {
                Console.WriteLine(ae.InnerException);
            }
        }
        static async Task MainAsync(string[] args)
        {
            const string templateConnectionString = "%Silhouette_IotHubConnectionString%";
            string connectionString = Environment.ExpandEnvironmentVariables(templateConnectionString);
            if (connectionString == templateConnectionString)
            {
                throw new Exception("Ensure that the Silhouette_IotHubConnectionString environment variable is set");
            }

            string deviceId = "dotNetDevice"; // TODO parameterise
            //string deviceId = "device1"; // TODO parameterise

            var device = new DeviceSimulator(connectionString, deviceId);
            device.ReceivedMessage += Device_ReceivedMessage;
            await device.InitializeAsync();
            device.StartReceiveMessageLoop();

            int i = 0;
            while (true)
            {
                Console.WriteLine($"Sending state counterValue={i}");
                await device.SendStateMessageAsync(new { counterValue = i });
                i++;
                await Task.Delay(2000);
            }
        }

        private static void Device_ReceivedMessage(object sender, ReceiveMessageEventArgs e)
        {
            var message = e.Message;
            Console.WriteLine("Received Message:");
            Console.WriteLine($"\tMessageType\t{message.MessageType}");
            Console.WriteLine($"\tMessageSubType\t{message.MessageSubType}");
            Console.WriteLine($"\tBody\t{message.Body}");

            e.Action = ReceiveMessageAction.Complete;
        }
    }

}
