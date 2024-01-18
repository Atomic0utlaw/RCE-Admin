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
        public static void Send(string message, int identifier = 1)
        {
            if (!IsConnected())
            {
                XtraMessageBox.Show("You Aren't Connected!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var packetObj = new Packet(message, identifier);
            var packetStr = JsonConvert.SerializeObject(packetObj);
            webSocket.SendAsync(packetStr, null);
            ServerConsole.AddNewEntry($"{packetObj.Message}");
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
        static void SendDiscordWebhook(bool killfeed, string message)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            Settings = Settings.Read();
            using (var client = new WebClient())
            {
                var data = new NameValueCollection();
                data["content"] = string.Format("{0}{1}Feeds Provided By [**RCE Admin**](https://github.com/KyleFardy/RCE-Admin/)", message, Environment.NewLine);
                var response = client.UploadValues(killfeed ? Settings.KillFeedWebhookUrl : Settings.EventWebhookUrl, "POST", data);
                string responseText = Encoding.UTF8.GetString(response);
            }
        }
        static void SendDiscordWebhookChat(string message)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            Settings = Settings.Read();
            using (var client = new WebClient())
            {
                var data = new NameValueCollection();
                data["content"] = message;
                var response = client.UploadValues(Settings.ChatWebhookUrl, "POST", data);
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
            { "autoturret_deployed", "Auto Turret" },
            { " died ", "** Died **" }
        };
        static string ReplaceText(string input, Dictionary<string, string> replacements)
        {
            foreach (var replacement in replacements)
            {
                input = input.Replace(replacement.Key, replacement.Value);
            }
            return input;
        }
        static string RemoveColorTags(string input)
        {
            string pattern = @"<color\s*=\s*#?[0-9A-Fa-f]{6}|[A-Za-z]+>";
            string result = Regex.Replace(input, pattern, string.Empty);
            return result.Replace("</color>", "").Replace("</>", "").Replace("</ ", " ").Replace(">", "");
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
                        if (!string.IsNullOrEmpty(Settings.ChatWebhookUrl))
                        {
                            SendDiscordWebhookChat(string.Format("**{0}** : {1}", username, newMessage));
                        }
                        SendCommand(string.Format("global.say <color=green>[CHAT]</color> <color=#153eff>{0}</color> : {1}", username, newMessage));

                    }
                    else
                    {
                        ServerConsole.AddNewEntry("No match found or old and new names are the same.");
                    }
                }
                else
                {
                    ServerConsole.AddNewEntry("No match found.");
                }
            }

            //events logging (in game)
            string ievent_ = packet.Message;
            if (ievent_.Contains("[event]"))
            {
                if (ievent_.Contains("Spawning assets/prefabs/npc/ch47/ch47scientists.entity.prefab for assets/bundled/prefabs/world/event_cargoheli.prefab"))
                {
                    SendCommand("global.say <color=green>[EVENT]</color> Chinook Is Dropping A Crate");
                }
                else if (ievent_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic2.prefab for assets/bundled/prefabs/world/event_cargoship.prefab") || ievent_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic1.prefab for assets/bundled/prefabs/world/event_cargoship.prefab") || ievent_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic.prefab for assets/bundled/prefabs/world/event_cargoship.prefab"))
                {
                    SendCommand("global.say <color=green>[EVENT]</color> Cargo Is Inbound, Ready To Board?");
                }
                else if (ievent_.Contains("Spawning assets/prefabs/npc/cargo plane/cargo_plane.prefab for assets/bundled/prefabs/world/event_airdrop.prefab"))
                {
                    SendCommand("global.say <color=green>[EVENT]</color> An Air Drop Is Inbound, Keep An Eye Out!");
                }
                else if (ievent_.Contains("Spawning assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab for assets/bundled/prefabs/world/event_helicopter.prefab"))
                {
                    SendCommand("global.say <color=green>[EVENT]</color> Patrol Helicopter Is Inbound, Run!");
                }
            }

            //kills logging (in game)
            string ikill_ = packet.Message;
            if (ikill_.Contains("killed") && !ikill_.Contains("cactus"))
            {
                string result = GetFirstSixNumbers(ikill_);
                string result2 = GetFirstSevenNumbers(ikill_);
                string result3 = getFirstEightNumbers(ikill_);
                string result4 = getFirstFiveNumbers(ikill_);
                string k = ikill_.Replace(" was killed by ", " Was Killed By ");
                if (result != null)
                {
                    SendCommand("global.say <color=orange>[KILL]</color> " + ReplaceWholeNumbers(k, "A Scientist"));
                }
                else if (result2 != null)
                {
                    SendCommand("global.say <color=orange>[KILL]</color> " + ReplaceWholeNumbers(k, "A Scientist"));
                }
                else if (result3 != null)
                {
                    SendCommand("global.say <color=orange>[KILL]</color> " + ReplaceWholeNumbers(k, "A Scientist"));
                }
                else if (result4 != null)
                {
                    SendCommand("global.say <color=orange>[KILL]</color> " + ReplaceWholeNumbers(k, "A Scientist"));
                }
                else
                {
                    SendCommand("global.say <color=orange>[KILL]</color> " + ReplaceWholeNumbers(k, "A Scientist"));
                }
            }
            else if (ikill_.Contains("died"))
            {
                if (!ikill_.Contains("died (Generic)"))
                {
                    SendCommand("global.say <color=red>[DEATH]</color> " + ikill_.Replace(" died ", " Died"));
                }
            }

            if (!string.IsNullOrEmpty(Settings.KillFeedWebhookUrl)) {
                //kills logging (discord)
                string kill_ = packet.Message;
                if (kill_.Contains("killed") && !kill_.Contains("cactus"))
                {
                    SendDiscordWebhook(true, string.Format("**{0}**", ReplaceText(kill_, replacements)));
                }
                else if (kill_.Contains("died"))
                {
                    if (!kill_.Contains("died (Generic)"))
                    {
                        SendDiscordWebhook(true, string.Format("**{0}**", ReplaceText(kill_, replacements)));
                    }
                }
            }
            if (!string.IsNullOrEmpty(Settings.EventWebhookUrl))
            {
                //events logging (discord)
                string event_ = packet.Message;
                if (event_.Contains("[event]"))
                {
                    if (event_.Contains("Spawning assets/prefabs/npc/ch47/ch47scientists.entity.prefab for assets/bundled/prefabs/world/event_cargoheli.prefab"))
                    {
                        SendDiscordWebhook(false, "**Chinook** Is Dropping A Crate");
                    }
                    else if (event_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic2.prefab for assets/bundled/prefabs/world/event_cargoship.prefab") || event_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic1.prefab for assets/bundled/prefabs/world/event_cargoship.prefab") || event_.Contains("Spawning assets/content/vehicles/boats/cargoship/cargoshipdynamic.prefab for assets/bundled/prefabs/world/event_cargoship.prefab"))
                    {
                        SendDiscordWebhook(false, "**Cargo** Is Inbound!");
                    }
                    else if (event_.Contains("Spawning assets/prefabs/npc/cargo plane/cargo_plane.prefab for assets/bundled/prefabs/world/event_airdrop.prefab"))
                    {
                        SendDiscordWebhook(false, "An **Air Drop** Is Inbound!");
                    }
                    else if (event_.Contains("Spawning assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab for assets/bundled/prefabs/world/event_helicopter.prefab"))
                    {
                        SendDiscordWebhook(false, "**Patrol Helicopter** Is Inbound!");
                    }
                }
            }
            ServerConsole.AddNewEntry(packet.Message);
        }
        private static void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            XtraMessageBox.Show($"An Error Occurred:\n\n{e.Message}");
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
