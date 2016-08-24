using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

static class AzureIoTHub
{    
    static DeviceClient deviceClient;

    public static void CreateDeviceClient(string connectionString)
    {            
        deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
    }

    public static async Task<Message> ReceiveCloudToDeviceMessageAsync()
    {
        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {                
                await deviceClient.CompleteAsync(receivedMessage);
                return receivedMessage;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }        
    }

    internal static async void SendDeviceToCloudMessageAsync(string messageBody, string messageType, string messageSubType)
    {
        var message = new Message(Encoding.UTF8.GetBytes(messageBody))
        {
            Properties = {
                    { "MessageType", messageType },
                    { "MessageSubType", messageSubType }
            }
        };
        
        await deviceClient.SendEventAsync(message);
    }    
}
