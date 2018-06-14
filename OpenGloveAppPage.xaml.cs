using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.ObjectModel;
using OpenGloveApp.Models;
using System;
using System.Collections.Generic;

namespace OpenGloveApp
{
    public partial class OpenGloveAppPage : ContentPage
    {
        // Publisher for UI commands to bluetoothManager
        public event EventHandler<BluetoothEventArgs> BluetoothMessageSended;
        public bool isMotorActive = false;
        public bool isMotorInitialize = false;
        public const int INITIALIZE_MOTORS = 1;
        public const int ACTIVATE_MOTORS = 2;
        public const int DISABLE_MOTORS = 3;
        public const int FLEXOR_READ = 4;

        // Vibe board: +11 y -12
        public Collection<int> mPins = new Collection<int> { 11, 12 };
        public Collection<string> mValuesON = new Collection<string> { "HIGH", "LOW" };
        public Collection<string> mValuesOFF = new Collection<string> { "LOW", "LOW" };

        // Flexor pins: 17 and  + and -
        public Collection<int> mFlexorPins = new Collection<int> { 17 };
        public Collection<int> mFlexorMapping = new Collection<int> { 8 };
        public Collection<string> mFlexorPinsMode = new Collection<string> { "OUTPUT" };

        public OpenGloveAppPage()
        {
            InitializeComponent();
            buttonActivateMotor.Text = "Motor OFF";
        }

        //Subcriber method
        public void OnBluetoothMessage(object source, BluetoothEventArgs e)
        {   
            // Handler for message from bluetooth ConnectedThread
            Device.BeginInvokeOnMainThread(() =>
            {
                if (e.Message != null)
                {
                    double value = double.Parse(e.Message);
                    progressBar_flexor_value.Progress = (value/300);
                    label_flexor_value.Text = e.Message;
                }
            });
        }

        // Method to raise event
        protected virtual void OnBluetoothMessageSended(int what, IEnumerable<int> pins, IEnumerable<string> values)
        {
            BluetoothMessageSended(this, new BluetoothEventArgs() 
            {What = what, Pins = pins, ValuesON = values, ValuesOFF = values});
        }

        void ShowBoundedDevices_Clicked(object sender, System.EventArgs e)
        {
            listViewBoundedDevices.ItemsSource = DependencyService.Get<IBluetoothManagerOG>().GetAllPairedDevices();
        }

        void ButtonActivateMotor_Clicked(object sender, System.EventArgs e)
        {
            //var helloWorld = DependencyService.Get<IBluetoothManagerOG>().HelloWorld();
            //DisplayAlert("Hello world sample",helloWorld,"OK");
            if (!isMotorInitialize)
            {
                OnBluetoothMessageSended(INITIALIZE_MOTORS, mPins, mValuesOFF);
                isMotorInitialize = true;
            }

            if (isMotorActive)
            {
                buttonActivateMotor.Text = "Motor OFF";
                OnBluetoothMessageSended(DISABLE_MOTORS, mPins, mValuesOFF);
                isMotorActive = false;
            }
            else
            {
                buttonActivateMotor.Text = "Motor ON";
                OnBluetoothMessageSended(ACTIVATE_MOTORS, mPins, mValuesON);
                isMotorActive = true;
            }
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
                DependencyService.Get<IBluetoothManagerOG>().OpenDeviceConnection(this, device);
        }
    }
}
