using System;
using System.Diagnostics;
using OpenGloveApp.iOS;
using OpenGloveApp.Models;
using Xamarin.Forms;
using System.Collections.Generic;

[assembly: Xamarin.Forms.Dependency(typeof(BluetoothManagerOG))]
namespace OpenGloveApp.iOS
{
    public class BluetoothManagerOG : IBluetoothManagerOG
    {
        public BluetoothManagerOG()
        {
        }

        public void HelloWorld(string message)
        {
            Debug.WriteLine(message);
        }

        public string HelloWorld()
        {
            Debug.WriteLine("Hello World iOS");
            return "Hello World iOS"; 
        }

        public void OpenDeviceConnection(BluetoothDeviceModel bluetoothDevice)
        {
            throw new NotImplementedException();
        }

        public List<BluetoothDeviceModel> GetAllPairedDevices()
        {
            throw new NotImplementedException();
        }

        public void OpenDeviceConnection(ContentPage contentPage, BluetoothDeviceModel bluetoothDevice)
        {
            throw new NotImplementedException();
        }

        public void ActivateMotor()
        {
            throw new NotImplementedException();
        }
    }
}
