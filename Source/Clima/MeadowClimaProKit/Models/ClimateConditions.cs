using MeadowClimaProKit.Database;
using System;

namespace MeadowClimaProKit.Models
{
    public class ClimateConditions
    {
        public ClimateReading New { get; set; }
        public ClimateReading Old { get; set; }

        public ClimateConditions(ClimateReading newClimate, ClimateReading oldClimate)
        {
            New = newClimate ?? throw new ArgumentNullException(nameof(newClimate));
            Old = oldClimate ?? throw new ArgumentNullException(nameof(oldClimate));
        }
    }
}