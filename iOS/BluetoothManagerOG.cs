using System;
using System.Diagnostics;
using OpenGloveApp.iOS;

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

        public void HelloWorld()
        {
            Debug.WriteLine("Hello World iOS");
        }
    }
}
