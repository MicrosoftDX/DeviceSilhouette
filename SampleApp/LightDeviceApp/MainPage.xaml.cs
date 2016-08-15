using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.System.Threading;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LightDeviceApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();           
        
            Task.Run(
                 async () => {
                     while (true)
                     {                         
                         var message = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
                         await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () => {                             
                             proccesMessage(message);                             
                         });                         
                     }
                 }
                 );

        }

        private void proccesMessage(Message message)
        {
            if (message == null) return;
                     
            var messageData = Encoding.ASCII.GetString(message.GetBytes());
            incomingMessage.Text = "Last incoming message: " + messageData;            

            // TODO: add error handling
            string messageType = message.Properties["MessageType"];
            string messageSubType = message.Properties["MessageSubType"];

            if (messageType.Equals("CommandRequest"))
            {
                switch (messageSubType)
                {
                    case "SetState":
                        setState(messageData);
                        break;
                    case "LatestState":
                        break;
                    case "ReportState":
                        reportState();
                        break;
                }
            }
            else if (messageType.Equals("InquiryResponse") && messageSubType.Equals("GetState")) {
                reportState();
            }
        }

        private void reportState()
        {
            if (toggleSwitch != null)
            {
                DeviceMessage msg = new DeviceMessage(toggleSwitch.IsOn);
                string json = JsonConvert.SerializeObject(msg);
                AzureIoTHub.SendDeviceToCloudMessageAsync(json, "Report", "State");
                outgoingMessage.Text = "Last outgoing message: " + json;
            }
        }

        private void setState(string messageData)
        {
            // expect the message data to hold a state for the light - on or off
            // "{"status" : "on"}"           
            DeviceMessage msg = JsonConvert.DeserializeObject<DeviceMessage>(messageData);
            toggleSwitch.IsOn = msg.status.Equals("on");
        }

        private void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                reportState();            
            }
        }
    }

    internal class DeviceMessage
    {
        public string status;

        public DeviceMessage(bool isOn)
        {
            status = isOn ? "on" : "off";
        }
    }
}
