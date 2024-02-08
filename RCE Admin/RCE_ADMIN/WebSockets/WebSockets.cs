using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using WebSocketSharp;
using RCE_ADMIN.Interface;
using RCE_ADMIN.Threading;
using DevExpress.XtraEditors;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using System.Net;
using System.Collections.Specialized;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Linq;
using RCE_ADMIN.Callbacks;
using DevExpress.Utils.Text;
using System.IO;
using static RCE_ADMIN.Form1;

namespace RCE_ADMIN.WebSockets
{
    public static class WebSocketsWrapper
    {
        private static WebSocket webSocket;
        private static Random random;
        public static Settings Settings;
        public static void Connect()
        {
            if (IsConnected())
            {
                XtraMessageBox.Show("You're Already Connected!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ConnectStatus.SetText("Connecting...");
            webSocket = new WebSocket($"ws://{Form1.Settings.ServerAddress}:{Form1.Settings.ServerPort}/{Form1.Settings.ServerPassword}");
            webSocket.OnOpen += WebSocket_OnOpen;
            webSocket.OnMessage += WebSocket_OnMessage;
            webSocket.OnError += WebSocket_OnError;
            webSocket.OnClose += WebSocket_OnClose;
            webSocket.ConnectAsync();
        }
        public static void Disconnect()
        {
            if (!IsConnected())
            {
                XtraMessageBox.Show("You Aren't Connected!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ConnectStatus.SetText("Disconnecting...");
            webSocket.CloseAsync();
        }
        public static string Send(string message, int identifier = 1)
        {
            if (!IsConnected())
            {
                XtraMessageBox.Show("You Aren't Connected!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
            var packetObj = new Packet(message, identifier);
            var packetStr = JsonConvert.SerializeObject(packetObj);
            webSocket.SendAsync(packetStr, null);
            ServerConsole.AddNewEntry(packetObj.Message);
            return packetObj.Message;
        }
        public static void SendCommand(string command)
        {
            var identifier = 1;
            if (Listener.NeedListener.Contains(command))
            {
                if (random == null)
                    random = new Random(DateTime.Now.Millisecond);
                identifier = random.Next(0, int.MaxValue);
                if (Listener.Listeners.ContainsKey(identifier))
                {
                    XtraMessageBox.Show("Duplicate Identifier Found!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Listener.Listeners.Add(identifier, command);
            }
            Send(command, identifier);
        }
        public static bool IsConnected()
        {
            if (webSocket == null)
                return false;
            return webSocket.ReadyState == WebSocketState.Open;
        }
        private static void WebSocket_OnOpen(object sender, EventArgs e)
        {
            ConnectStatus.SetText("Connected");
            ServerConsole.Enable();
            Update.StartThreads();
        }
        class Embed
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("color")]
            public int Color { get; set; }

            [JsonProperty("fields")]
            public List<Field> Fields { get; set; }
        }

        class Field
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public bool Inline { get; set; }
        }
        public static void SendEmbedToWebhook(string webhookUrl, string ititle, string content, Dictionary<string, string> fields, string image = "https://i.ibb.co/rZRvGDV/rust-logo.png")
        {
            try
            {
                Task.Run(async () =>
                {
                using (HttpClient httpClient = new HttpClient())
                {
                    string jsonPayload;
                    if (fields == null)
                    {
                        var payload = new
                        {
                            embeds = new[]
                            {
                            new
                            {
                                type = "rich",
                                color =  0xcd402a,
                                title = ititle,
                                description = content,
                                thumbnail = new { url = image },
                                timestamp = DateTime.Now,
                                footer = new { text =  "RCE Admin • " + Settings.Version },
                            }
                        }
                        };
                        jsonPayload = JsonConvert.SerializeObject(payload);
                    }
                        else
                        {
                            var payload = new
                            {
                                embeds = new[]
                                {
                            new
                            {
                                type = "rich",
                                color =  0xcd402a,
                                title = ititle,
                                description = content,
                                thumbnail = new { url = image },
                                timestamp = DateTime.Now,
                                footer = new { text =  "RCE Admin • " + Settings.Version },
                                fields = fields.Select(f => new { name = f.Key, value = f.Value, inline = true }).ToArray()
                            }
                        }
                            };
                            jsonPayload = JsonConvert.SerializeObject(payload);
                        }

                        using (var contentData = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
                        {
                            await httpClient.PostAsync(webhookUrl, contentData);
                        }
                    }
                }).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("SendEmbedToWebhook Error : " + ex.Message);
            }
        }
        static void SendDiscordWebhook(bool killfeed, string message)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            Settings = Settings.Read();
            using (var client = new WebClient())
            {
                var data = new NameValueCollection();
                data["content"] = string.Format("{0}{1}", message, Environment.NewLine);
                var response = client.UploadValues(killfeed ? Settings.KillFeedWebhookUrl : Settings.EventWebhookUrl, "POST", data);
                string responseText = Encoding.UTF8.GetString(response);
            }
        }
        static string getFirstFiveNumbers(string inputString)
        {
            string firstSixChars = inputString.Substring(0, Math.Min(inputString.Length, 5));
            Match regexResult = Regex.Match(firstSixChars, @"^(\d{5})");

            if (regexResult.Success)
            {
                return regexResult.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        static string GetFirstSixNumbers(string inputString)
        {
            string firstSixChars = inputString.Substring(0, Math.Min(inputString.Length, 6));
            Match regexResult = Regex.Match(firstSixChars, @"^(\d{6})");

            if (regexResult.Success)
            {
                return regexResult.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        static string GetFirstSevenNumbers(string inputString)
        {
            string firstSixChars = inputString.Substring(0, Math.Min(inputString.Length, 7));
            Match regexResult = Regex.Match(firstSixChars, @"^(\d{7})");

            if (regexResult.Success)
            {
                return regexResult.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        static string getFirstEightNumbers(string inputString)
        {
            string firstSixChars = inputString.Substring(0, Math.Min(inputString.Length, 8));
            Match regexResult = Regex.Match(firstSixChars, @"^(\d{8})");

            if (regexResult.Success)
            {
                return regexResult.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        static string ReplaceWholeNumbers(string inputString, string replacementChar)
        {
            string replacedString = Regex.Replace(inputString, @"\b\d+\b", replacementChar);
            replacedString = replacedString.Replace("sentry.scientist.static", "Sentry Turret")
                                           .Replace("patrolhelicopter", "Patrol Helicopter")
                                           .Replace("autoturret_deployed", "Auto Turret");
            return replacedString;
        }

        static Dictionary<string, string> replacements = new Dictionary<string, string>
        {
            { " was killed by ", "** Was Killed By **" },
            { "sentry.scientist.static", "Sentry Turret" },
            { "patrolhelicopter", "Patrol Helicopter" },
            { "autoturret_deployed", "An Auto Turret" },
            { "bradleyapc", "Bradley APC" },
            { "barricade", "A Metal Barricade" },
            { "wall", "A High External Wall" },
            { "ch47scientists", "Chinook" },
            { "bear", "A Bear" },
            { "boar", "A Pig" },
            { "wolf", "A Wolf" },
            { "stag", "A Deer" },
            { "fall", "Falling To Their Death" },
            { "radiation", "Radiation" },
            { "Radiation", "Radiation" },
            { "collision", "Suicide" },
            { "Collision", "Suicide" },
            { "guntrap", "Shotgun Trap" },
            { " died ", "** Died **" }
        };
        static string ReplaceDeathText(string input, Dictionary<string, string> replacements)
        {
            foreach (var replacement in replacements)
            {
                input = input.Replace(replacement.Key, replacement.Value);
            }
            return input;
        }
        static string RemoveColorTags(string input)
        {
            string pattern = @"<color\s*=\s*(?:#[0-9A-Fa-f]{6}|[A-Za-z]+)\s*>(.*?)</color>";
            return Regex.Replace(input, pattern, "$1");
        }
        static string[] ParseAndFilterKill(string input)
        {
            string pattern = @"(?<victim>[\w\s_-]+) was killed by (?<killer>[\w\s_-]+)";
            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                string victim = match.Groups["victim"].Value;
                string killer = match.Groups["killer"].Value;
                if (IsAllNumbers(victim))
                {
                    victim = "A Scientist";
                }
                return new string[] { victim, killer };
            }
            else
            {
                return new string[] { input, null };
            }
        }
        static string[] ParseDeath(string input)
        {
            string pattern = @"(\S+)\sdied\s\(([^)]+)\)";
            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                string victim = match.Groups[1].Value;
                string reason = match.Groups[2].Value;
                return new string[] { victim, reason };
            }
            else
            {
                return new string[] { input, null };
            }
        }

        static bool IsAllNumbers(string input)
        {
            return input.All(char.IsDigit);
        }
        public static double ConvertToDouble(string Value)
        {
            if (Value == null)
            {
                return 0;
            }
            else
            {
                double OutVal;
                double.TryParse(Value, out OutVal);

                if (double.IsNaN(OutVal) || double.IsInfinity(OutVal))
                {
                    return 0;
                }
                return OutVal;
            }
        }
        public static string GetItemImageUrl(string displayName)
        {
            string apiUrl = $"https://void-dev.co/item_image?display_name={Uri.EscapeDataString(displayName).Replace("%00", "").Replace("&00", "")}";
            try
            {
                using (WebClient client = new WebClient())
                {
                    string result = client.DownloadString(apiUrl);
                    return result;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("GetItemImageUrl Error: " + ex.Message);
                return "https://cdn.void-dev.co/eutopia.png";
            }
        }
        private static void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            var packet = JsonConvert.DeserializeObject<Packet>(e.Data);
            if (Listener.Listeners.ContainsKey(packet.Identifier))
            {
                Listener.ProcessMessage(packet);
                return;
            }
            if (packet.Identifier == -1 || string.IsNullOrEmpty(packet.Message))
                return;

            Settings = Settings.Read();


            //player joined
            string pinfo = packet.Message;
            if (pinfo.Contains("has entered the game"))
            {
               
            }
            //server info
            string sinfo = packet.Message;
            if (sinfo.Contains("Hostname"))
            {
                var server_info = JsonConvert.DeserializeAnonymousType(sinfo, new { 
                    Hostname = "", 
                    Players = 0,
                    Queued = 0,
                    Joining = 0,
                    GameTime = ""
                });
                Form1.ServerInfo.Hostname = RemoveColorTags(server_info.Hostname);
                Form1.ServerInfo.Players = server_info.Players;
                Form1.ServerInfo.Queued = server_info.Queued;
                Form1.ServerInfo.Joining = server_info.Joining;

                double totalSecondsInDay = 24 * 60 * 60;
                double totalSeconds = ConvertToDouble(server_info.GameTime) * totalSecondsInDay;
                DateTime midnight = DateTime.Today;
                DateTime full24HourTime = midnight.AddSeconds(totalSeconds);
                Form1.ServerInfo.GameTime = full24HourTime.ToString("HH:mm:ss");
            }

            //item logging
            string itemmsg = packet.Message;
            if (itemmsg.Contains("[ServerVar] giving"))
            {
                if (!string.IsNullOrEmpty(Settings.ItemWebhookUrl))
                {
                    string pattern = @"\[([^\]]+)\]\s+giving\s+([^\d]+)\s+(\d+)\s+x\s+([^\n]+)";
                    Match match = Regex.Match(itemmsg, pattern);

                    if (match.Success)
                    {
                        string serverVar = match.Groups[1].Value;
                        string player_name = match.Groups[2].Value;
                        string quantity = match.Groups[3].Value;
                        string item = match.Groups[4].Value; 
                        string imageUrl = GetItemImageUrl(item);
                        SendEmbedToWebhook(Settings.ItemWebhookUrl, "Item Recieved", string.Format("**{0}** Has Recieved ***{1}*** ({2})", player_name, item, int.Parse(quantity)), null, imageUrl);
                    }
                }
            }

            //team logging
            string teammsg = packet.Message;
            if (teammsg.Contains("team, ID"))
            {
                if (!string.IsNullOrEmpty(Settings.TeamWebhookUrl) && Settings.DiscordChat)
                {
                    if (teammsg.Contains("has joined"))
                    {
                        string pattern = @"\[([^\]]+)\] has joined \[([^\]]+)\]s team, ID: \[([^\]]+)\].";
                        Match match = Regex.Match(teammsg, pattern);
                        if (match.Success)
                        {
                            string playerName1 = match.Groups[1].Value;
                            string playerName2 = match.Groups[2].Value;
                            int teamId = int.Parse(match.Groups[3].Value);
                            if (playerName1 == playerName2)
                                SendEmbedToWebhook(Settings.TeamWebhookUrl, "Team Created", string.Format("**{0}** Created A Team ({1})", playerName1, teamId.ToString()), null, "https://cdn.void-dev.co/team_created.png");
                            else
                                SendEmbedToWebhook(Settings.TeamWebhookUrl, "Team Invite", string.Format("**{1}** Invited **{0}** To Their Team ({2})", playerName1, playerName2, teamId.ToString()), null, "https://cdn.void-dev.co/team_joined.png");
                        }
                    }
                    else if (teammsg.Contains("has left"))
                    {
                        string pattern = @"\[([^\]]+)\] has left \[([^\]]+)\]s team, ID: \[([^\]]+)\].";
                        Match match = Regex.Match(teammsg, pattern);
                        if (match.Success)
                        {
                            string playerName = match.Groups[1].Value;
                            string teamLeader = match.Groups[2].Value;
                            int teamId = int.Parse(match.Groups[3].Value);
                            if (playerName != teamLeader)
                                SendEmbedToWebhook(Settings.TeamWebhookUrl, "Team Leave", string.Format("**{0}** Left **{1}'s** Team ({2})", playerName, teamLeader, teamId.ToString()), null, "https://cdn.void-dev.co/team_left.png");
                            else
                                SendEmbedToWebhook(Settings.TeamWebhookUrl, "Team Deleted", string.Format("**{0}** Deleted Their Team ({1})", playerName, teamId.ToString()), null, "https://cdn.void-dev.co/team_deleted.png");
                        }
                    }
                }
            }

            //in game chat (via notes)
            string notemsg = packet.Message;
            if (notemsg.Contains("[NOTE PANEL]"))
            {
                string pattern = @"\[ (.*?) \] changed name from \[ (.*?) \] to \[ (.*?) \]";
                Match match = Regex.Match(notemsg.Replace("\n", "").Replace("\r", "").Replace("[NOTE PANEL] ", ""), pattern);
                if (match.Success)
                {
                    string username = match.Groups[1].Value;
                    string oldMessage = match.Groups[2].Value;
                    string newMessage = match.Groups[3].Value;
                    if (!string.Equals(oldMessage, newMessage) && !string.IsNullOrEmpty(newMessage) && !string.IsNullOrWhiteSpace(newMessage))
                    {
                        if (!string.IsNullOrEmpty(Settings.ChatWebhookUrl) && Settings.DiscordChat)
                        {
                            SendEmbedToWebhook(Settings.ChatWebhookUrl, "New Message", string.Format("**{0}** : {1}", username, newMessage), null, "https://cdn.void-dev.co/chat.png");
                        }
                        if (Settings.InGameChat)
                            SendCommand(string.Format("global.say <color=green>[CHAT]</color> <color=#153eff><b>{0}</b></color> : {1}", username, newMessage));

                    }
                }
            }

            //events logging (in game)
            string ievent_ = packet.Message;
            if (ievent_.Contains("[event]") && Settings.InGameEventFeed)
            {
                if (ievent_.Contains("Spawning assets/prefabs/npc/ch47/ch47scientists.entity.prefab for assets/bundled/prefabs/world/event_cargoheli.prefab"))
                {
                    SendCommand("global.say <color=green>[EVENT]</color> <b>Chinook</b> Is Looking For A Monument To Drop A Crate!");
                }
                else if (ievent_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic2.prefab for assets/bundled/prefabs/world/event_cargoship.prefab") || ievent_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic1.prefab for assets/bundled/prefabs/world/event_cargoship.prefab") || ievent_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic.prefab for assets/bundled/prefabs/world/event_cargoship.prefab"))
                {
                    SendCommand("global.say <color=green>[EVENT]</color> <b>Cargo Ship</b> Is Sailing The Seas Around The Island");
                }
                else if (ievent_.Contains("Spawning assets/prefabs/npc/cargo plane/cargo_plane.prefab for assets/bundled/prefabs/world/event_airdrop.prefab"))
                {
                    SendCommand("global.say <color=green>[EVENT]</color> An <b>Air Drop</b> Is Falling From The Sky, Can You Find It?");
                }
                else if (ievent_.Contains("Spawning assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab for assets/bundled/prefabs/world/event_helicopter.prefab"))
                {
                    SendCommand("global.say <color=green>[EVENT]</color> A <b>Patrol Helicopter</b> Is Circling The Map, Ready To Take It Down?");
                }
            }

            //kills logging (in game)
            string ikill_ = packet.Message;
            if (ikill_.Contains("killed") && !ikill_.Contains("cactus") && !ikill_.Contains("Collision") && Settings.InGameKillFeed)
            {
                string[] kill_info = ParseAndFilterKill(ikill_);
                if (kill_info[0] != "A Scientist")
                {
                    new PlayerDatabase().AddDeath(kill_info[0]);
                    if (IsAllNumbers(kill_info[1]))
                        kill_info[1] = "A Scientist";
                    else
                        new PlayerDatabase().AddKill(kill_info[1]);
                    SendCommand(string.Format("global.say <color=orange>[KILL]</color> <color=green><b>{0}</b></color> Was Killed By <color=red><b>{1}</b></color> And Now Has <color=orange><b>{2}</b></color> Death(s)!", kill_info[0], ReplaceDeathText(kill_info[1], replacements), new PlayerDatabase().GetDeathsByDisplayName(kill_info[0])));
                }
                else
                    SendCommand(string.Format("global.say <color=orange>[KILL]</color> <color=green><b>{0}</b></color> Was Killed By <color=red><b>{1}</b></color>", kill_info[0], kill_info[1]));
            }
            else if (ikill_.Contains("died"))
            {
                if (!ikill_.ToLower().Contains("generic") && !ikill_.ToLower().Contains("collision"))
                {
                    string[] death_info = ParseDeath(ikill_);
                    if (!death_info[1].ToLower().Contains("collision") || !death_info[1].ToLower().Contains("generic"))
                    {
                        new PlayerDatabase().AddDeath(death_info[0]);
                        SendCommand(string.Format("global.say <color=red>[DEATH]</color> <color=green><b>{0}</b></color> Died By <color=red><b>{1}</b></color> And Now Has <color=orange><b>{2}</b></color> Death(s)!", death_info[0], ReplaceDeathText(death_info[1], replacements), new PlayerDatabase().GetDeathsByDisplayName(death_info[0])));
                    }
                }
            }

            if (!string.IsNullOrEmpty(Settings.KillFeedWebhookUrl) && Settings.DiscordKillFeed) {
                //kills logging (discord)
                string kill_ = packet.Message;
                if (ikill_.Contains("killed") && !ikill_.Contains("cactus") && !ikill_.Contains("Collision"))
                {
                    string[] kill_info = ParseAndFilterKill(ikill_);
                    if (kill_info[0] != "A Scientist")
                    {
                        if (IsAllNumbers(kill_info[1]))
                        {
                            try
                            {
                                kill_info[1] = "A Scientist";
                                string[] victim_stats = new PlayerDatabase().GetKillStatsByName(kill_info[0]);
                                double victim_ratio;
                                double.TryParse(victim_stats[2], out victim_ratio);
                                Dictionary<string, string> fields = new Dictionary<string, string>
                                {
                                    {
                                        "AI Type",
                                        string.Format("**{0}**", ReplaceDeathText(kill_info[1], replacements))
                                    },
                                    {
                                        "Victim",
                                        string.Format("Name : **{0}**\nKills : **{1}**\nDeaths : **{2}**\nK/D Ratio : **{3}**",
                                            kill_info[0],
                                            victim_stats[0],
                                            victim_stats[1],
                                            Math.Round(victim_ratio, 2)
                                        )
                                    },
                                };
                                SendEmbedToWebhook(Settings.KillFeedWebhookUrl, "New AI Death", "", fields, "https://cdn.void-dev.co/death.png");
                            }
                            catch (Exception E)
                            {
                                XtraMessageBox.Show("AI Death Error : " + E.Message);
                            }
                            
                        }
                        else
                        {
                            try
                            {
                                string[] killer_stats = new PlayerDatabase().GetKillStatsByName(kill_info[1]);
                                string[] victim_stats = new PlayerDatabase().GetKillStatsByName(kill_info[0]);
                                double killer_ratio;
                                double.TryParse(killer_stats[2], out killer_ratio);
                                double victim_ratio;
                                double.TryParse(victim_stats[2], out victim_ratio);
                                if (replacements.Keys.Any(key => kill_info[1].Contains(key)))
                                {
                                    Dictionary<string, string> fields = new Dictionary<string, string>
                                    {
                                        {
                                            "Died By",
                                            string.Format("**{0}**", ReplaceDeathText(kill_info[1], replacements))
                                        },
                                        {
                                            "Victim",
                                            string.Format("Name : **{0}**\nKills : **{1}**\nDeaths : **{2}**\nK/D Ratio : **{3}**",
                                                kill_info[0],
                                                victim_stats[0],
                                                victim_stats[1],
                                                Math.Round(victim_ratio, 2)
                                            )
                                        },
                                    };
                                    SendEmbedToWebhook(Settings.KillFeedWebhookUrl, "New Death", "", fields, "https://cdn.void-dev.co/death.png");
                                }
                                else
                                {

                                    Dictionary<string, string> fields = new Dictionary<string, string>
                                    {
                                        {
                                            "Killer",
                                            string.Format("Name : **{0}**\nKills : **{1}**\nDeaths : **{2}**\nK/D Ratio : **{3}**",
                                                kill_info[1],
                                                killer_stats[0],
                                                killer_stats[1],
                                                Math.Round(killer_ratio, 2)
                                            )
                                        },
                                        {
                                            "Victim",
                                            string.Format("Name : **{0}**\nKills : **{1}**\nDeaths : **{2}**\nK/D Ratio : **{3}**",
                                                kill_info[0],
                                                victim_stats[0],
                                                victim_stats[1],
                                                Math.Round(victim_ratio, 2)
                                            )
                                        },
                                    };
                                    SendEmbedToWebhook(Settings.KillFeedWebhookUrl, "New Kill", "", fields, "https://cdn.void-dev.co/death.png");
                                }
                            }
                            catch (Exception E)
                            {
                                XtraMessageBox.Show("Kill Error : " + E.ToString());
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            string[] killer_stats = new PlayerDatabase().GetKillStatsByName(kill_info[1]);
                            double killer_ratio;
                            double.TryParse(killer_stats[2], out killer_ratio);
                            Dictionary<string, string> fields = new Dictionary<string, string>
                            {
                                {
                                    "Killer",
                                    string.Format("Name : **{0}**\nKills : **{1}**\nDeaths : **{2}**\nK/D Ratio : **{3}**",
                                        kill_info[1],
                                        killer_stats[0],
                                        killer_stats[1],
                                        Math.Round(killer_ratio, 2)
                                    )
                                },
                                {
                                    "AI Type",
                                    string.Format("**{0}**",
                                        ReplaceDeathText(kill_info[0], replacements)
                                    )
                                },
                            };
                            SendEmbedToWebhook(Settings.KillFeedWebhookUrl, "New AI Kill", "", fields, "https://cdn.void-dev.co/death.png");
                        }
                        catch (Exception E)
                        {
                            XtraMessageBox.Show("AI Kill Error : " + E.Message);
                        }
                    }
                }
                else if (kill_.Contains("died"))
                {
                    if (!kill_.Contains("died (Generic)") || !kill_.Contains("died (Collision)"))
                    {
                        string[] death_info = ParseDeath(ikill_);
                        if (!death_info[1].ToLower().Contains("collision") || !death_info[1].ToLower().Contains("generic"))
                        {
                            string[] death_stats = new PlayerDatabase().GetKillStatsByName(death_info[0]);
                            double victim_ratio;
                            double.TryParse(death_stats[2], out victim_ratio);
                            Dictionary<string, string> fields = new Dictionary<string, string>
                            {
                                {
                                    "Died By",  
                                    string.Format("**{0}**", ReplaceDeathText(death_info[1], replacements)) 
                                },
                                {
                                    "Victim",
                                    string.Format("Name : **{0}**\nKills : **{1}**\nDeaths : **{2}**\nK/D Ratio : **{3}**",
                                        death_info[0],
                                        death_stats[0],
                                        death_stats[1],
                                        Math.Round(victim_ratio, 2)
                                    )
                                },
                            };
                            SendEmbedToWebhook(Settings.KillFeedWebhookUrl, "New Death", "", fields, "https://cdn.void-dev.co/death.png");
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(Settings.EventWebhookUrl) && Settings.DiscordEventFeed)
            {
                //events logging (discord)
                string event_ = packet.Message;
                if (event_.Contains("[event]"))
                {
                    if (event_.Contains("Spawning assets/prefabs/npc/ch47/ch47scientists.entity.prefab for assets/bundled/prefabs/world/event_cargoheli.prefab"))
                    {
                        SendEmbedToWebhook(Settings.EventWebhookUrl, "Chinook", "**Chinook** Is Looking For A Monument To Drop A Crate!", null, "https://rustlabs.com/img/screenshots/codelockedhackablecrate.png");
                    }
                    else if (event_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic2.prefab for assets/bundled/prefabs/world/event_cargoship.prefab") || event_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic1.prefab for assets/bundled/prefabs/world/event_cargoship.prefab") || event_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic.prefab for assets/bundled/prefabs/world/event_cargoship.prefab"))
                    {
                        SendEmbedToWebhook(Settings.EventWebhookUrl, "Cargo", "**Cargo Ship** Is Sailing The Seas Around The Island!", null, "https://rustlabs.com/img/screenshots/cargo-ship-scientist.png");
                    }
                    else if (event_.Contains("Spawning assets/prefabs/npc/cargo plane/cargo_plane.prefab for assets/bundled/prefabs/world/event_airdrop.prefab"))
                    {
                        SendEmbedToWebhook(Settings.EventWebhookUrl, "Air Drop", "An **Air Drop** Is Falling From The Sky, Can You Find It?", null, "https://rustlabs.com/img/screenshots/supply-drop.png");
                    }
                    else if (event_.Contains("Spawning assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab for assets/bundled/prefabs/world/event_helicopter.prefab"))
                    {
                        SendEmbedToWebhook(Settings.EventWebhookUrl, "Patrol Helicopter", "A **Patrol Helicopter** Is Circling The Map, Ready To Take It Down?", null, "https://rustlabs.com/img/screenshots/helicopter.png");
                    }
                }
            }
            ServerConsole.AddNewEntry(packet.Message);
        }
        private static void WebSocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            ServerConsole.AddNewEntry($"An Error Occurred: {e.Message}");
        }
        private static void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            ConnectStatus.SetText("Disconnected");
            ServerConsole.Disable();
            PlayerCounter.Reset();
            Update.StopThreads();
        }
    }
}
