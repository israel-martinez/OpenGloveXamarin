using System;
using System.Collections;
using System.Collections.Generic;
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
        private List<BluetoothDeviceModel> mBoundedDevicesModel;
        private Hashtable mBoundedDevices = new Hashtable();

        public BluetoothManagerOG()
        {
            mBoundedDevicesModel = new List<BluetoothDeviceModel>();
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
            private List<int> mFlexorPins = new List<int> { 17 }; //TODO get this from OpenGloveApp
            public int mEvaluation = 0; //OpenGloveAppPage.FLEXOR_EVALUATION; //OpenGloveAppPage.MOTOR_EVALUATION;

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
                    case OpenGloveAppPage.INITIALIZE_MOTORS:
                        {
                            Debug.WriteLine($"INITIALIZE_MOTORS: Initializing motors (thread: {this.Id})");
                            string message = mMessageGenerator.InitializeMotor( ((List<int>)e.Pins).GetRange(0,2).ToArray() );
                            this.Write(message);
                            break;
                        }
                    case OpenGloveAppPage.ACTIVATE_MOTORS:
                        {
                            Debug.WriteLine($"ACTIVATE_MOTORS: Activating motors (thread: {this.Id})");
                            Debug.WriteLine($"ValuesON: {((List<string>)e.ValuesON).Count}");
                            Debug.WriteLine($"ValuesON.range(0,2): { ((List<string>)e.ValuesON).GetRange(0,2).ToArray() }");
                            Debug.WriteLine($"Pins.range(0,2).Count: { ((List<int>)e.Pins).GetRange(0, 2).Count }");
                            string message = mMessageGenerator.ActivateMotor( ((List<int>)e.Pins).GetRange(0,2).ToArray(), ((List<string>)e.ValuesON).GetRange(0,2).ToArray() );
                            this.Write(message);
                            break;
                        }
                    case OpenGloveAppPage.DISABLE_MOTORS:
                        {
                            Debug.WriteLine($"DISABLE_MOTORS: Disable motors (thread: {this.Id})");
                            string message = mMessageGenerator.ActivateMotor(((List<int>)e.Pins).GetRange(0, 2).ToArray(), ((List<string>)e.ValuesOFF).GetRange(0, 2).ToArray() );
                            this.Write(message);
                            break;
                        }
                    case OpenGloveAppPage.FLEXOR_READ:
                        {
                            //Debug.WriteLine($"FLEXOR_READ: FOR READ PINS (thread: {this.Id})");
                            //mFlexorPins = e.FlexorPins as List<int>;
                            break;
                        }
                    case OpenGloveAppPage.FLEXOR_EVALUATION:
                        {
                            mEvaluation = OpenGloveAppPage.FLEXOR_EVALUATION;
                            break;
                        }
                    case OpenGloveAppPage.MOTOR_EVALUATION:
                        {
                            mEvaluation = OpenGloveAppPage.MOTOR_EVALUATION;
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

                switch(mEvaluation)
                {
                    case OpenGloveAppPage.FLEXOR_EVALUATION:
                        {
                            FlexorTest(1000, 1, "latency-test", "flexor_1XamarinGalaxy.csv");
                            break;
                        }
                    case OpenGloveAppPage.MOTOR_EVALUATION:
                        {
                            MotorTest(1000, 5, "latency-test", "motor_5XamarinGalaxy.csv");
                            break;
                        }
                    default:
                        {
                            FlexorCapture();
                            break;
                        }
                }
            }

            private void FlexorCapture()
            {
                // Keep listening to the InputStream whit a StreamReader until an exception occurs
                string line;
                while (true)
                {
                    try
                    {
                        line = AnalogRead(mFlexorPins[0]);

                        if (line != null)
                        {
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
                    catch (Java.IO.IOException e)
                    {
                        Debug.WriteLine($"CONNECTED THREAD {this.Id}: {e.Message}");
                    }
                }
            }

            private void FlexorTest(int samples, int flexors, String folderName, String fileName)
            {
                // Keep listening to the InputStream whit a StreamReader until an exception occurs
                string line;
                int counter = 0;
                List<long> latencies = new List<long>();
                IO.CSV csvWriter = new IO.CSV(folderName, fileName);

                Debug.WriteLine(csvWriter.ToString());

                Stopwatch stopWatch = new Stopwatch(); // for capture elapsed time
                TimeSpan ts;

                while (true)
                {
                    try
                    {
                        Debug.WriteLine("Counter: " + counter);
                        stopWatch = new Stopwatch();
                        stopWatch.Start();
                        line = AnalogRead(mFlexorPins[0]);
                        stopWatch.Stop();
                        ts = stopWatch.Elapsed;

                        if (counter < samples)
                        {
                            latencies.Add(ts.Ticks * 100); // nanoseconds https://msdn.microsoft.com/en-us/library/system.datetime.ticks(v=vs.110).aspx
                            if ((counter + 1) % 100 == 0) Debug.WriteLine("Counter: " + counter);
                        }
                        else
                        {
                            break;
                        }
                        counter++;

                        if (line != null)
                        {

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
                    catch (Java.IO.IOException e)
                    {
                        Debug.WriteLine($"CONNECTED THREAD {this.Id}: {e.Message}");
                    }
                }

                csvWriter.Write(latencies, "latencies-ns");
                Debug.WriteLine(csvWriter.ToString());    
            }

            private void MotorTest(int samples, int motors, string folderName, string fileName)
            {
                string message;
                int counter = 0;
                List<long> latencies = new List<long>();
                List<int> pins = new List<int>(OpenGloveAppPage.mPins.GetRange(0, motors * 2));
                List<string> valuesON = new List<string>(OpenGloveAppPage.mValuesON.GetRange(0, motors * 2));
                List<string> valuesOFF = new List<string>(OpenGloveAppPage.mValuesOFF.GetRange(0, motors * 2));
                IO.CSV csvWriter = new IO.CSV(folderName, fileName);

                Debug.WriteLine(csvWriter.ToString());

                Stopwatch stopWatch = new Stopwatch(); // for capture elapsed time
                TimeSpan ts;

                while (true)
                {
                    if (counter < samples)
                    {
                        try
                        {
                            stopWatch = new Stopwatch();
                            stopWatch.Start();

                            message = mMessageGenerator.ActivateMotor(pins, valuesON);
                            this.Write(message); // Activate the motors

                            message = mMessageGenerator.ActivateMotor(pins, valuesOFF);
                            this.Write(message); // Disable the motors

                            stopWatch.Stop();
                            ts = stopWatch.Elapsed;

                            latencies.Add(ts.Ticks * 100);
                            if ((counter + 1) % 100 == 0) Debug.WriteLine("Counter: " + counter);
                        }
                        catch (Java.IO.IOException e)
                        {
                            e.PrintStackTrace();
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    counter++;
                }

                csvWriter.Write(latencies, "latencies-ns");
                Debug.WriteLine(csvWriter.ToString());
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


        public List<BluetoothDeviceModel> GetAllPairedDevices()
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
