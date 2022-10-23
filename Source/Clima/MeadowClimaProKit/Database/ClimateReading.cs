using System;
using System.Text.Json.Serialization;
using MU = Meadow.Units;

namespace MeadowClimaProKit.Database
{
    //[Table("ClimateReadings")]
    public class ClimateReading
    {
        //[PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public double TemperatureValue
        {
            get => Temperature?.Celsius ?? 0;
            set => Temperature = new MU.Temperature(value, MU.Temperature.UnitType.Celsius);
        }

        public double PressureValue
        {
            get => Pressure?.Bar ?? 0;
            set => Pressure = new MU.Pressure(value, MU.Pressure.UnitType.Bar);
        }

        public double HumidityValue
        {
            get => Humidity?.Percent ?? 0;
            set => Humidity = new MU.RelativeHumidity(value, MU.RelativeHumidity.UnitType.Percent);
        }

        public double RainFallValue
        {
            get => RainFall?.Millimeters ?? 0;
            set => RainFall = new MU.Length(value, MU.Length.UnitType.Millimeters);
        }

        public double WindDirectionValue
        {
            get => WindDirection?.DecimalDegrees ?? 0;
            set => WindDirection = new MU.Azimuth(value);
        }

        public double WindSpeedValue
        {
            get => WindSpeed?.KilometersPerHour ?? 0;
            set => WindSpeed = new MU.Speed(value, MU.Speed.UnitType.KilometersPerHour);
        }

        //[Indexed]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Whether or not this particular reading has been uploaded to the cloud.
        /// </summary>
        public bool Synchronized { get; set; }

        //[Ignore]
        [JsonIgnore]
        public MU.Temperature? Temperature { get; set; }
        //[Ignore]
        [JsonIgnore]
        public MU.Pressure? Pressure { get; set; }
        //[Ignore]
        [JsonIgnore]
        public MU.RelativeHumidity? Humidity { get; set; }
        //[Ignore]
        [JsonIgnore]
        public MU.Length? RainFall { get; set; }
        //[Ignore]
        [JsonIgnore]
        public MU.Azimuth? WindDirection { get; set; }
        //[Ignore]
        [JsonIgnore]
        public MU.Speed? WindSpeed { get; set; }
    }
}
