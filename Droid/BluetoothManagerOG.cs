using System;
using System.Diagnostics;
using Android.Bluetooth;
using OpenGloveApp.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(BluetoothManagerOG))]
namespace OpenGloveApp.Droid
{
    public class BluetoothManagerOG : IBluetoothManagerOG
    {
        // Universally Unique Identifier
        private const string UUID = "1e966f42-52a8-45db-9735-5db0e21b881d";
        private BluetoothDevice mSocket;

        public BluetoothManagerOG()
        {
        }

        public void HelloWorld(string message)
        {
            Debug.WriteLine(message);
        }

        public void HelloWorld()
        {
            Debug.WriteLine("Hello World Droid");
        }
    }
}
