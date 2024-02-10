using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RCE_ADMIN.Callbacks;
using RCE_ADMIN.Interface;
using WebSocketSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace RCE_ADMIN.WebSockets
{
    public static class Listener
    {
        public static Dictionary<int, string> Listeners = new Dictionary<int, string>();

        public static List<string> NeedListener = new List<string>()
        {
            "playerlist",
            "banlist",
            "find_entity bradley",
            "find_entity heli"
        };
        static JArray ConvertTextToJsonArray(string inputText)
        {
            JArray jsonArray = new JArray();
            string[] lines = inputText.Split('\n');

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    Match match = Regex.Match(line, @"\[(\d+)\] User \[([^\]]+)\] Expiry \[([^\]]+)\]");
                    if (match.Success)
                    {
                        int id = int.Parse(match.Groups[1].Value);
                        string user = match.Groups[2].Value;
                        var entry = new
                        {
                            ID = id,
                            DisplayName = user
                        };

                        jsonArray.Add(JToken.FromObject(entry));
                    }
                }
            }

            return jsonArray;
        }
        [STAThread]
        public static void ProcessMessage(Packet packet)
        {
            var command = Listeners[packet.Identifier];
            Listeners.Remove(packet.Identifier);

            switch (command.ToLower())
            {
                case "playerlist":
                    PlayerList.UpdatePlayers(packet.Message);
                    break;
                case "banlist":
                    JArray jsonArray = ConvertTextToJsonArray(packet.Message);
                    string ban_list = jsonArray.ToString(Formatting.Indented);
                    BanList.UpdateBans(ban_list);
                    break;
                case "find_entity bradley":
                    if (packet.Message.Contains("servergibs_bradley"))
                    {
                        if (!string.IsNullOrEmpty(Settings.EventWebhookUrl) && Settings.DiscordEventFeed)
                        {
                            WebSocketsWrapper.SendEmbedToWebhook(Settings.EventWebhookUrl, "Bradley APC", "Somebody Has Just Destroyed **Bradley APC**!", null, "https://rustlabs.com/img/screenshots/bradleyapc.png");
                        }
                        if (Settings.InGameEventFeed)
                        {
                            WebSocketsWrapper.SendCommand("global.say <color=green>[EVENT]</color> Somebody Has Just Destroyed <b><color=orange>Bradley APC</color></b>!");
                        }
                        WebSocketsWrapper.SendCommand("entity.deleteentity servergibs_bradley 0");
                    }
                    break;
                case "find_entity heli":
                    if (packet.Message.Contains("servergibs_patrolhelicopter"))
                    {
                        if (!string.IsNullOrEmpty(Settings.EventWebhookUrl) && Settings.DiscordEventFeed)
                        {
                            WebSocketsWrapper.SendEmbedToWebhook(Settings.EventWebhookUrl, "Patrol Helicopter", "Somebody Has Just Dropped The **Patrol Helicopter**!", null, "https://rustlabs.com/img/screenshots/helicopter.png");
                        }
                        if (Settings.InGameEventFeed)
                        {
                            WebSocketsWrapper.SendCommand("global.say <color=green>[EVENT]</color> Somebody Has Just Dropped The <b><color=orange>Patrol Helicopter</color></b>!");
                        }
                        WebSocketsWrapper.SendCommand("entity.deleteentity servergibs_patrolhelicopter 0");
                    }
                    break;
            }
        }
    }
}
