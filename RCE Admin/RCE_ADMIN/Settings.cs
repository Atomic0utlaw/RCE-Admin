using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Newtonsoft.Json;

namespace RCE_ADMIN
{
    public class Settings
    {
        private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Settings/config.json");

        [JsonProperty("ServerAddress")]
        public string ServerAddress { get; set; }

        [JsonProperty("ServerPort")]
        public string ServerPort { get; set; }

        [JsonProperty("ServerPassword")]
        public string ServerPassword { get; set; }

        [JsonProperty("EventWebhookUrl")]
        public string EventWebhookUrl { get; set; }

        [JsonProperty("KillFeedWebhookUrl")]
        public string KillFeedWebhookUrl { get; set; }

        [JsonProperty("ChatWebhookUrl")]
        public string ChatWebhookUrl { get; set; }

        [JsonProperty("TeamWebhookUrl")]
        public string TeamWebhookUrl { get; set; }

        [JsonProperty("ItemWebhookUrl")]
        public string ItemWebhookUrl { get; set; }

        [JsonProperty("InGameName")]
        public string InGameName { get; set; }

        [JsonProperty("AutoMessages")]
        public bool AutoMessages { get; set; }

        [JsonProperty("AutoMessagesTime")]
        public int AutoMessagesTime { get; set; }

        [JsonProperty("InGameKillFeed")]
        public bool InGameKillFeed { get; set; }

        [JsonProperty("DiscordKillFeed")]
        public bool DiscordKillFeed { get; set; }

        [JsonProperty("InGameEventFeed")]
        public bool InGameEventFeed { get; set; }

        [JsonProperty("DiscordEventFeed")]
        public bool DiscordEventFeed { get; set; }

        [JsonProperty("InGameChat")]
        public bool InGameChat { get; set; }

        [JsonProperty("DiscordChat")]
        public bool DiscordChat { get; set; }

        [JsonProperty("Version")]
        [JsonIgnore]
        public static string Version = "v1.12";
        public Settings(string server_address, string server_port, string server_password, string events_webhook_url, string killfeed_webhook_url, string chat_webhook_url, string team_webhook_url, string item_webhook_url, string in_game_name, bool auto_messages, int auto_messages_time, bool in_game_kill_feed, bool discord_kill_feed, bool in_game_event_feed, bool discord_event_feed, bool in_game_chat, bool discord_chat)
        {
            ServerAddress = server_address;
            ServerPort = server_port;
            ServerPassword = server_password;
            EventWebhookUrl = events_webhook_url;
            KillFeedWebhookUrl = killfeed_webhook_url;
            ChatWebhookUrl = chat_webhook_url;
            TeamWebhookUrl = team_webhook_url;
            ItemWebhookUrl = item_webhook_url;
            InGameName = in_game_name;
            AutoMessages = auto_messages;
            AutoMessagesTime = auto_messages_time;
            InGameKillFeed = in_game_kill_feed;
            DiscordKillFeed = discord_kill_feed;
            InGameEventFeed = in_game_event_feed;
            DiscordEventFeed = discord_event_feed;
            InGameChat = in_game_chat;
            DiscordChat = discord_chat;
        }
        public static void Write(Settings settings)
        {
            Settings.Version = "v1.12";

            if (!File.Exists(FilePath))
                File.Create(FilePath).Close();

            File.WriteAllText(FilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));

            XtraMessageBox.Show("Successfully Saved Settings!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static Settings Read()
        {
            if (!File.Exists(FilePath))
            {
                XtraMessageBox.Show("Couldn't Find A Configuration File, A New One Will Be Created", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Write(new Settings("ip", "port", "password", null, null, null, null, null, null, true, 2, true, true, true, true, true, true));
            }
            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath));
            return settings;
        }
    }
}
