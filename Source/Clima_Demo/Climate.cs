using Meadow;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clima_Demo
{
    public static class Climate
    {
        public static (Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure, Resistance? GasResistance) Bme688 { get; set; }
        public static Voltage SolarVoltage { get; set; }
        public static Speed Anemometer { get; set; }
        public static Length RainGuage{ get; set; }
        public static Azimuth Windvane { get; set; }
        public static (Concentration? Concentration, Temperature? Temperature, RelativeHumidity? Humidity) Scd40 { get; set; }

    }
}
