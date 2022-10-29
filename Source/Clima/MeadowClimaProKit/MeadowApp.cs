using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Web.Maple;
using Meadow.Gateway.WiFi;
using Meadow.Hardware;
using MeadowClimaProKit.Connectivity;
using MeadowClimaProKit.Controller;
using MeadowClimaProKit.ServiceAccessLayer;
using System;
using System.Threading.Tasks;

namespace MeadowClimaProKit
{
    public class MeadowApp : App<F7FeatherV2>
    {
        public MeadowApp()
        {
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                Console.WriteLine(e.Exception);
            };
        }

        public override Task Initialize()
        {
            InitializeBluetooth();

            return InitializeMaple()
                .ContinueWith(t => InitializeClimateMonitor());

            //return Task.FromResult<object>(null);
            //return InitializeClimateMonitor();
        }

        void InitializeBluetooth()
        {
            //BluetoothServer.Instance.Initialize();
        }

        Task InitializeClimateMonitor()
        {
            return ClimateMonitorAgent.Instance.Initialize();
        }

        private async Task InitializeMaple()
        {
            Console.WriteLine("Initialize maple");
            /*LedController.Instance.SetColor(Color.Red);

            Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            if (Device.WiFiAdapter == default) throw new Exception("WiFiAdapter is null.");

            var result = Device.WiFiAdapter.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD).Result;
            if (result.ConnectionStatus != ConnectionStatus.Success)
            {
                throw new Exception($"Cannot connect to network: {result.ConnectionStatus}");
            }
            Device.WiFiAdapter.AutomaticallyReconnect = true;
            Device.WiFiAdapter.AutomaticallyStartNetwork = true;
            Console.WriteLine($"Wifi IP address: {Device.WiFiAdapter.IpAddress}");

            DateTimeService.GetTimeAsync().Wait();

            var mapleServer = new MapleServer(Device.WiFiAdapter.IpAddress, 5417, false);

            LedController.Instance.SetColor(Color.Green);

            return mapleServer;*/
            LedController.Instance.SetColor(Color.Red);

            var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            var connectionResult = await wifi.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD, TimeSpan.FromSeconds(45), ReconnectionType.Automatic);
            if (connectionResult.ConnectionStatus != ConnectionStatus.Success)
            {
                throw new Exception($"Cannot connect to network: {connectionResult.ConnectionStatus}");
            }
            Console.WriteLine($"Wifi IP address: {wifi.IpAddress}");

            await DateTimeService.GetTimeAsync();

            var mapleServer = new MapleServer(wifi.IpAddress, 5417, false);
            mapleServer.Start();

            LedController.Instance.SetColor(Color.Green);
        }
    }
}