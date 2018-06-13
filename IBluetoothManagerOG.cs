﻿using System;
using System.Collections.ObjectModel;
using OpenGloveApp.Models;

namespace OpenGloveApp
{
    public interface IBluetoothManagerOG
    {
        void HelloWorld(string message); 
        string HelloWorld(); //Sample flow 
        Collection<BluetoothDeviceModel> GetAllPairedDevices();
        string GetDataFromDevice();
        void OpenDeviceConnection(BluetoothDeviceModel bluetoothDevice);
    }
}