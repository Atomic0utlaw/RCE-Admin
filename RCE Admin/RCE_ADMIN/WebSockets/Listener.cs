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

namespace RCE_ADMIN.WebSockets
{
    public static class Listener
    {
        public static Dictionary<int, string> Listeners = new Dictionary<int, string>();
        public static List<string> NeedListener = new List<string>()
        {
            "playerlist",
            "banlist"
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
                    // Use a regular expression to extract information
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
            }
        }
    }
}
