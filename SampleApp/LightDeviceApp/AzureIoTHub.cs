using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

static class AzureIoTHub
{
    //
    // Note: this connection string is specific to the device "light". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=silhouette-tests.azure-devices.net;DeviceId=light;SharedAccessKey=Zz0dD62TWmphjMVW3qL8owkfBSyBJpMQB9PpInQ+CIY=";

    //
    // To monitor messages sent to device "light" use iothub-explorer as follows:
    //    iothub-explorer HostName=silhouette-tests.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=3SEujwE6kt5IBmU746hse2H0jbAjX1skgMDnI+d4Tgg= monitor-events "light"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

    static DeviceClient deviceClient = createDeviceClient();

    private static DeviceClient createDeviceClient()
    {            
        return DeviceClient.CreateFromConnectionString(deviceConnectionString);
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
