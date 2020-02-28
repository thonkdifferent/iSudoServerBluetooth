using System;
using System.Linq;
using System.Threading.Tasks;
using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;
//TODO: Add setup functionality
namespace iSudoBluetoothClient
{
    class Program
    {
        public static async Task<HashtagChris.DotNetBlueZ.Device> GetiSudoDevice(string[] args)
        {
            //TODO: in-app pair
            try{
    #region GettingBluetooth
                var adapters = await BlueZManager.GetAdaptersAsync();//get adapters
                if(adapters.Count == 0)
                {
                    throw new NotSupportedException("No adapters found");//if none are found, quit
                }
                IAdapter1 adapter=null;//initialise adapter variable
                if(args.Length>1)
                {
                    if(args.Length>=2 && args[0].ToLower()=="adapter")
                    {
                        adapter = await BlueZManager.GetAdapterAsync(args[1]);//use the adapter that has been specified
                    }
                }
                Console.WriteLine("No adapter provided. Using default one");
                adapter = adapters.First();//if not, use the first one found

    #endregion
                var adapterPath = adapter.ObjectPath.ToString();//get path
                var adapterName = adapterPath.Substring(adapterPath.LastIndexOf("/") + 1);//get name
                
                Console.WriteLine($"Using Bluetooth adapter {adapterName}");

                var devices = await adapter.GetDevicesAsync();
                foreach(var device in devices)//iterate trough each device
                {
                    string deviceDescription = await device.GetAliasAsync();
                    Console.WriteLine($"Detected the following devices:{deviceDescription}");
                }

                Console.WriteLine("Type name of device you want to use");
                string name = Console.ReadLine();

                foreach(var device in devices) // TODO: optimise
                {
                    string deviceDescription = await device.GetAliasAsync();
                    if(deviceDescription.Contains(name,StringComparison.OrdinalIgnoreCase))
                    {
                        return device;
                    }
                }
            }
            catch(System.NotSupportedException)
            {
                Console.WriteLine("Error, BlueZ couldn't start. Make sure you have the adapter plugged in and try again");
            }
            return null;
        }
        private static async Task adapter_PoweredOnAsync(Adapter adapter, BlueZEventArgs e)
        {
            try
            {
                if (e.IsStateChange)
                {
                Console.WriteLine("Bluetooth adapter powered on.");
                }
                else
                {
                Console.WriteLine("Bluetooth adapter already powered on.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
        private static async Task device_ConnectedAsync(Device device, BlueZEventArgs e)
        {
            try
            {
                if (e.IsStateChange)
                {
                Console.WriteLine($"Connected to {await device.GetAddressAsync()}");
                }
                else
                {
                Console.WriteLine($"Already connected to {await device.GetAddressAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
        private static async Task device_DisconnectedAsync(Device device, BlueZEventArgs e)
        {
        try
        {
            Console.WriteLine($"Disconnected from {await device.GetAddressAsync()}");

            await Task.Delay(TimeSpan.FromSeconds(15));

            Console.WriteLine($"Attempting to reconnect to {await device.GetAddressAsync()}...");
            await device.ConnectAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
        }
        static async Task Main(string[] args)
        {
            if(!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                throw new System.PlatformNotSupportedException("This app only works on Linux");
            }
            if(args[0]== "setup")
            {
                var device = await GetiSudoDevice(args);
                Adapter adapter = new Adapter();
                adapter.PoweredOn += adapter_PoweredOnAsync;

                device.Connected += device_ConnectedAsync;
                device.Disconnected += device_DisconnectedAsync;
                //device.ServicesResolved += device_ServicesResolvedAsync;
            }
           

            Console.WriteLine("Hello World!");
        }
    }
}
