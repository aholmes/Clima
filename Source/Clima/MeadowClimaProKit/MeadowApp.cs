using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Web.Maple.Server;
using Meadow.Gateway.WiFi;
using MeadowClimaProKit.Connectivity;
using MeadowClimaProKit.Controller;
using MeadowClimaProKit.ServiceAccessLayer;
using System;
using System.Threading.Tasks;

namespace MeadowClimaProKit
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        public MeadowApp()
        {
        }

        public Task Initialize()
        {
            InitializeBluetooth();

            var mapleServer = InitializeMaple().Result;
            mapleServer.Start();

            return InitializeClimateMonitor();
        }

        void InitializeBluetooth()
        {
            //BluetoothServer.Instance.Initialize();
        }

        Task InitializeClimateMonitor()
        {
            return ClimateMonitorAgent.Instance.Initialize();
        }

        async Task<MapleServer> InitializeMaple()
        {
            Console.WriteLine("Initialize maple");
            LedController.Instance.SetColor(Color.Red);

            if (Device.WiFiAdapter == default) throw new Exception("WiFiAdapter is null.");

            var result = await Device.WiFiAdapter.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD).ConfigureAwait(false);
            if (result.ConnectionStatus != ConnectionStatus.Success)
            {
                throw new Exception($"Cannot connect to network: {result.ConnectionStatus}");
            }
            Device.WiFiAdapter.AutomaticallyReconnect = true;
            Device.WiFiAdapter.AutomaticallyStartNetwork = true;
            Console.WriteLine($"Wifi IP address: {Device.WiFiAdapter.IpAddress}");

            await DateTimeService.GetTimeAsync();

            var mapleServer = new MapleServer(Device.WiFiAdapter.IpAddress, 5417, false);

            LedController.Instance.SetColor(Color.Green);

            return mapleServer;
        }
    }
}