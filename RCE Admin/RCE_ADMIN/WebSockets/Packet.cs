using Newtonsoft.Json;

namespace RCE_ADMIN.WebSockets
{
    public class Packet
    {
        [JsonProperty("Identifier")]
        public int Identifier { get; set; }
        [JsonProperty("Message")]
        public string Message { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        public Packet(string message, int identifier, string name = "RCE_ADMIN")
        {
            Identifier = identifier;
            Message = message;
            Name = name;
        }
    }
}
