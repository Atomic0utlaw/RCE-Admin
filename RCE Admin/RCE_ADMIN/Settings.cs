using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Newtonsoft.Json;

namespace RCE_ADMIN
{
    public class Settings
    {
        private static readonly string SettingsPath = Path.Combine(Environment.CurrentDirectory, "Settings");
        private static readonly string KitsPath = Path.Combine(Environment.CurrentDirectory, "Kits");
        private static readonly string EventsPath = Path.Combine(Environment.CurrentDirectory, "Events");
        private static readonly string ConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "Settings/config.json");

        [JsonProperty("ServerAddress")]
        public string ServerAddress { get; set; }

        [JsonProperty("ServerPort")]
        public string ServerPort { get; set; }

        [JsonProperty("ServerPassword")]
        public string ServerPassword { get; set; }

        [JsonProperty("EventWebhookUrl")]
        public static string EventWebhookUrl { get; set; }

        [JsonProperty("KillFeedWebhookUrl")]
        public static string KillFeedWebhookUrl { get; set; }

        [JsonProperty("ChatWebhookUrl")]
        public static string ChatWebhookUrl { get; set; }

        [JsonProperty("TeamWebhookUrl")]
        public static string TeamWebhookUrl { get; set; }

        [JsonProperty("ItemWebhookUrl")]
        public static string ItemWebhookUrl { get; set; }

        [JsonProperty("InGameName")]
        public string InGameName { get; set; }

        [JsonProperty("AutoMessages")]
        public bool AutoMessages { get; set; }

        [JsonProperty("AutoMessagesTime")]
        public int AutoMessagesTime { get; set; }

        [JsonProperty("InGameKillFeed")]
        public static bool InGameKillFeed { get; set; }

        [JsonProperty("DiscordKillFeed")]
        public static bool DiscordKillFeed { get; set; }

        [JsonProperty("InGameEventFeed")]
        public static bool InGameEventFeed { get; set; }

        [JsonProperty("DiscordEventFeed")]
        public static bool DiscordEventFeed { get; set; }

        [JsonProperty("InGameChat")]
        public static bool InGameChat { get; set; }

        [JsonProperty("DiscordChat")]
        public static bool DiscordChat { get; set; }

        [JsonProperty("Theme")]
        public static string Theme { get; set; }

        [JsonProperty("SQLType")]
        private static string _sqlType = "sqlite";
        public static string SQLType
        {
            get { return _sqlType; }
            set { _sqlType = value; }
        }

        [JsonProperty("MySQLHost")]
        public static string MySQLHost { get; set; }

        [JsonProperty("MySQLUsername")]
        public static string MySQLUsername { get; set; }

        [JsonProperty("MySQLPassword")]
        public static string MySQLPassword { get; set; }

        [JsonProperty("MySQLPort")]
        public static string MySQLPort { get; set; }

        [JsonProperty("MySQLDatabaseName")]
        public static string MySQLDatabaseName { get; set; }

        [JsonProperty("Version")]
        [JsonIgnore]
        public static string Version = "v1.19";
        public Settings(string server_address, string server_port, string server_password, string events_webhook_url, string killfeed_webhook_url, string chat_webhook_url, string team_webhook_url, string item_webhook_url, string in_game_name, bool auto_messages, int auto_messages_time, bool in_game_kill_feed, bool discord_kill_feed, bool in_game_event_feed, bool discord_event_feed, bool in_game_chat, bool discord_chat, string theme, string sql_type, string sql_host, string sql_port, string sql_usermame, string sql_password, string sql_dbname)
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
            Theme = theme;
            SQLType = sql_type;
            MySQLHost = sql_host;
            MySQLUsername = sql_usermame;
            MySQLPassword = sql_password;
            MySQLPort = sql_port;
            MySQLDatabaseName = sql_dbname;
        }
        public static void Write(Settings settings)
        {
            Settings.Version = "v1.19";

            if (!File.Exists(ConfigFile))
                File.Create(ConfigFile).Close();

            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
            XtraMessageBox.Show("Successfully Saved Settings!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static Settings Read()
        {
            if (!Directory.Exists(SettingsPath))
            {
                Directory.CreateDirectory(SettingsPath);
            }
            if (!Directory.Exists(KitsPath))
            {
                Directory.CreateDirectory(KitsPath);
            }
            if (!Directory.Exists(EventsPath))
            {
                Directory.CreateDirectory(EventsPath);
            }
            if (!File.Exists(ConfigFile))
            {
                XtraMessageBox.Show("Couldn't Find A Configuration File, A New One Will Be Created", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Write(new Settings("ip", "port", "password", null, null, null, null, null, null, true, 2, true, true, true, true, true, true, "#cc402a", "sqlite", null, "3306", null, null, null));
            }
            Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(ConfigFile));
            if (string.IsNullOrEmpty(Settings.SQLType))
            {
                Settings newSettings = new Settings("ip", "port", "password", null, null, null, null, null, null, true, 2, true, true, true, true, true, true, "#cc402a", "sqlite", null, "3306", null, null, null);
                XtraMessageBox.Show("Your Config Has Been Reset Due To Being Outdated, Please Enter All The Information Again!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Write(newSettings);
                return newSettings;
            }

            return settings;
        }
    }
}
