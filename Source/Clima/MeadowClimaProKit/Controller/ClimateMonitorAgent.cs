using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeadowClimaProKit.Models;
using Meadow;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Weather;
using MeadowClimaProKit.Database;
using Meadow.Hardware;
using System.Collections.Generic;
using System.Text;

namespace MeadowClimaProKit
{
    /// <summary>
    /// Basically combines all the sensors into one and enables the whole system
    /// to be read at once. Then it can go to sleep in between.
    ///
    /// ## Design considerations:
    ///
    /// we can probably get rid of the StartUpdating and StopUppating stuff
    /// in favor of managing the lifecycle elsewhere for sleep purposes. but we may
    /// not need to, depending on how we design the sleep APIs
    ///
    /// </summary>
    public class ClimateMonitorAgent
    {
        private static TimeSpan UPDATE_WAIT_TIMESPAN = TimeSpan.FromSeconds(10);

        private static readonly Lazy<ClimateMonitorAgent> instance =
            new Lazy<ClimateMonitorAgent>(() => new ClimateMonitorAgent());
        public static ClimateMonitorAgent Instance => instance.Value;

        public event EventHandler<ClimateConditions> ClimateConditionsUpdated = delegate { };

        IF7FeatherMeadowDevice Device => MeadowApp.Device;
        object samplingLock = new object();
        CancellationTokenSource? SamplingTokenSource;
        bool IsSampling = false;

        Bme680? bme680;
        //Bme280? bme280;
        WindVane? windVane;
        SwitchingAnemometer? anemometer;
        SwitchingRainGauge? rainGauge;

        private ClimateMonitorAgent() { }

        public Task Initialize()
        {
            var i2c = Device.CreateI2cBus();
            try
            {
                bme680 = new Bme680(i2c, (byte)Bme680.Addresses.Address_0x76);
                Console.WriteLine("Bme680 successully initialized.");
                var bmeObserver = Bme680.CreateObserver(
                    handler: result => Console.WriteLine($"Temp: {result.New.Temperature.Value.Fahrenheit:n2}, Humidity: {result.New.Humidity.Value.Percent:n2}%"),
                    filter: result => true);
                bme680.Subscribe(bmeObserver);
            }
            catch(Exception e)
            {
                bme680 = null;
                Console.WriteLine($"Bme680 failed bring-up: {e.Message}");
            }

            /*
            if(bme680 == null)
            {
                Console.WriteLine("Trying it as a BME280.");
                try
                {
                    bme280 = new Bme280(i2c, (byte)Bme280.Addresses.Default);
                    Console.WriteLine("Bme280 successully initialized.");
                    var bmeObserver = Bme280.CreateObserver(
                        handler: result => Console.WriteLine($"Temp: {result.New.Temperature.Value.Fahrenheit:n2}, Humidity: {result.New.Humidity.Value.Percent:n2}%"),
                        filter: result => true);
                    bme280.Subscribe(bmeObserver);
                }
                catch(Exception e2)
                {
                    Console.WriteLine($"Bme280 failed bring-up: {e2.Message}");
                }
            }
            */

            windVane = new WindVane(Device, Device.Pins.A00);
            Console.WriteLine("WindVane up.");

            anemometer = new SwitchingAnemometer(Device, Device.Pins.A01);
            anemometer.UpdateInterval = UPDATE_WAIT_TIMESPAN;
            anemometer.StartUpdating();
            Console.WriteLine("Anemometer up.");

            rainGauge = new SwitchingRainGauge(Device, Device.Pins.D15);
            Console.WriteLine("Rain gauge up.");

            return StartUpdating(UPDATE_WAIT_TIMESPAN);
        }

        Task StartUpdating(TimeSpan updateInterval)
        {
            lock(samplingLock)
            {
                if(IsSampling)
                    return Task.FromResult<bool>(false);

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                var ct = SamplingTokenSource.Token;

                var oldClimate = new ClimateReading();

                Console.WriteLine("Starting Climate Monitor Agent loop.");
                var updateTask = Task.Run(async () =>
                {
                    try
                    {
                        while(true)
                        {
                            Console.WriteLine("ClimateMonitorAgent: About to do a reading.");

                            if(ct.IsCancellationRequested)
                            {
                                // do task clean up here
                                //observers.ForEach(x => x.OnCompleted());
                                Console.WriteLine("Cancellation requested.");
                                break;
                            }

                            var climate = await Read();

                            // build a new result with the old and new conditions
                            var result = new ClimateConditions(climate, oldClimate);
                            oldClimate = climate;

                            Console.WriteLine("ClimateMonitorAgent: Reading complete.");
                            DatabaseManager.Instance.SaveReading(result.New);
                            Console.WriteLine("ClimateMonitorAgent: Saved reading.");

                            //ClimateConditionsUpdated.Invoke(this, result);

                            // sleep for the appropriate interval
                            await Task.Delay(UPDATE_WAIT_TIMESPAN, SamplingTokenSource.Token).ConfigureAwait(false);
                            //await Task.Delay(updateInterval, SamplingTokenSource.Token).ConfigureAwait(false);
                        }
                    }
                    catch(Exception e)
                    {
                        var sb = new StringBuilder($"Error in main read loop:{Environment.NewLine}");
                        sb.AppendLine(e.Message);
                        sb.AppendLine(e.StackTrace);
                        Console.WriteLine(sb);
                    }
                    finally
                    {
                        IsSampling = false;
                    }
                }, SamplingTokenSource.Token);
                Console.WriteLine("Climate Monitor Agent read loop started.");
                return updateTask;
            }
        }

        void StopUpdating()
        {
            if(!IsSampling) return;

            lock(samplingLock)
            {
                SamplingTokenSource?.Cancel();

                IsSampling = false;
            }
        }

        public async Task<ClimateReading> Read()
        {
            //==== create the read tasks
            var bme680Task = bme680?.Read();
            //var bme280Task = bme280?.Read();
            var windVaneTask = windVane?.Read();
            //var rainFallTask = rainGauge?.Read();

            /*
            var tasks = new Dictionary<string, Task?>
            {
                { "BME680", bme680Task },
                { "BME280", bme280Task },
                { "Wind Vane", windVaneTask },
                //{ "Rain Gauge", rainFallTask }
            };

            var nullTasks = tasks.Where(o => o.Value == default);
            if (nullTasks.Any())
            {
                Console.WriteLine($"Some sensor readings are null: {string.Join(',', nullTasks.Select(o => o.Key))}");
            }

            var nonNullTasks = tasks.Except(nullTasks).Select(o => o.Value);
            //==== await until all tasks complete 
            await Task.WhenAll(nonNullTasks);
            */

            await Task.WhenAll(bme680Task, windVaneTask);
            var climate = new ClimateReading()
            {
                DateTime = DateTime.Now,                
                //Temperature = (bme280Task?.Result ?? bme680Task?.Result)?.Temperature,
                //Pressure = (bme280Task?.Result ?? bme680Task?.Result)?.Pressure,
                //Humidity = (bme280Task?.Result ?? bme680Task?.Result)?.Humidity,
                Temperature = bme680Task?.Result.Temperature,
                Pressure = bme680Task?.Result.Pressure,
                Humidity = bme680Task?.Result.Humidity,
                //RainFall = rainFallTask?.Result,
                WindDirection = windVaneTask?.Result,
                WindSpeed = anemometer?.WindSpeed,
            };

            return climate;
        }
    }
}