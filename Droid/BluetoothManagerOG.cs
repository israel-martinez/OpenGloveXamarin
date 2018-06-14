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
using Xamarin.Forms;

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

        public void OpenDeviceConnection(ContentPage contentPage, BluetoothDeviceModel bluetoothDevice)
        {
            mDevice = mBoundedDevices[bluetoothDevice.Name] as BluetoothDevice;
            ConnectThread connectThread = new ConnectThread(contentPage, mDevice);
            connectThread.Start();
        }

        private class ConnectThread : Java.Lang.Thread
        {
            private BluetoothSocket mmSocket;
            private BluetoothDevice mmDevice;
            private BluetoothAdapter mmBluetoothAdapter;
            private ContentPage mmContentPage;

            public ConnectThread(ContentPage contentPage, BluetoothDevice device)
            {
                BluetoothSocket auxSocket = null;
                mmDevice = device;
                mmContentPage = contentPage;

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
                    ConnectedThread connectedThread = new ConnectedThread(mmContentPage, mmSocket);
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
            // Event for send data to UI thread on Main Xamarin.Forms project
            public event EventHandler<BluetoothEventArgs> BluetoothDataReceived;

            private BluetoothSocket mmSocket;
            private StreamReader mmInputStreamReader;
            private Stream mmOutputStream;
            private MessageGenerator mMessageGenerator = new MessageGenerator();
            private Collection<int> mFlexorPins = new Collection<int> { 17 }; //TODO get this from OpenGloveApp

            public ConnectedThread(ContentPage contentPage, BluetoothSocket bluetoothSocket)
            {
                mmSocket = bluetoothSocket;
                try
                {
                    mmInputStreamReader = new StreamReader(mmSocket.InputStream);
                    mmOutputStream = mmSocket.OutputStream;

                    //subscribe UI thread on this especific ConnectedThread for get data
                    this.BluetoothDataReceived += ((OpenGloveAppPage)contentPage).OnBluetoothMessage; //UI thread subscribe to this instance of ConnectedThread
                    ((OpenGloveAppPage)contentPage).BluetoothMessageSended += this.OnBluetoothMessageSended; //This thread subscribe to UI thread for get commands

                }
                catch(System.IO.IOException e)
                {
                    mmInputStreamReader.Close();
                    mmOutputStream.Close();
                    Debug.WriteLine(e.Message);
                }
            }

            // Method for raise the event 
            protected virtual void OnBluetootDataReceived(long threadId, string message)
            {
                if (BluetoothDataReceived != null)
                    BluetoothDataReceived(this, new BluetoothEventArgs() 
                    { ThreadId = threadId, Message = message });
            }

            //Handle event from UI thread
            public void OnBluetoothMessageSended(object source, BluetoothEventArgs e)
            {
                switch(e.What)
                {
                    case (OpenGloveAppPage.INITIALIZE_MOTORS):
                        {
                            Debug.WriteLine($"INITIALIZE_MOTORS: Initializing motors (thread: {this.Id})");
                            string message = mMessageGenerator.InitializeMotor((Collection<int>)e.Pins);
                            this.Write(message);
                            break;
                        }
                    case (OpenGloveAppPage.ACTIVATE_MOTORS):
                        {
                            Debug.WriteLine($"ACTIVATE_MOTORS: Activating motors (thread: {this.Id})");
                            Debug.WriteLine($"ValuesON: {((Collection<string>)e.ValuesON).Count}");
                            string message = mMessageGenerator.ActivateMotor((Collection<int>)e.Pins, ((Collection<string>)e.ValuesON));
                            this.Write(message);
                            break;
                        }
                    case (OpenGloveAppPage.DISABLE_MOTORS):
                        {
                            Debug.WriteLine($"DISABLE_MOTORS: Disable motors (thread: {this.Id})");
                            string message = mMessageGenerator.ActivateMotor((Collection<int>)e.Pins, ((Collection<string>)e.ValuesOFF));
                            this.Write(message);
                            break;
                        }
                    case (OpenGloveAppPage.FLEXOR_READ):
                        {
                            //Debug.WriteLine($"FLEXOR_READ: FOR READ PINS (thread: {this.Id})");
                            //mFlexorPins = e.FlexorPins as Collection<int>;
                            break;
                        }
                    default:
                        {
                            break; 
                        }
                }
            }

            override
            public void Run(){
                //TODO: capture data from bluetooth device
                try
                {
                    //Debug.WriteLine($"CONNECTED THREAD {this.Id}: Initializing motors");
                    //string message = mMessageGenerator.InitializeMotor(mPins);
                    //this.Write(message);
                }
                catch(System.IO.IOException e)
                {
                    Debug.WriteLine($"CONNECTED THREAD {this.Id}: Failed on Run()");
                    Debug.WriteLine(e.Message);
                }

                // Keep listening to the InputStream whit a StreamReader until an exception occurs
                string line;

                while (true){
                    try
                    {
                        Debug.WriteLine($"ON WHILE: thread {this.Id}");
                        line = AnalogRead(mFlexorPins[0]);
                        if (line != null)
                        {
                            Debug.WriteLine($"Flexor pin {mFlexorPins[0]}: {line}");
                            //Android.OS.Message message = MainActivity.mUIHandler.ObtainMessage(READ_PIN, line);
                            //message.SendToTarget();

                            //Raise the event to UI thread, that need stay subscriber to this publisher thread
                            //Send the current thread id and send Message
                            OnBluetootDataReceived(this.Id, line);
                        }
                        else 
                        {
                            Debug.WriteLine($"BluetoothSocket is Disconnected");
                            mmSocket.Connect();
                        }
                    }
                    catch(Java.IO.IOException e)
                    {
                        Debug.WriteLine($"CONNECTED THREAD {this.Id}: {e.Message}");
                    }
                }

            }

            public string AnalogRead(int pin)
            {
                string message = mMessageGenerator.AnalogRead(pin);
                this.Write(message);
                try
                {
                    return mmInputStreamReader.ReadLine();
                }
                catch (Exception e)
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
                catch(Java.IO.IOException e)
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

        public void ActivateMotor()
        {
            
        }
    }
}
