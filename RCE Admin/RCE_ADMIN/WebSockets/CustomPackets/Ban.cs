using Newtonsoft.Json;

namespace RCE_ADMIN.WebSockets.CustomPackets
{
    public class Ban
    {
        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("DisplayName2")]
        public string DisplayName2 { get; set; }

        [JsonProperty("Reason")]
        public string Reason { get; set; }
    }
}