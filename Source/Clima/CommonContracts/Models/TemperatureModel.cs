
using Newtonsoft.Json;

namespace CommonContracts.Models
{
    public class TemperatureModel
    {
        [JsonProperty("temperature")]
        public string Temperature { get; set; }
        [JsonProperty("dateTime")]
        public string DateTime { get; set; }
    }
}