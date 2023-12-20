using Newtonsoft.Json;

namespace RCE_ADMIN.WebSockets.CustomPackets
{
    public class Player
    {
        [JsonProperty("SteamID")]
        public string SteamId { get; set; }

        [JsonProperty("OwnerSteamID")]
        public string OwnerSteamId { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("Ping")]
        public int Ping { get; set; }

        [JsonProperty("Address")]
        public string Address { get; set; }

        [JsonProperty("ConnectedSeconds")]
        public int ConnectedSeconds { get; set; }

        [JsonProperty("VoiationLevel")]
        public float ViolationLevel { get; set; }

        [JsonProperty("CurrentLevel")]
        public float CurrentLevel { get; set; }

        [JsonProperty("UnspentXp")]
        public float UnspentXp { get; set; }

        [JsonProperty("Health")]
        public float Health { get; set; }
    }
}