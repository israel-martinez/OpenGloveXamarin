using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Android.Bluetooth;
using Java.IO;
using OpenGloveApp.Droid;
using OpenGloveApp.Models;
using OpenGloveApp.OpenGloveAPI;

[assembly: Xamarin.Forms.Dependency(typeof(BluetoothManagerOG))]
namespace OpenGloveApp.Droid
{
    public class BluetoothManagerOG : IBluetoothManagerOG
    {
        // Universally Unique Identifier
        private const string mUUID = "1e966f42-52a8-45db-9735-5db0e21b881d";
        private BluetoothAdapter mBluetoothAdapter;
        private BluetoothDevice mDevice;
        private Collection<BluetoothDeviceModel> mBoundedDevicesModel;
        private Hashtable mBoundedDevices = new Hashtable();

        public BluetoothManagerOG()
        {
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
            catch (Exception e)
            {
                throw e;
            }
            aConnectedObject = null;
        }

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
                BluetoothSocket auxSocket = null;
                mmDevice = device;

                try
                {
                    Java.Util.UUID uuid = Java.Util.UUID.FromString(mUUID);
                    auxSocket = (BluetoothSocket)mmDevice.Class.GetMethod("createRfcommSocket", new Java.Lang.Class[] { Java.Lang.Integer.Type }).Invoke(mmDevice, 1);
                    Debug.WriteLine("BluetoothSocket: CREATED");
                    Debug.WriteLine($"Name: {device.Name}, Address: {device.Address}");
                }
                catch (Java.IO.IOException e)
                {
                    Debug.WriteLine("BluetoothSocket: NOT CREATED");
                    e.PrintStackTrace();
                }
                mmSocket = auxSocket;
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
                    // Do work to manage the connection (in a separate thread)
                    // TODO manageConnectedSocket(mmSocket);
                    ConnectedThread connectedThread = new ConnectedThread(mmSocket);
                    connectedThread.Start();
                }
                catch (Java.IO.IOException e)
                {
                    mmSocket.Close();
                    Debug.WriteLine("BluetoothSocket: NOT CONNECTED");
                    e.PrintStackTrace();
                }
            }
        }


        private class ConnectedThread : Java.Lang.Thread
        {
            private BluetoothSocket mmSocket;
            private StreamReader mmInputStream;
            private Stream mmOutputStream;
            private BufferedInputStream mmBufferedStream;
            private MessageGenerator mMessageGenerator = new MessageGenerator();

            private Android.OS.Handler mHandler;

            // Vibe board: +11 y -12
            Collection<int> mPins = new Collection<int> { 11, 12 };
            Collection<string> mValuesON = new Collection<string> { "HIGH", "LOW" };
            Collection<string> mValuesOFF = new Collection<string> { "LOW", "LOW" };

            // Flexor pins: 17 and  + and -
            Collection<int> mFlexorPins = new Collection<int> { 17 };
            Collection<int> mFlexorMapping = new Collection<int> { 8 };
            Collection<string> mFlexorPinsMode = new Collection<string> {"OUTPUT"};
          

            public ConnectedThread(BluetoothSocket bluetoothSocket)
            {
                mmSocket = bluetoothSocket;
                try
                {
                    mmInputStream = new StreamReader(mmSocket.InputStream);
                    mmOutputStream = mmSocket.OutputStream;
                    mmBufferedStream = new BufferedInputStream(mmSocket.InputStream);

                }
                catch(System.IO.IOException e)
                {
                    mmInputStream.Close();
                    mmOutputStream.Close();
                    mmBufferedStream.Close();
                    Debug.WriteLine(e.Message);
                }
            }

            override
            public void Run(){
                //TODO: capture data from bluetooth device
                try
                {
                    Debug.WriteLine($"CONNECTED THREAD {this.Id}: Initializing motors");
                    string message = mMessageGenerator.InitializeMotor(mPins);
                    this.Write(message);

                    Debug.WriteLine($"CONNECTED THREAD {this.Id}: Activating motors");
                    message = mMessageGenerator.ActivateMotor(mPins, mValuesON);
                    this.Write(message);
                }
                catch(System.IO.IOException e)
                {
                    Debug.WriteLine($"CONNECTED THREAD {this.Id}: Failed on Run()");
                    Debug.WriteLine(e.Message);
                }

                // Keep listening to the InputStream until an exception occurs
                string line;
                while (true){
                    try
                    {
                        line = AnalogRead(mFlexorPins[0]);
                        if (line != null) Debug.WriteLine($"Flexor pin {mFlexorPins[0]}: {line}");
                        line = null;
                    }
                    catch(System.IO.IOException e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }

            }

            public string AnalogRead(int pin)
            {
                string message = mMessageGenerator.AnalogRead(pin);
                this.Write(message);

                try
                {
                    return mmInputStream.ReadLine();
                }
                catch (System.IO.IOException e)
                {
                    Debug.WriteLine(e.Message);
                    return null;
                }
            }

            public void Write(string message)
            {
                try
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(message);
                    mmOutputStream.Write(bytes, 0, bytes.Length);
                }
                catch(System.IO.IOException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            public void Close()
            {
                try
                {
                    mmSocket.Close();
                }
                catch(Java.IO.IOException e)
                {
                    Debug.WriteLine(e.Message);
                }
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
