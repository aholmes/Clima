
using Newtonsoft.Json;

namespace CommonContracts.Models
{
    public class ClimateModel
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("temperature")]
        public string Temperature { get; set; }

        [JsonProperty("pressure")]
        public string Pressure { get; set; }

        [JsonProperty("humdity")]
        public string Humidity { get; set; }

        [JsonProperty("rain")]
        public string Rain { get; set; }

        [JsonProperty("windspeed")]
        public string WindSpeed { get; set; }

        [JsonProperty("winddirection")]
        public string WindDirection { get; set; }
    }
}