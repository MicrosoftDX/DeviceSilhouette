using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using SilhouetteRestClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

using Windows.UI.Core;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HomeLightsMobile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string _state = "unknown";
        public MainPage()
        {
          
            this.InitializeComponent();

            Task.Run(
                 async () =>
                 {
                     while (true)
                     {
                         

                         await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                         {
                             CheckCurrentState();
                         });
                     }
                 }
                 );
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            RestClient restClient = new RestClient();
            var result = await restClient.GetLatestReportedState();
            GetStateFromResult(result);

            change_image();
            textBox.Text = result;



        }

        private async void CheckCurrentState()
        {
            RestClient restClient = new RestClient();
            var result = await restClient.GetLatestReportedState();
            GetStateFromResult(result);

            change_image();
            textBox.Text = result;



        }

        private void GetStateFromResult(string result)
        {
            try
            {
                dynamic jObj = (JObject)JsonConvert.DeserializeObject(result);
                _state = jObj["values"]["status"].Value;
            }
            catch (Exception ex) // result is null or contain error message
            {

            }
   
           
        }

        private void change_image()
        {
            if (_state == "on")
            {

                BitmapImage newImage = new BitmapImage();
                newImage.UriSource = new Uri("ms-appx:///Assets/on.gif");
                image.Source = newImage;
               
            }
            else if (_state == "off")
            {

                BitmapImage newImage = new BitmapImage();
                newImage.UriSource = new Uri("ms-appx:///Assets/off.gif");
                image.Source = newImage;
               
            }
            else // (_state == "unknown")
            {

                BitmapImage newImage = new BitmapImage();
                newImage.UriSource = new Uri("ms-appx:///Assets/unknown.gif");
                image.Source = newImage;
               
            }
        }

        private async void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            RestClient restClient = new RestClient();
            ChangeState();
            var result = await restClient.UpdateState(_state);
            textBox.Text = result;
        }

        private void ChangeState()
        {
            if (_state == "on")
                _state = "off";
            else if (_state == "off")
                _state = "on";

        }
      
    }

    public enum state
    {
        off,
        on
    }
}
