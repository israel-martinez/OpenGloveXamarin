using System;
using System.Diagnostics;
using OpenGloveApp.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(BluetoothManagerOG))]
namespace OpenGloveApp.iOS
{
    public class BluetoothManager : IBluetoothManagerOG
    {
        public BluetoothManager()
        {
        }

        public void HelloWorld(string message)
        {
            Debug.WriteLine(message);
        }

        public void HelloWorld()
        {
            Debug.WriteLine("Hello World iOS");
        }
    }
}
