﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Web.Maple;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Clima_Demo
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        IClimaHardware clima;

        public override Task Initialize()
        {
            Resolver.Log.LogLevel = Meadow.Logging.LogLevel.Trace;

            Resolver.Log.Info("Initialize hardware...");

            var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            wifi.NetworkConnected += (sender, args) =>
            {
                var mapleServer = new MapleServer(sender.IpAddress, 5417, true, logger: Resolver.Log);
                mapleServer.Start();
            };
            wifi.ConnectToDefaultAccessPoint();

            clima = Clima.Create();
            Resolver.Log.Info($"Running on Clima Hardware {clima.RevisionString}");

            if (clima.AtmosphericSensor is { } bme688)
            {
                bme688.Updated += Bme688Updated;
            }

            if (clima.EnvironmentalSensor is { } scd40)
            {
                scd40.Updated += Scd40Updated;
            }

            if (clima.WindVane is { } windVane)
            {
                windVane.Updated += WindvaneUpdated;
            }

            if (clima.RainGauge is { } rainGuage)
            {
                rainGuage.Updated += RainGuageUpdated;
            }

            if (clima.Anemometer is { } anemometer)
            {
                anemometer.WindSpeedUpdated += AnemometerUpdated;
            }

            if (clima.SolarVoltageInput is { } solarVoltage)
            {
                solarVoltage.Updated += SolarVoltageUpdated;
            }

            if (clima.Gnss is { } gnss)
            {
                //gnss.GsaReceived += GnssGsaReceived;
                //gnss.GsvReceived += GnssGsvReceived;
                //gnss.VtgReceived += GnssVtgReceived;
                gnss.RmcReceived += GnssRmcReceived;
                gnss.GllReceived += GnssGllReceived;
            }

            Resolver.Log.Info("Initialization complete");

            clima.ColorLed.SetColor(Color.Green);

            return base.Initialize();
        }

        private void GnssGsaReceived(object sender, ActiveSatellites e)
        {
            if (e.SatellitesUsedForFix is { } sats)
            {
                Resolver.Log.Info($"Number of active satellites: {sats.Length}");
            }
        }

        private void GnssGsvReceived(object sender, SatellitesInView e)
        {
            Resolver.Log.Info($"Satellites in view: {e.Satellites.Length}");
        }

        private void GnssVtgReceived(object sender, CourseOverGround e)
        {
            if (e is { } cv)
            {
                Resolver.Log.Info($"{cv}");
            };
        }

        private void GnssRmcReceived(object sender, GnssPositionInfo e)
        {
            if (e.Valid)
            {
                Resolver.Log.Info($"GNSS Position: lat: [{e.Position.Latitude}], long: [{e.Position.Longitude}]");
            }
        }

        private void GnssGllReceived(object sender, GnssPositionInfo e)
        {
            if (e.Valid)
            {
                Resolver.Log.Info($"GNSS Position: lat: [{e.Position.Latitude}], long: [{e.Position.Longitude}]");
            }
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            var updateInterval = TimeSpan.FromSeconds(5);

            if (clima.AtmosphericSensor is { } bme688)
            {
                bme688.StartUpdating(updateInterval);
            }

            if (clima.EnvironmentalSensor is { } scd40)
            {
                scd40.StartUpdating(updateInterval);
            }

            if (clima.WindVane is { } windVane)
            {
                windVane.StartUpdating(updateInterval);
            }

            if (clima.RainGauge is { } rainGuage)
            {
                rainGuage.StartUpdating(updateInterval);
            }

            if (clima.Anemometer is { } anemometer)
            {
                anemometer.StartUpdating(updateInterval);
            }

            if (clima.SolarVoltageInput is { } solarVoltage)
            {
                solarVoltage.StartUpdating(updateInterval);
            }

            if (clima.Gnss is { } gnss)
            {
                gnss.StartUpdating();
            }

            return base.Run();
        }

        private void Bme688Updated(object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance)> e)
        {
            Climate.Bme688 = e.New;
            Resolver.Log.Info($"BME688:        {(int)e.New.Temperature?.Celsius:0.0}C, {(int)e.New.Humidity?.Percent:0.#}%, {(int)e.New.Pressure?.Millibar:0.#}mbar");
        }

        private void SolarVoltageUpdated(object sender, IChangeResult<Voltage> e)
        {
            Climate.SolarVoltage = e.New;
            Resolver.Log.Info($"Solar Voltage: {e.New.Volts:0.#} volts");
        }

        private void AnemometerUpdated(object sender, IChangeResult<Speed> e)
        {
            Climate.Anemometer = e.New;
            Resolver.Log.Info($"Anemometer:    {e.New.MetersPerSecond:0.#} m/s");
        }

        private void RainGuageUpdated(object sender, IChangeResult<Length> e)
        {
            Climate.RainGuage = e.New;
            Resolver.Log.Info($"Rain Gauge:    {e.New.Millimeters:0.#} mm");
        }

        private void WindvaneUpdated(object sender, IChangeResult<Azimuth> e)
        {
            Climate.Windvane = e.New;
            Resolver.Log.Info($"Wind Vane:     {e.New.Compass16PointCardinalName} ({e.New.Radians:0.#} radians)");
        }

        private void Scd40Updated(object sender, IChangeResult<(Concentration? Concentration, Temperature? Temperature, RelativeHumidity? Humidity)> e)
        {
            Climate.Scd40 = e.New;
            Resolver.Log.Info($"SCD40:         {e.New.Concentration.Value.PartsPerMillion:0.#}ppm, {e.New.Temperature.Value.Celsius:0.0}C, {e.New.Humidity.Value.Percent:0.0}%");
        }
    }
}