using System;
using System.Collections.ObjectModel;
using OpenGloveApp.Models;
using Xamarin.Forms;

namespace OpenGloveApp
{
    public interface IBluetoothManagerOG
    {
        void HelloWorld(string message); 
        string HelloWorld(); //Sample flow 
        Collection<BluetoothDeviceModel> GetAllPairedDevices();
        void OpenDeviceConnection(ContentPage contentPage, BluetoothDeviceModel bluetoothDevice);
        void ActivateMotor();
        //void DeactivateMotor();
    }
}
