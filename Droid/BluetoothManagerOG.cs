using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Android.Bluetooth;
using Java.IO;
using OpenGloveApp.Droid;
using OpenGloveApp.Models;

[assembly: Xamarin.Forms.Dependency(typeof(BluetoothManagerOG))]
namespace OpenGloveApp.Droid
{
    public class BluetoothManagerOG : IBluetoothManagerOG
    {
        // Universally Unique Identifier
        private const string mUUID = "1e966f42-52a8-45db-9735-5db0e21b881d";
        private BluetoothAdapter mBluetoothAdapter;
        private BluetoothDevice mDevice;
        private BluetoothSocket mSocket;
        private InputStreamReader mInputStreamReader;
        private BufferedReader mReader;
        private Stream mStream;
        private OutputStream mOutStream;
        private Collection<BluetoothDeviceModel> mBoundedDevicesModel;
        private Hashtable mBoundedDevices = new Hashtable();

        public BluetoothManagerOG()
        {
            mInputStreamReader = null;
            mBoundedDevicesModel = new Collection<BluetoothDeviceModel>();
            mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        }

        public Java.Util.UUID GetUUID()
        {
            return Java.Util.UUID.FromString(mUUID);
        }

        private void Close(IDisposable aConnectedObject)
        {
            if (aConnectedObject == null) return;
            try
            {
                aConnectedObject.Dispose();
            }
            catch(Exception e)
            {
                throw e;
            }
            aConnectedObject = null;
        }

        public string GetDataFromDevice()
        {
            return mReader.Read().ToString();
        }
        /*
        public bool OpenDeviceConnection(BluetoothDeviceModel deviceModel)
        {
            bool connected = false;
            Thread thread = new Thread(() =>
            {
                try 
                {
                    BluetoothDevice device = mBoundedDevices[deviceModel.Name] as BluetoothDevice;
                    mSocket = device.CreateRfcommSocketToServiceRecord(GetUUID());
                    mSocket.Connect(); // Blocking operation
                    mStream = mSocket.InputStream;
                    mInputStreamReader = new InputStreamReader(mStream);
                    mReader = new BufferedReader(mInputStreamReader);
                    connected = true;
                } 
                catch (System.IO.IOException e)
                {
                    Close(mSocket);
                    Close(mStream);
                    Close(mReader);
                    connected = false;
                    throw e;
                }
            });

            thread.IsBackground = true;
            thread.Start();

            return connected;
        }
        */

        public void OpenDeviceConnection(BluetoothDeviceModel bluetoothDevice)
        {
            mDevice = mBoundedDevices[bluetoothDevice.Name] as BluetoothDevice;
            ConnectThread connectThread = new ConnectThread(mDevice);
            connectThread.Start();
        }

        private class ConnectThread : Java.Lang.Thread
        {
            private BluetoothSocket mmSocket;
            private BluetoothDevice mmDevice;
            private BluetoothAdapter mmBluetoothAdapter;

            public ConnectThread(BluetoothDevice device)
            {
                BluetoothSocket temporalSocket = null;
                mmDevice = device;

                try
                {
                    Java.Util.UUID uuid = Java.Util.UUID.FromString(mUUID);
                    temporalSocket = (BluetoothSocket)mmDevice.Class.GetMethod("createRfcommSocket", new Java.Lang.Class[] {Java.Lang.Integer.Type}).Invoke(mmDevice,1);
                    Debug.WriteLine("BluetoothSocket: CREATED");
                    Debug.WriteLine($"Name: {device.Name}, Address: {device.Address}");
                }
                catch(Java.IO.IOException e)
                {
                    Debug.WriteLine("BluetoothSocket: NOT CREATED");
                    e.PrintStackTrace();
                }
                mmSocket = temporalSocket;
            }

            override
            public void Run()
            {
                // Cancel discovery because it will slow down the connection
                mmBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                mmBluetoothAdapter.CancelDiscovery();

                try
                {
                    // Connect the device through the socket. This will block
                    // until it succeeds or throws an exception
                    mmSocket.Connect();
                    Debug.WriteLine("BluetoothSocket: CONNECTED");
                }
                catch(Java.IO.IOException e)
                {
                    mmSocket.Close();
                    Debug.WriteLine("BluetoothSocket: NOT CONNECTED");
                    e.PrintStackTrace();
                }

                // Do work to manage the connection (in a separate thread)
                // TODO manageConnectedSocket(mmSocket);
                //ConnectedThread connectedThread = new ConnectedThread(mmSocket);
                //connectedThread.start();
            }
        }


        public Collection<BluetoothDeviceModel> GetAllPairedDevices()
        {
            mBoundedDevices.Clear();
            mBoundedDevicesModel.Clear();

            var devices = mBluetoothAdapter.BondedDevices;

            if (devices != null && devices.Count > 0)
            {
                foreach (BluetoothDevice device in devices)
                {
                    mBoundedDevices.Add(device.Name, device);
                    mBoundedDevicesModel.Add(new BluetoothDeviceModel
                    {
                        Name = device.Name,
                        Address = device.Address
                    });
                }
            }
            return mBoundedDevicesModel;
        }
        // TODO quit this example implements methods

        public void HelloWorld(string message)
        {
            Debug.WriteLine(message);
        }

        public string HelloWorld()
        {
            Debug.WriteLine("Hello World Droid");
            return "Hello World Droid";
        }
    }
}
