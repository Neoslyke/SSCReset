using Newtonsoft.Json;
using TShockAPI;

namespace SSCReset
{
    public class Configuration
    {
        [JsonProperty("RequireConfirmation")]
        public bool RequireConfirmation { get; set; } = true;

        [JsonProperty("LogDeletions")]
        public bool LogDeletions { get; set; } = true;

        [JsonProperty("BroadcastDeletions")]
        public bool BroadcastDeletions { get; set; } = false;

        [JsonProperty("AllowWipeOnline")]
        public bool AllowWipeOnline { get; set; } = false;

        [JsonProperty("AllowDeleteOnline")]
        public bool AllowDeleteOnline { get; set; } = false;

        public static Configuration GetDefault()
        {
            return new Configuration();
        }

        public static Configuration Load(string path)
        {
            try
            {
                return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(path)) ?? GetDefault();
            }
            catch
            {
                TShock.Log.ConsoleError("[SSCReset] Error loading configuration. Using defaults.");
                return GetDefault();
            }
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}