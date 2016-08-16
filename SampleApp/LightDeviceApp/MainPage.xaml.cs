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
using System.Threading;

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
        }

        

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = connectStr.Text;
            if (connectionString != null)
            {
                AzureIoTHub.CreateDeviceClient(connectionString);
                connectButton.IsEnabled = false;
                connectStr.IsEnabled = false;
                MyFrame.Navigate(typeof(HomeLightPage));
            }
            
        }
    }    
}
