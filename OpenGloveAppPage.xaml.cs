using Xamarin.Forms;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;
using Plugin.BLE;
using System.Diagnostics;

namespace OpenGloveApp
{
    public partial class OpenGloveAppPage : ContentPage
    {
        IBluetoothLE mBluetoothLE;
        IAdapter mAdapter;
        ObservableCollection<IDevice> mDeviceList;
        ObservableCollection<string> mSampleList;

        public OpenGloveAppPage()
        {
            InitializeComponent();
            mBluetoothLE = CrossBluetoothLE.Current;
            mAdapter = CrossBluetoothLE.Current.Adapter;
            mDeviceList = new ObservableCollection<IDevice>();
            mSampleList = new ObservableCollection<string>();
            mSampleList.Add("First element");
            mSampleList.Add("Second element");
            mSampleList.Add("Third element");

            label_bluetooth_status.Text = mBluetoothLE.State.ToString();

            mBluetoothLE.StateChanged += (s, e) =>
            {
                label_bluetooth_status.Text = mBluetoothLE.State.ToString();
                Debug.WriteLine($"The bluetooth state changed to {e.NewState}");
            };

            DependencyService.Get<IBluetoothManagerOG>().HelloWorld();
        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            //GetSystemDevices();
            //DisplayAlert("Connection", "Trying connected whit device", "Ok");
            ScanForDevicesAsync();
        }

        void Status_Clicked(object sender, System.EventArgs e)
        {   
            var state = mBluetoothLE.State;
            DisplayAlert("Bluetooth status", state.ToString(), "Ok");

            ShowDeviceList();
            GetSystemDevices();

            Debug.WriteLine($"mSampleList: Count: {mSampleList.Count}");
        }

        void ShowDeviceList()
        {
            Debug.WriteLine($"System Devices List Count: {mDeviceList.Count}");
            foreach (IDevice device in mDeviceList)
            {
                Debug.WriteLine($"Name: {device.Name} GUID: {device.Id}");
            }
        }

        void GetSystemDevices()
        {
            var systemDevices = mAdapter.GetSystemConnectedOrPairedDevices();
            Debug.WriteLine($"Get System Devices Count: {systemDevices.Count}");
            foreach (IDevice device in systemDevices)
            {
                //await _adapter.ConnectToDeviceAsync(device);
                Debug.WriteLine($"Name: {device.Name} GUID: {device.Id}");
            }
        }

        async void ScanForDevicesAsync()
        {
            mAdapter.DeviceDiscovered += (s, a) => mDeviceList.Add(a.Device);
            await mAdapter.StartScanningForDevicesAsync();
        }


    }
}
