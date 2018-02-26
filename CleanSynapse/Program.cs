//using System;
using System.Windows.Forms;
using Razer.Emily.Common;
using Razer.Emily.UI;
//using Razer.Emily.UISDK;
using Razer.Storage;

class Program
{
    static void Main()
    {
        MainStorage mainApp = Singleton<MainStorage>.Instance;
        mainApp.Initialize(Storage_Project.Emily);
        if (!mainApp.TryLastLogin())
            MessageBox.Show("Please login on Razer Synapse first.");
        else
        {
            RzDeviceManager dManager = new RzDeviceManager();
            dManager.Enumerate();
            CommonConfigLoader configLoader = Singleton<CommonConfigLoader>.Instance;
            configLoader.StartConfig();
            RzDevice[] allDevices = dManager.ActiveDevices.ToArray();
            foreach (RzDevice rzDevice in allDevices)
            {
                rzDevice.RefreshData();
                configLoader.DeviceAdded(rzDevice.VID, rzDevice.PID);
                //PluginDevice pluginDevice = configLoader.FindDevice(rzDevice.PID);
                //Console.WriteLine("Loaded: " + pluginDevice.Name);
            }
        }
    }
}