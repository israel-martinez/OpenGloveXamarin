using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.ObjectModel;
using OpenGloveApp.Models;

namespace OpenGloveApp
{
    public partial class OpenGloveAppPage : ContentPage
    {
        
        public OpenGloveAppPage()
        {
            InitializeComponent();
        }

        void ShowBoundedDevices_Clicked(object sender, System.EventArgs e)
        {
            listViewBoundedDevices.ItemsSource = DependencyService.Get<IBluetoothManagerOG>().GetAllPairedDevices();
        }

        void Status_Clicked(object sender, System.EventArgs e)
        {
            var helloWorld = DependencyService.Get<IBluetoothManagerOG>().HelloWorld();
            DisplayAlert("Hello world sample",helloWorld,"OK");

        }

        void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            listViewBoundedDevices.SelectedItem = null;
        }

        async void Handle_ItemTappedAsync(object sender, ItemTappedEventArgs e)
        {
            var device = e.Item as BluetoothDeviceModel;
            bool connect = await DisplayAlert("Try Connecting", $" Device: {device.Name} \n MAC Address: {device.Address}", "Connect","Cancel");
            //Blocking call
            if(connect)
                DependencyService.Get<IBluetoothManagerOG>().OpenDeviceConnection(device);
        }
    }
}
