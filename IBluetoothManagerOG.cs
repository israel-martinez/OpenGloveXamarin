using System;
using System.Collections.Generic;
using OpenGloveApp.Models;
using Xamarin.Forms;

namespace OpenGloveApp
{
    public interface IBluetoothManagerOG
    {
        void HelloWorld(string message); 
        string HelloWorld(); //Sample flow 
        List<BluetoothDeviceModel> GetAllPairedDevices();
        void OpenDeviceConnection(ContentPage contentPage, BluetoothDeviceModel bluetoothDevice);
        void ActivateMotor();
        //void DeactivateMotor();
    }
}
