using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using Razer.Emily.Common;
using Razer.Emily.UI;
//using Razer.Emily.UISDK;
using Razer.Storage;

class Program
{
    static string defaultDirectory = @"C:\Program Files (x86)\Razer\Synapse";
    static void Main(string[] args)
    {
        if (File.Exists(AssemblyLocation(true) + "\\RzSynapse.exe"))
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
        else
        {
            DialogResult result = DialogResult.None;
            if (args.Length < 1)
            {
                if (Directory.Exists(defaultDirectory))
                    result = MessageBox.Show("The program is not in the Synapse folder. Automatically move it?", "Synapse Was Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                else
                {
                    MessageBox.Show("The program is not in the Synapse folder, and we can't find the directory. You probably installed it somewhere else. Please manually move the program to the root of the Razer Synapse directory.");
                    return;
                }
            }
            if (result == DialogResult.Yes || args.Length > 0)
            {
                WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
                if (!hasAdministrativeRight)
                {
                    string fileName = Assembly.GetExecutingAssembly().Location;
                    ProcessStartInfo processInfo = new ProcessStartInfo
                    {
                        Verb = "runas",
                        Arguments = "-true",
                        FileName = fileName
                    };
                    try
                    {
                        Process.Start(processInfo);
                    }
                    catch
                    {
                        //Thrown if cancelled.
                        MessageBox.Show("The program needs admin to move the file to your program files.");
                        MessageBox.Show("The program will now exit.");
                        return;
                    }
                    return;
                }
                else
                {
                    string path = defaultDirectory + "\\" + Path.GetFileName(AssemblyLocation(false));
                    File.Copy(AssemblyLocation(false), path, true);
                    Process.Start(path);
                    return;
                }
            }
            if (result == DialogResult.No)
            {
                MessageBox.Show("The program will now exit.");
                return;
            }
        }
    }

    public static string AssemblyLocation(bool directory)
    {
        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return directory ? Path.GetDirectoryName(path) : path;
    }
}