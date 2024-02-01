using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress;
using RCE_ADMIN.Interface;
using RCE_ADMIN.WebSockets;
using System.Diagnostics;
using DevExpress.Utils;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using static DevExpress.XtraEditors.RoundedSkinPanel;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections.Generic;
using static RCE_ADMIN.Form1;
using System.Data.SQLite;
using Dapper;
using DevExpress.Utils.Extensions;
using System.Data;
using RCE_ADMIN.Callbacks;
using DiscordRPC;
using RCE_ADMIN.WebSockets.CustomPackets;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using WebSocketSharp;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using static DevExpress.XtraBars.Docking2010.Views.BaseRegistrator;
using Button = DiscordRPC.Button;
using System.Linq;
using static Dapper.SqlMapper;
using System.Media;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using DevExpress.LookAndFeel.Design;
using DevExpress.LookAndFeel.Helpers;
using DevExpress.XtraBars.Commands;
using DevExpress.XtraBars.Customization.Helpers;
using DevExpress.XtraBars.Ribbon;
using System.Reflection.Emit;
using System.Xml;

namespace RCE_ADMIN
{
    public partial class Form1 : XtraForm
    {
        public static Settings Settings;
        public static BarStaticItem Status;
        public static RichTextBox Console;
        public static BarStaticItem Counter;
        public static DataGridView Players;
        public static DataGridView AllPlayers;
        public static DataGridView Bans;
        public static CheckedListBoxControl CratePlayers;
        public static CheckedListBoxControl AnimalPlayers;
        public static XtraTabPage ConsoleTab;
        public static XtraTabPage PlayersTab;
        public static XtraTabPage EventsTab;
        public static XtraTabPage ServerSettingsTab;
        private DiscordRpcClient rpcClient;
        public static int selectedPlayer = -1;
        public Form1()
        {
            InitializeComponent();

            rpcClient = new DiscordRpcClient("1199514507932860528");
            rpcClient.SetPresence(new RichPresence
            {
                Details = "Console Admin Tool",
                State = "Using Version " + Settings.Version,
                Timestamps = new Timestamps(),
                Buttons = new Button[]
                {
                    new Button() { Label = "RCE Admin (Download)", Url = "https://github.com/KyleFardy/RCE-Admin/releases" }
                },
                Assets = new Assets
                {
                    LargeImageKey = "rce",
                    LargeImageText = "RCE Admin",
                }
            });
        }
        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(this, new Point(e.X, e.Y));
            }
        }
        private void DeleteCratePosition(string x, string y, string z)
        {
            if (lockedCrateGroupName.SelectedIndex >= 0)
            {
                string groupName = lockedCrateGroupName.SelectedItem.ToString();
                var existingGroup = crateGroups.Find(group => group.Name == groupName);
                if (existingGroup != null)
                {
                    var positionToRemove = existingGroup.Positions.Find(position => position.X == x && position.Y == y && position.Z == z);
                    if (positionToRemove != null)
                    {
                        existingGroup.Positions.Remove(positionToRemove);
                        string jsonContent = JsonConvert.SerializeObject(crateGroups, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText("Events/locked_crate_event.json", jsonContent);
                    }
                }
            }
        }
        static async Task<string[]> GetLatestReleaseChangelog()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "RCE Admin");
                var releaseResponse = await httpClient.GetStringAsync("https://api.github.com/repos/KyleFardy/RCE-Admin/releases/latest");
                var releaseData = JsonConvert.DeserializeObject<ReleaseData>(releaseResponse);
                return new string[] { releaseData.Version, releaseData.Body };
            }
        }
        public class ServerInfo
        {
            [JsonProperty("Hostname")]
            public static string Hostname { get; set; }

            [JsonProperty("Players")]
            public static int Players { get; set; }

            [JsonProperty("Queued")]
            public static int Queued { get; set; }

            [JsonProperty("Joining")]
            public static int Joining { get; set; }

            [JsonProperty("GameTime")]
            public static string GameTime { get; set; }
        }
        public class ReleaseData
        {
            [JsonProperty("body")]
            public string Body { get; set; }
            [JsonProperty("tag_name")]
            public string Version { get; set; }
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            Settings = Settings.Read();
            Status = toolStripStatusLabelRight;
            Console = richTextBoxConsole;
            ConsoleTab = xtraTabPage2;
            PlayersTab = xtraTabPage3;
            EventsTab = xtraTabPage6;
            ServerSettingsTab = xtraTabPage9;
            Counter = toolStripStatusLabelCounter;
            Players = dataGridViewPlayers;
            AllPlayers = allPlayersDataTable;
            Bans = bansDataTable;
            CratePlayers = checkedListBoxControl1;
            AnimalPlayers = checkedListBoxControl2;
            textBoxAddress.Text = Settings.ServerAddress;
            textBoxPort.Text = Settings.ServerPort;
            textBoxPassword.Text = Settings.ServerPassword;
            eventsWebhookUrl.Text = Settings.EventWebhookUrl;
            killfeedsWebhookUrl.Text = Settings.KillFeedWebhookUrl;
            chatWebhookUrl.Text = Settings.ChatWebhookUrl;
            teamWebhookUrl.Text = Settings.TeamWebhookUrl;
            itemWebhookUrl.Text = Settings.ItemWebhookUrl;
            inGameName.Text = Settings.InGameName;
            autoMessagesCheck.Checked = Settings.AutoMessages;
            AutoMessageTime.Value = Settings.AutoMessagesTime;
            InGameKillFeedCheck.Checked = Settings.InGameKillFeed;
            DiscordKillFeedCheck.Checked = Settings.DiscordKillFeed;
            InGameEventFeedCheck.Checked = Settings.InGameEventFeed;
            DiscordEventFeedCheck.Checked = Settings.DiscordEventFeed;
            InGameChatCheck.Checked = Settings.InGameChat;
            DiscordChatCheck.Checked = Settings.DiscordChat;
            CheckTheme(HexToColor(Settings.Theme));
            color_change(HexToColor(Settings.Theme));
            colorPickEdit1.EditValue = ColorTranslator.FromHtml(Settings.Theme);
            ServerConsole.Disable();
            string[] update_info = await GetLatestReleaseChangelog();
            richTextBoxChangelog.Text = update_info[1];
            curVer.Text = string.Format("Current Version : <br>{0}</b>", Settings.Version);
            latestVer.Text = string.Format("Latest Version : <br>{0}</b>", update_info[0]);
            LoadKit(1);
            LoadKit(2);
            LoadKit(3);
            load_players();
            LoadCrates();
            LoadMessages();
            rpcClient.Initialize();
            while (true)
            {
                await check_update();
                await Task.Delay(TimeSpan.FromMinutes(10));
            }

        }
        public string ColorToRGB(Color color)
        {
            return $"{color.R}, {color.G}, {color.B}".Replace(" ", "");
        }
        static string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        private void CheckTheme(Color colour)
        {
            string xmlFilePath = AppDomain.CurrentDomain.BaseDirectory + "//RCE Admin.exe.config";
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);
                XmlNode customPaletteNode = xmlDoc.SelectSingleNode("//CustomPaletteCollection/Skin/SvgPalette");
                if (customPaletteNode != null)
                {
                    XmlNode keyPaintNode = customPaletteNode.SelectSingleNode("SvgColor[@Name='Key Paint']");
                    if (keyPaintNode != null)
                    {
                        if (keyPaintNode.Attributes["Value"].Value.Replace(" ", "") != ColorToRGB(colour)) {
                            ChangeAccentPaintColor(colour);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Error: " + ex.Message);
            }
        }
        private void ChangeAccentPaintColor(Color newKeyPaintColor)
        {
            string xmlFilePath = AppDomain.CurrentDomain.BaseDirectory + "//RCE Admin.exe.config";

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);
                XmlNode customPaletteNode = xmlDoc.SelectSingleNode("//CustomPaletteCollection/Skin/SvgPalette");
                if (customPaletteNode != null)
                {
                    XmlNode keyPaintNode = customPaletteNode.SelectSingleNode("SvgColor[@Name='Key Paint']");
                    if (keyPaintNode != null)
                    {
                        keyPaintNode.Attributes["Value"].Value = ColorToRGB(newKeyPaintColor);
                    }
                    XmlNode paintShadowNode = customPaletteNode.SelectSingleNode("SvgColor[@Name='Paint Shadow']");
                    if (paintShadowNode != null)
                    {
                        paintShadowNode.Attributes["Value"].Value = ColorToRGB(newKeyPaintColor);
                    }
                    XmlNode paintDeepShadowNode = customPaletteNode.SelectSingleNode("SvgColor[@Name='Paint Deep Shadow']");
                    if (paintDeepShadowNode != null)
                    {
                        paintDeepShadowNode.Attributes["Value"].Value = ColorToRGB(newKeyPaintColor);
                    }
                    XmlNode accentPaintNode = customPaletteNode.SelectSingleNode("SvgColor[@Name='Accent Paint']");
                    if (accentPaintNode != null)
                    {
                        accentPaintNode.Attributes["Value"].Value = ColorToRGB(newKeyPaintColor);
                    }
                    XmlNode accentPaintLightNode = customPaletteNode.SelectSingleNode("SvgColor[@Name='Accent Paint Light']");
                    if (accentPaintLightNode != null)
                    {
                        accentPaintLightNode.Attributes["Value"].Value = ColorToRGB(newKeyPaintColor);
                    }
                    XmlNode accentBrushMajorNode = customPaletteNode.SelectSingleNode("SvgColor[@Name='Brush Major']");
                    if (accentBrushMajorNode != null)
                    {
                        accentBrushMajorNode.Attributes["Value"].Value = ColorToRGB(newKeyPaintColor);
                    }
                    XmlNode accentBrushMinorNode = customPaletteNode.SelectSingleNode("SvgColor[@Name='Brush Minor']");
                    if (accentBrushMinorNode != null)
                    {
                        accentBrushMinorNode.Attributes["Value"].Value = ColorToRGB(newKeyPaintColor);
                    }
                }
                xmlDoc.Save(xmlFilePath);
                XtraMessageBox.Show("New Theme Detected, Restarting To Apply New Theme!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RestartApplication();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Error: " + ex.Message);
            }
        }
        private void RestartApplication()
        {
            System.Diagnostics.Process.Start(Application.ExecutablePath);
            Application.Exit();
        }

        public void color_change(Color COL)
        {
            ActiveGlowColor = COL;
            InactiveGlowColor = COL;
            DefaultAppearance.BorderColor = COL;
            UserLookAndFeelDefault.Default.UseDefaultLookAndFeel = true;
            Players.AlternatingRowsDefaultCellStyle.SelectionBackColor = COL;
            Players.ColumnHeadersDefaultCellStyle.SelectionBackColor = COL;
            Players.DefaultCellStyle.SelectionBackColor = COL;
            Players.RowHeadersDefaultCellStyle.SelectionBackColor = COL;
            Players.RowsDefaultCellStyle.SelectionBackColor = COL;
            Players.RowTemplate.DefaultCellStyle.SelectionBackColor = COL;
            Players.GridColor = COL;
            AllPlayers.AlternatingRowsDefaultCellStyle.SelectionBackColor = COL;
            AllPlayers.ColumnHeadersDefaultCellStyle.SelectionBackColor = COL;
            AllPlayers.DefaultCellStyle.SelectionBackColor = COL;
            AllPlayers.RowHeadersDefaultCellStyle.SelectionBackColor = COL;
            AllPlayers.RowsDefaultCellStyle.SelectionBackColor = COL;
            AllPlayers.RowTemplate.DefaultCellStyle.SelectionBackColor = COL;
            AllPlayers.GridColor = COL;
            Bans.AlternatingRowsDefaultCellStyle.SelectionBackColor = COL;
            Bans.ColumnHeadersDefaultCellStyle.SelectionBackColor = COL;
            Bans.DefaultCellStyle.SelectionBackColor = COL;
            Bans.RowHeadersDefaultCellStyle.SelectionBackColor = COL;
            Bans.RowsDefaultCellStyle.SelectionBackColor = COL;
            Bans.RowTemplate.DefaultCellStyle.SelectionBackColor = COL;
            Bans.GridColor = COL;
            bar3.BarAppearance.Normal.BackColor = COL;
            bar3.BarAppearance.Hovered.BackColor = COL;
            bar3.BarAppearance.Pressed.BackColor = COL;
            bar3.BarAppearance.Disabled.BackColor = COL;
            groupControl6.AppearanceCaption.ForeColor = COL;
            panel1.BackColor = COL;
            panel2.BackColor = COL;
            panel3.BackColor = COL;
            barManager1.StatusBar.Appearance.BackColor = COL;
            barManager1.StatusBar.Appearance.BorderColor = COL;
            barManager1.StatusBar.BarAppearance.Normal.BackColor = COL;
            barManager1.StatusBar.BarAppearance.Hovered.BackColor = COL;
            barManager1.StatusBar.BarAppearance.Pressed.BackColor = COL;
            barManager1.StatusBar.BarAppearance.Disabled.BackColor = COL;
            barManager1.Form.BackColor = COL;
            labelControl3.BackColor = COL;
            labelControl4.BackColor = COL;
            labelControl5.BackColor = COL;
            labelControl6.BackColor = COL;
            labelControl8.BackColor = COL;
            labelControl15.BackColor = COL;
            labelControl17.BackColor = COL;
            labelControl18.BackColor = COL;
            labelControl21.BackColor = COL;
            this.Refresh();
            this.ForceRefresh();
        }
        public static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');

            if (hex.Length != 6)
            {
                throw new ArgumentException("Invalid hex string. Hex string must be 6 characters long (excluding #). " + hex);
            }

            try
            {
                int red = Convert.ToInt32(hex.Substring(0, 2), 16);
                int green = Convert.ToInt32(hex.Substring(2, 2), 16);
                int blue = Convert.ToInt32(hex.Substring(4, 2), 16);
                return Color.FromArgb(red, green, blue);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid hex string. " + ex.Message, ex);
            }
        }
        public void load_players()
        {
            new PlayerDatabase().GetAllPlayers(allPlayersDataTable);
        }
        private void LoadMessages()
        {
            string json = string.Format("Settings/auto_messages.json");
            List<AutoMessage> messages = new List<AutoMessage>();
            if (File.Exists(json))
            {
                messages = JsonConvert.DeserializeObject<List<AutoMessage>>(File.ReadAllText(json));
            }
            else
            {
                List<AutoMessage> messageData = new List<AutoMessage>();
                messageData.Add(new AutoMessage { Message = "Craft A Note With Wood To Chat With Other Players In Game!" });
                messageData.Add(new AutoMessage { Message = "Report Suspected Cheaters To Admins!" });
                string jsonContent = JsonConvert.SerializeObject(messageData, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText("Settings/auto_messages.json", jsonContent);
            }
            autoMessages.Items.Clear();
            foreach (var message in messages)
            {
                autoMessages.Items.Add($"{message.Message}");
            }
        }
        private void LoadCrates()
        {
            string json = "Events/locked_crate_event.json";

            if (File.Exists(json))
            {
                crateGroups = JsonConvert.DeserializeObject<List<LockedCrateGroup>>(File.ReadAllText(json));
            }
            else
            {
                crateGroups = new List<LockedCrateGroup>
                {
                    new LockedCrateGroup
                    {
                        Name = "Launch Site",
                        Positions = new List<LockedCrate>()
                    }
                };
                string jsonContent = JsonConvert.SerializeObject(crateGroups, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText("Events/locked_crate_event.json", jsonContent);
            }
            lockedCrateGroupName.Properties.Items.Clear();
            foreach (var crateGroup in crateGroups)
            {
                lockedCrateGroupName.Properties.Items.Add(crateGroup.Name);
            }
            if (lockedCrateGroupName.Properties.Items.Count > 0)
            {
                lockedCrateGroupName.SelectedIndex = 0;
            }
            UpdateLockedCratePositions();
        }
        private void LoadKit(int kit)
        {
            string json = string.Format("Kits/custom_kit{0}.json", kit);
            List<Kit> custom_kit = new List<Kit>();
            if (File.Exists(json))
            {
                custom_kit = JsonConvert.DeserializeObject<List<Kit>>(File.ReadAllText(json));
            }
            switch (kit)
            {
                case 1:
                    customKit1Box.Items.Clear();
                    break;
                case 2:
                    customKit2Box.Items.Clear();
                    break;
                case 3:
                    customKit2Box.Items.Clear();
                    break;
            }
            foreach (var itemData in custom_kit)
            {
                switch (kit)
                {
                    case 1:
                        customKit1Box.Items.Add($"{itemData.Item}:{itemData.Amount}");
                    break;
                    case 2:
                        customKit2Box.Items.Add($"{itemData.Item}:{itemData.Amount}");
                    break;
                    case 3:
                        customKit2Box.Items.Add($"{itemData.Item}:{itemData.Amount}");
                    break;
                }
            }
        }
        public void CopyFromDT(int i)
        {
            try
            {
                if (Players.Rows[Players.CurrentRow.Index].Cells[i].Value.ToString() != "")
                {
                    Clipboard.SetText(Players.Rows[Players.CurrentRow.Index].Cells[i].Value.ToString());
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            catch (NullReferenceException)
            {
                XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
        public void CopyFromODT(int i)
        {
            try
            {
                if (AllPlayers.Rows[AllPlayers.CurrentRow.Index].Cells[i].Value.ToString() != "")
                {
                    Clipboard.SetText(AllPlayers.Rows[AllPlayers.CurrentRow.Index].Cells[i].Value.ToString());
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            catch (NullReferenceException)
            {
                XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
        public static string GetFromDT(int i)
        {
            try
            {
                if (Players.Rows[Players.CurrentRow.Index].Cells[i].Value.ToString() != "")
                {
                    return Players.Rows[Players.CurrentRow.Index].Cells[i].Value.ToString();
                }
                else
                {
                    XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return null;
                }
            }
            catch (NullReferenceException)
            {
                XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }
        }
        public static string GetFromODT(int i)
        {
            try
            {
                if (AllPlayers.Rows[AllPlayers.CurrentRow.Index].Cells[i].Value.ToString() != "")
                {
                    return AllPlayers.Rows[AllPlayers.CurrentRow.Index].Cells[i].Value.ToString();
                }
                else
                {
                    XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return null;
                }
            }
            catch (NullReferenceException)
            {
                XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }
        }
        public static string GetFromBDT(int i)
        {
            try
            {
                if (Bans.Rows[Bans.CurrentRow.Index].Cells[i].Value.ToString() != "")
                {
                    return Bans.Rows[Bans.CurrentRow.Index].Cells[i].Value.ToString();
                }
                else
                {
                    XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return null;
                }
            }
            catch (NullReferenceException)
            {
                XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }
        }

        public void SetInfo()
        {
            if (!string.IsNullOrEmpty(ServerInfo.Hostname))
            {
                this.Text = "RCE Admin - " + ServerInfo.Hostname;
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (WebSocketsWrapper.IsConnected())
                WebSocketsWrapper.Disconnect();
        }
        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            textBoxPassword.Properties.UseSystemPasswordChar = !checkBoxShowPassword.Checked;
        }
        static bool IsValidIPv4(string ipAddress)
        {
            IPAddress result;
            return IPAddress.TryParse(ipAddress, out result) && result.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }

        static bool IsValidPort(string port)
        {
            int portNumber;
            return int.TryParse(port, out portNumber) && portNumber > 0 && portNumber <= 65535;
        }
        public void save_settings(string theme = "")
        {
            if (!IsValidIPv4(textBoxAddress.Text))
            {
                XtraMessageBox.Show("Please Enter A Valid IP Address!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (!IsValidPort(textBoxPort.Text))
            {
                XtraMessageBox.Show("Please Enter A Valid Port!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                Color theme_ = string.IsNullOrEmpty(theme) ? HexToColor("#cc402a") : HexToColor(theme);
                Settings.Write(new Settings(
                    textBoxAddress.Text,
                    textBoxPort.Text,
                    textBoxPassword.Text,
                    eventsWebhookUrl.Text,
                    killfeedsWebhookUrl.Text,
                    chatWebhookUrl.Text,
                    teamWebhookUrl.Text,
                    itemWebhookUrl.Text,
                    inGameName.Text,
                    autoMessagesCheck.Checked,
                    Convert.ToInt32(AutoMessageTime.Value),
                    InGameKillFeedCheck.Checked,
                    DiscordKillFeedCheck.Checked,
                    InGameEventFeedCheck.Checked,
                    DiscordEventFeedCheck.Checked,
                    InGameChatCheck.Checked,
                    DiscordChatCheck.Checked,
                    ColorToHex(theme_)
                 ));
                Settings = Settings.Read();
            }
        }
        private void buttonSave_Click(object sender, EventArgs e)
        {
            save_settings();
        }
        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.ServerAddress) || string.IsNullOrEmpty(Settings.ServerPort) || string.IsNullOrEmpty(Settings.ServerPassword))
                XtraMessageBox.Show("Seems Like Your Trying To Connect Using The Default Config, Did You Save The Information Before Trying To Connect?", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            else
            {
                WebSocketsWrapper.Connect();
                await Task.Delay(3000);
                if (WebSocketsWrapper.IsConnected())
                {
                    SetInfo();
                    await SendAutoMessages();
                    await SendFeedMessage();
                }
            }
        }
        async Task SendFeedMessage()
        {
            while (true)
            {
                WebSocketsWrapper.Send(string.Format("global.say {0}", "Feeds Provided By <color=red>RCE Admin</color>"));
                await Task.Delay(10 * 60 * 1000);
            }
        }
        async Task SendAutoMessages()
        {
            while (Settings.AutoMessages)
            {
                foreach (var item in autoMessages.Items)
                {
                    WebSocketsWrapper.Send(string.Format("global.say <color=green>[AUTO MESSAGE]</color> {0}", item.ToString()));
                    await Task.Delay(5 * 60 * 1000);
                }
            }
        }
        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Disconnect();
        }
        private void buttonCommand_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.SendCommand(textBoxCommand.Text);
            textBoxCommand.Text = "";
        }
        private void buttonBroadcast_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send($"global.say {broadcastMessageBox.Text}");
            broadcastMessageBox.Text = "";
        }
        private void textBoxCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((ConsoleKey)e.KeyChar == ConsoleKey.Enter)
            {
                WebSocketsWrapper.SendCommand(textBoxCommand.Text);
                textBoxCommand.Text = "";
            }
        }
        private void buttonClear_Click(object sender, EventArgs e)
        {
            ServerConsole.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            rpcClient.Dispose();
            Environment.Exit(0);
        }

        [STAThread]
        private void copyNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyFromDT(1);
            XtraMessageBox.Show(string.Format("Gamertag {0} Has Been Copied To Your Clipboard!", GetFromDT(1)), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void kickPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("kick \"{0}\"", GetFromDT(1)));
        }

        private void banPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("banid \"{0}\"", GetFromDT(1)));
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            Process.Start("https://prnt.sc/Qc-uU0PHiPx8");
            XtraMessageBox.Show(string.Format("Go To Your Servers Console On GPortal{0}Press CTRL, SHIFT & I To Open Inspect Element{0}Click On The ngapi/ Requests Until You Find The One With The Sameish Output At The Screenshot We Just Opened{0}Follow The Screenshot For The RCON Password!", Environment.NewLine), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void checkButton1_CheckedChanged(object sender, EventArgs e)
        {
            save_settings();
        }

        private void checkButton2_CheckedChanged(object sender, EventArgs e)
        {
            save_settings();
        }

        private void checkButton4_CheckedChanged(object sender, EventArgs e)
        {
            save_settings();
        }

        private void checkButton3_CheckedChanged(object sender, EventArgs e)
        {
            save_settings();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("VIPID \"{0}\"", GetFromDT(1)));
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveVIP \"{0}\"", GetFromDT(1)));
        }

        private void addToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("ModeratorID \"{0}\"", GetFromDT(1)));
        }

        private void removeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveModerator \"{0}\"", GetFromDT(1)));
        }

        private void addToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("AdminID \"{0}\"", GetFromDT(1)));
        }

        private void removeToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveAdmin \"{0}\"", GetFromDT(1)));
        }

        private void addToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("OwnerID \"{0}\"", GetFromDT(1)));
        }

        private void removeToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveOwner \"{0}\"", GetFromDT(1)));
        }

        private void broadcastMessageBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((ConsoleKey)e.KeyChar == ConsoleKey.Enter)
            {
                WebSocketsWrapper.Send($"global.say {broadcastMessageBox.Text}");
                broadcastMessageBox.Text = "";
            }
        }
        public static void give_item_to_player(string player, string item, int amount = 1)
        {
            WebSocketsWrapper.Send(string.Format("inventory.giveto \"{0}\" \"{1}\" \"{2}\"", player, item, amount));
        }
        public static void teleport_to(string player)
        {
            WebSocketsWrapper.Send(string.Format("global.teleport \"{0}\" \"{1}\"", Settings.InGameName, player));
        }
        public static async void teleport_here(string player)
        {
            WebSocketsWrapper.Send(string.Format("printpos \"{0}\"", Settings.InGameName));
            await Task.Delay(1000);
            string pos = ServerConsole.ReadLastFewLines();
            if (ServerConsole.IsValidPrintPos(pos))
            {
                WebSocketsWrapper.Send(string.Format("teleportpos \"{1}\" \"{0}\"", player, pos.Replace(" ", "").Replace("(", "").Replace(")", "")));
            }
            else
            {
                XtraMessageBox.Show("Failed To Find Your Position, Try Again!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void give_item_to_all(string item)
        {
            WebSocketsWrapper.Send(string.Format("inventory.giveall \"{0}\"", item));
        }

        private void boneClubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bone.club");
        }

        private void compoundBowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bow.compound");
        }

        private void huntingBowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bow.hunting");
        }

        private void crossbowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "crossbow");
        }

        private void flameThrowerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "flamethrower");
        }

        private void beancanGrenadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "grenade.beancan");
        }

        private void f1GrenadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "grenade.f1");
        }

        private void smokeGrenadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "grenade.smoke");
        }

        private void boneKnifeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "knife.bone");
        }

        private void combatKnifeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "knife.combat");
        }

        private void m249ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lmg.m249");
        }

        private void longSwordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "longsword");
        }

        private void maceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "mace");
        }

        private void macheteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "machete");
        }

        private void grenadeLauncherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "multiplegrenadelauncher");
        }

        private void paddleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "paddle");
        }

        private void eokaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.eoka");
        }

        private void m92ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.m92");
        }

        private void nailgunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.nailgun");
        }

        private void pythonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.python");
        }

        private void revolverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.revolver");
        }

        private void semiAutoPistolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pistol.semiauto");
        }

        private void assaultRifleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.ak");
        }

        private void boltToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.bolt");
        }

        private void l96ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.l96");
        }

        private void lR300ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.lr300");
        }

        private void m339ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.m39");
        }

        private void semiAutoRifToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.semiauto");
        }

        private void dataGridViewPlayers_Click(object sender, EventArgs e)
        {
            MainForm_MouseClick(sender, (MouseEventArgs)e);
        }
        private void rocketLauncherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rocket.launcher");
        }
        private void salvagedCleaverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "salvaged.cleaver");
        }
        private void salvagedSwordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "salvaged.sword");
        }
        private void doubleBarrelShotgunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shotgun.double");
        }

        private void pumpShotgunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shotgun.pump");
        }

        private void spas12ShotgunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shotgun.shas12");
        }

        private void waterpipeShotguunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shotgun.waterpipe");
        }

        private void customSMGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "smg.2");
        }

        private void mP5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "smg.mp5");
        }

        private void thompsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "smg.thompson");
        }

        private void stoneSpearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "spear.stone");
        }

        private void woodenSpearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "spear.wooden");
        }

        private void spearguunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "speargun");
        }

        private void flashlightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.flashlight");
        }

        private void holoSightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.holosight");
        }
        private void laserSightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.lasersight");
        }
        private void muzzleBoostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.muzzleboost");
        }
        private void muzzleBreakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.muzzlebreak");
        }
        private void silencerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.silencer");
        }
        private void simpleHandmadeSigntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.simplesight");
        }
        private void xscopeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "weapon.mod.small.scope");
        }

        private void concreteBarricadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.concrete");
        }

        private void metalBarricadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.metal");
        }

        private void sandBagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.sandbags");
        }

        private void stoneBarricadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.stone");
        }

        private void woodBarricadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.wood");
        }

        private void smallWoodenWallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.wood.cover");
        }

        private void barbedWoodenBarricadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "barricade.woodwire");
        }

        private void buuildingPlanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "building.planner");
        }

        private void toolCupboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "cupboard.tool");
        }

        private void metaldoubleDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.double.hinged.metal");
        }

        private void armouredDoubleDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.double.hinged.toptier");
        }

        private void wooddenDoubleDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.double.hinged.wood");
        }

        private void singleMetalDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.hinged.metal");
        }

        private void armouredDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.hinged.toptier");
        }

        private void woodenDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.hinged.wood");
        }

        private void floorGrillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "floor.grill");
        }

        private void ladderHtachToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "floor.ladder.hatch");
        }

        private void triangleFloorGrillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "floor.triangle.grill");
        }

        private void triangleLadderHatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "floor.triangle.ladder.hatch");
        }

        private void highExternalStoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "gates.external.high.stone");
        }

        private void highExternalWoodGateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "gates.external.high.wood");
        }

        private void ladderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ladder.wooden.wall");
        }

        private void codeLockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lock.code");
        }

        private void keyLockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lock.key");
        }

        private void hoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shutter.metal.embrasure.a");
        }

        private void metalVerticalEmbrasureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shutter.metal.embrasure.b");
        }

        private void woodShuttersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shutter.wood.a");
        }

        private void highExternalWoodenWallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.external.high");
        }

        private void highExternalStoneWallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.external.high.stone");
        }

        private void prisonCellWallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.cell");
        }

        private void prisonCellGateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.cell.gate");
        }

        private void chainLinkFenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.fence");
        }

        private void chainLinkGateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.fence.gate");
        }

        private void garageDoorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.garagedoor");
        }

        private void nettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.netting");
        }

        private void woodenShopFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.shopfront");
        }

        private void metalShopFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.frame.shopfront.metal");
        }

        private void metalWindowBarsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.window.bars.metal");
        }

        private void reinforcedGlassWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.window.bars.toptier");
        }

        private void strengthenedGlasWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wall.window.glass.reinforced");
        }

        private void woodenWatchTowerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "watchtower.wood");
        }

        private void largeWacerCatcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "water.catcher.large");
        }

        private void smallWaterCatcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "water.catcher.small");
        }

        private void barbequeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bbq");
        }

        private void botaBagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "botabag");
        }

        private void bedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bed");
        }

        private void smallFurnaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "furnace");
        }

        private void fridgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fridge");
        }

        private void smallFishTrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fishtrap.small");
        }

        private void stoneFirePlaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fireplace.stone");
        }

        private void dropBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "dropbox");
        }

        private void composerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "composter");
        }

        private void chairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chair");
        }

        private void campFireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "campfire");
        }

        private void largeWoodenBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "box.wooden.large");
        }

        private void smallWoodenBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "box.wooden.small");
        }

        private void repairBenchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "box.repair.bench");
        }

        private void largeFuurnaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "furnace.large");
        }

        private void hitchTrouughToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hitchtroughcombo");
        }

        private void kayakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "kayak");
        }

        private void lanternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lantern");
        }

        private void mailboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "mailbox");
        }

        private void mixingTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "Mixing Table");
        }

        private void largePlanterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "planter.large");
        }

        private void smallPlanterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "planter.small");
        }

        private void researchTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "research.table");
        }

        private void bearRuugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rug.bear");
        }

        private void rugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rug");
        }

        private void salvaagedShelvesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shelves");
        }

        private void sleepingBagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "sleepingbag");
        }

        private void workbenchLevel3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "workbench3");
        }

        private void workbechLevel2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "workbench2");
        }

        private void workbenchLevel1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "workbench1");
        }

        private void waterPuurifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "water.purifier");
        }

        private void waterBarrelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "water.barrel");
        }

        private void vendingMachineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "vending.machine");
        }

        private void tuunaCanLampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tunalight");
        }

        private void tableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "table");
        }

        private void smallStashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "stash.small");
        }

        private void smallOilRefineryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "small.oil.refinery");
        }

        private void highQualityMetalOreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hq.metal.ore", 100);
        }

        private void horseDungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horsedung");
        }

        private void gunPowderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "gunpowder", 1000);
        }

        private void fertilizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fertilizer", 1000);
        }

        private void animalFatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fat.animal", 1000);
        }

        private void explosivesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "explosives", 100);
        }

        private void dieselFuelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "diesel_barrel");
        }

        private void crudeOilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "crude.oil");
        }

        private void clothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "cloth", 1000);
        }

        private void charcoalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "charcoal", 1000);
        }

        private void cCTVCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "cctv.camera");
        }

        private void emptyCanOfTunaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "can.tuna.empty");
        }

        private void emptyCanOfBeansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "can.beans.empty");
        }

        private void boneFragmentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bone.fragments", 1000);
        }

        private void leatherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "leather", 1000);
        }

        private void lowGradeFuelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lowgradefuel", 500);
        }

        private void metalFragmentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metal.fragments", 1000);
        }

        private void metalOreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metal.ore", 1000);
        }

        private void highQualityMetalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metal.refined", 100);
        }

        private void plantFiberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "plantfiber", 1000);
        }

        private void scrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "scrap", 1000);
        }

        private void wolfSkuullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "skull.wolf", 1000);
        }

        private void stoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "stones", 1000);
        }

        private void sulfurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "sulfur", 1000);
        }

        private void sulfurOreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "sulfur.ore", 1000);
        }

        private void targetingComputerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "targeting.computer");
        }

        private void woodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wood", 1000);
        }

        private void vestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.vest");
        }

        private void skirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.skirt");
        }

        private void ponchoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.poncho");
        }

        private void pantsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.pants");
        }

        private void halterneckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.helterneck");
        }

        private void bootsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "attire.hide.boots");
        }

        private void boneArmourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bone.armor.suit");
        }

        private void frogBootsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "boots.frog");
        }

        private void buucketHelmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bucket.helmet");
        }

        private void glovesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "burlap.gloves");
        }

        private void headWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "burlap.headwrap");
        }

        private void shirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "burlap.shirt");
        }

        private void shoesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "burlap.shoes");
        }

        private void trousersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "burlap.trousers");
        }

        private void coffeeCanHelmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "coffeecan.helmet");
        }

        private void boneHelmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "deer.skull.mask");
        }

        private void finsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "diving.fins");
        }

        private void maskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "diving.mask");
        }

        private void tankToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "diving.tank");
        }

        private void wetSuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "diving.wetsuit");
        }

        private void beenieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hat.beenie");
        }

        private void boonieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hat.boonie");
        }

        private void candleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hat.candle");
        }

        private void minerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hat.miner");
        }

        private void wolfHedressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hat.wolf");
        }

        private void hazmatSuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hazmatsuit");
        }

        private void helmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "heavy.plate.helmet");
        }

        private void jacketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "heavy.plate.jacket");
        }

        private void pantsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "heavy.plate.pants");
        }

        private void hoodieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hoodie");
        }

        private void roadsignArmorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.armor.roadsign");
        }

        private void woodenArmorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.armor.wood");
        }

        private void saddleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.saddle");
        }

        private void doubleSaddleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.saddle.double");
        }

        private void saddleBagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.saddlebag");
        }

        private void basicShoesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.shoes.basic");
        }

        private void highQualityShoesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horse.shoes.advanced");
        }

        private void jacketToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "jacket");
        }

        private void snowJacketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "jacket.snow");
        }

        private void lumberjackHoodieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "lumberjack hoodie");
        }

        private void balaclavaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "mask.balaclava");
        }

        private void bandanaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "mask.bandana");
        }

        private void metalFacemaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metal.facemask");
        }

        private void metalChestPlateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metal.plate.torso");
        }

        private void pantsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pants");
        }

        private void shortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pants.shorts");
        }

        private void riotHelmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "riot.helmet");
        }

        private void glovesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "roadsign.gloves");
        }

        private void jacketToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "roadsign.jacket");
        }

        private void kiltToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "roadsign.kilt");
        }

        private void shirtToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shirt.collared");
        }

        private void bootsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shoes.boots");
        }

        private void tacticalGlovesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tactical.gloves");
        }

        private void tShirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tshirt.long");
        }

        private void longsleeveTShirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tshirt");
        }

        private void tankTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "shirt.tanktop");
        }

        private void helmetToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wood.armor.helmet");
        }

        private void jacketToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wood.armor.jacket");
        }

        private void pantsToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wood.armor.pants");
        }

        private void salvagedAceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "axe.salvaged");
        }

        private void waterBuucketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bucket.water");
        }

        private void chainsawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chainsaw");
        }

        private void satchelChargeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "explosive.satchel");
        }

        private void timedExplosiveChargeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "explosive.timed");
        }

        private void fishingRodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fishingrod.handmade");
        }

        private void flareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "flare");
        }

        private void flashlightToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "flashlight.held");
        }

        private void hammerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hammer");
        }

        private void salvagedHammerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hammer.salvaged");
        }

        private void hatchetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hatchet");
        }

        private void salvagedIcepickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "icepick.salvaged");
        }

        private void jackhammerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "jackhammer");
        }

        private void pickaxeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pickaxe");
        }

        private void rFTransmitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rf.detonator");
        }

        private void rockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rock");
        }

        private void stonePickaxeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "stone.pickace");
        }

        private void stoneHatchetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "stonehatchet");
        }

        private void suupplySignalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "supply.signal");
        }

        private void binocuularsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tool.binoculars");
        }

        private void torchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "torch");
        }

        private void antiRadiationPillsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "antiradpills");
        }

        private void bandageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bandage");
        }

        private void largeMedKitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "largemedkit");
        }

        private void medicalSyringeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "syringe.medical");
        }

        private void blueBerryCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.blue.berry");
        }

        private void cornCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.corn");
        }

        private void greenBerryCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.green.berry");
        }

        private void hedmpCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.hemp");
        }

        private void potatoCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.potato");
        }

        private void pumpkinCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.pumpkin");
        }

        private void redBerryCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.red.berry");
        }

        private void whiteBerryCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.white.berry");
        }

        private void yellowBerryCloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "clone.yellow.berry");
        }

        private void blueBerryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.blue.berry");
        }

        private void cornToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.corn");
        }

        private void greenBerryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.green.berry");
        }

        private void hempToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.hemp");
        }

        private void potatoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.potato");
        }

        private void pumpkinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.pumpkin");
        }

        private void redBerryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.red.berry");
        }

        private void whiteBerryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.white.berry");
        }

        private void ywllowBerryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "seed.yellow.berry");
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "radiationresisttea");
        }

        private void advancedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "radiationresisttea.advanced");
        }

        private void pureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "radiationresisttea.pure");
        }

        private void basicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "oretea");
        }

        private void advancedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "oretea.advanced");
        }

        private void puureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "oretea.pure");
        }

        private void basicToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "scraptea");
        }

        private void advancedToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "scraptea.advanced");
        }

        private void pureToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "scraptea.pure");
        }

        private void basicToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "woodtea");
        }

        private void advancedToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "woodtea.advanced");
        }

        private void pureToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "woodtea.pure");
        }

        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "red.berry");
        }

        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "green.berry");
        }

        private void ywllowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "yellow.berry");
        }

        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "blue.berry");
        }

        private void whiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "white.berry");
        }

        private void rawToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wolfmeat.raw");
        }

        private void cookedToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wolfmeat.cooked");
        }

        private void burnedToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wolfmeat.burned");
        }

        private void rawToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "meat.boar");
        }

        private void cookedToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "meat.pork.cooked");
        }

        private void buurnedToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "meat.pork.burned");
        }

        private void rawToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "humanmeat.raw");
        }

        private void cToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "humanmeat.cooked");
        }

        private void buurnedToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "humanmeat.burned");
        }


        private void toolStripMenuItem6_Click_1(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horsemeat.cooked");
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horsemeat.raw");
        }

        private void buurnedToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "horsemeat.burned");
        }

        private void rawToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "deermeat.raw");
        }

        private void cookedToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "deermeat.cooked");
        }

        private void buurnedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "deermeat.burned");
        }

        private void rawToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chicken.raw");
        }

        private void cookedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chicken.cooked");
        }

        private void burnedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chicken.burned");
        }

        private void buurnedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bearmeat.burned");
        }

        private void cookedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bearmeat.cooked");
        }

        private void rawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "bearmeat");
        }

        private void anchovyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.anchovy");
        }

        private void catfishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.catfish");
        }

        private void cookedToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.cooked");
        }

        private void herringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.herring");
        }

        private void minnorwsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.minnows");
        }

        private void rawToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.raw");
        }

        private void salmonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.salmon");
        }

        private void sardineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.sardine");
        }

        private void smallSharkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.smallshark");
        }

        private void smallTrouutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.troutsmall");
        }

        private void ywlloePerchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fish.yellowperch");
        }

        private void basicToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "healingtea");
        }

        private void advancedToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "healingtea.advanced");
        }

        private void puureToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "healingtea.pure");
        }

        private void basicToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "maxhealthtea");
        }

        private void advancedToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "maxhealthtea.advanced");
        }

        private void pureToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "maxhealthtea.pure");
        }

        private void appleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "apple");
        }

        private void blackRaspbeerriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "black.raspberries");
        }

        private void bluueBerriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "blueberries");
        }

        private void cactusFleshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "cactusflesh");
        }

        private void canOfBeansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "can.beans");
        }

        private void canOfTuunaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "can.tuna");
        }

        private void chocholateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "chocholate");
        }

        private void cornToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "corn");
        }

        private void granolaBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "granolabar");
        }

        private void grubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "grub");
        }

        private void picklesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "jar.pickle");
        }

        private void mushroomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "mushroom");
        }

        private void potatoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "potato");
        }

        private void pumpkinToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "pumpkin");
        }

        private void smallWaterBottleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "smallwaterbottle");
        }

        private void waterJugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "waterjug");
        }

        private void wormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "worm");
        }

        private void gLBuckshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.grenadelauncher.buckshot", 32);
        }

        private void hEGrenadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.grenadelauncher.he", 32);
        }

        private void mmSmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.grenadelauncher.smoke", 32);
        }

        private void handmadeShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.handmade.shell", 64);
        }

        private void nailguunNailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.nailgun.nails", 64);
        }

        private void pistolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.pistol", 128);
        }

        private void incendiaryPistolBulletToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.pistol.fire", 128);
        }

        private void highVelocityPistolBluuuetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.pistol.hv", 128);
        }

        private void rifleAmmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rifle", 128);
        }

        private void explosive556RifleAmmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rifle.explosive", 128);
        }

        private void hV556RifleAmmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rifle.hv", 128);
        }

        private void inccccccendiary556RifleAmmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rifle.incendiary", 128);
        }

        private void rocketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rocket.basic");
        }

        private void incendiaryRocketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rocket.fire");
        }

        private void highVelocityRocketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.rocket.hv");
        }

        private void guageBuckshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.shotgun", 64);
        }

        private void guuageIncendiaryShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.shotgun.fire", 64);
        }

        private void guageSluugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ammo.shotgun.slug", 64);
        }

        private void boneArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "arrow.bone", 64);
        }

        private void fireArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "arrow.fire", 64);
        }

        private void highVelocityArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "arrow.hv", 64);
        }

        private void woodenArrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "arrow.wooden", 64);
        }

        private void speargunSpearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "speargun.spear");
        }

        private void flameTurretToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "flameturret");
        }

        private void shotgunTrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "guntrap");
        }

        private void woodenFloorSpikesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "spikes.floor");
        }

        private void snapTrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "trap.bear");
        }

        private void homemadeLandmineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "trap.landmine");
        }

        private void redToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "keycard_red");
        }

        private void greenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "keycard_green");
        }

        private void blueToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "keycard_blue");
        }

        private void doorKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "door.key");
        }

        private void guitarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fun.guitar");
        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "note");
        }

        private void fuseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fuse");
        }

        private void gearsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "gears");
        }

        private void metalBladeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metalblade");
        }

        private void metalPipeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metalpipe");
        }

        private void metalSpringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "metalspring");
        }

        private void propaneTankToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "propanetank");
        }

        private void rifleBodyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "riflebody");
        }

        private void roadsignToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "roadsigns");
        }

        private void ropeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rope");
        }

        private void semiAutomaticBodyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "semibody");
        }

        private void sewingKitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "sewingkit");
        }

        private void sheetMetalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "sheetmetal");
        }

        private void sMGBodyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "smgbody");
        }

        private void tarpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "tarp");
        }

        private void techTrshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "techparts");
        }

        private void auutoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "autoturret");
        }

        private void ceilingLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "ceilinglight");
        }

        private void computerStationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "computerstation");
        }

        private void andSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.andswitch");
        }

        private void audioAlarmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.audioalarm");
        }

        private void largeBatteryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.battery.rechargeable.large");
        }

        private void mediumBatteryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.battery.rechargeable.medium");
        }

        private void smallBatteryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.battery.rechargeable.small");
        }

        private void blockerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.blocker");
        }

        private void buuttonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.button");
        }

        private void couunterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.counter");
        }

        private void doorControllerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.doorcontroller");
        }

        private void flasherLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.flasherlight");
        }

        private void smallGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.fuelgenerator.small");
        }

        private void hBHFSensorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.hbhfsensor");
        }

        private void heaterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.heater");
        }

        private void igniterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.igniter");
        }

        private void laserDetectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.laserdetector");
        }

        private void oRSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.orswitch");
        }

        private void pressurePadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.pressurepad");
        }

        private void randSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.random.switch");
        }

        private void rFBroadcasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.rf.broadcaster");
        }

        private void rFReceiverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.rf.receiver");
        }

        private void sirenLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.sirenlight");
        }

        private void largeSolarPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.solarpanel.large");
        }

        private void splitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.splitter");
        }

        private void sprinklerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.sprinkler");
        }

        private void switchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.switch");
        }

        private void teslaCoilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.teslacoil");
        }

        private void timerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.timer");
        }

        private void xORSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electric.xorswitch");
        }

        private void branchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electrical.branch");
        }

        private void combinerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electrical.combiner");
        }

        private void memoryCellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "electrical.memorycell");
        }

        private void elevatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "elevator");
        }

        private void fluidCombinerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fluid.combiner");
        }

        private void fluidSplitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fluid.splitter");
        }

        private void fluidSwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "fluid.switch");
        }

        private void windTurbineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "generator.wind.scrap");
        }

        private void hoseToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hosetool");
        }

        private void poweredWaterPurifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "powered.water.purifier");
        }

        private void rFPagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rf_pager");
        }

        private void searchLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "searchlight");
        }

        private void storageMonitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "storage.monitor");
        }

        private void reactiveTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "target.reactive");
        }

        private void waterPuumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "waterpump");
        }

        private void wireToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "wiretool");
        }

        private void teleportToYouToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Settings.InGameName))
            {
                string name = GetFromDT(1);
                DialogResult tp = XtraMessageBox.Show(string.Format("Are You Sure You Want To Teleport {0} To You?", name), "RCE Admin - Teleportation", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                if (tp == DialogResult.Yes)
                {
                    teleport_here(name);
                }
            }
            else
                XtraMessageBox.Show(string.Format("Can't Teleport To {0} As You Have Not Set Your In Game Name!", GetFromDT(1)), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void teleporToThemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Settings.InGameName))
            {
                string name = GetFromDT(1);
                DialogResult tp = XtraMessageBox.Show(string.Format("Are You Sure You Want To Teleport To {0}?", name), "RCE Admin - Teleportation", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                if (tp == DialogResult.Yes)
                {
                    teleport_to(name);
                }
            }
            else
                XtraMessageBox.Show(string.Format("Can't Teleport To {0} As You Have Not Set Your In Game Name!", GetFromDT(1)), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        class PSNRequest
        {
            public string onlineId;
            public bool reserveIfAvailable = false;
        }
        public static string CheckPSN(string PSN)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://accounts.api.playstation.com/api/v1/accounts/onlineIds");
            req.ContentType = "application/json";
            req.Method = WebRequestMethods.Http.Post;
            PSNRequest o = new PSNRequest();
            o.onlineId = PSN;
            string json = JsonConvert.SerializeObject(o);
            using (var streamWriter = new StreamWriter(req.GetRequestStream()))
            {
                streamWriter.Write(json);
            }
            try
            {
                var resp = (HttpWebResponse)req.GetResponse();
                return null;
            }
            catch (WebException e)
            {
                using (WebResponse resp = e.Response)
                {
                    using (var reader = new StreamReader(resp.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
        private void hazzyMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "hazmatsuit");
            give_item_to_player(GetFromDT(1), "smg.mp5");
            give_item_to_player(GetFromDT(1), "weapon.mod.holosight");
            give_item_to_player(GetFromDT(1), "weapon.mod.lasersight");
            give_item_to_player(GetFromDT(1), "ammo.pistol", 128);
            give_item_to_player(GetFromDT(1), "syringe.medical", 6);
            give_item_to_player(GetFromDT(1), "bandage", 10);
            give_item_to_player(GetFromDT(1), "jackhammer");
            give_item_to_player(GetFromDT(1), "chainsaw");
            give_item_to_player(GetFromDT(1), "lowgradefuel", 50);
        }
        public static void give_full_kit(string name)
        {
            give_item_to_player(name, "rifle.ak");
            give_item_to_player(name, "weapon.mod.holosight");
            give_item_to_player(name, "weapon.mod.lasersight");
            give_item_to_player(name, "ammo.rifle", 128);
            give_item_to_player(name, "syringe.medical", 6);
            give_item_to_player(name, "bandage", 10);
            give_item_to_player(name, "metal.facemask");
            give_item_to_player(name, "metal.plate.torso");
            give_item_to_player(name, "tactical.gloves");
            give_item_to_player(name, "pants");
            give_item_to_player(name, "hoodie");
            give_item_to_player(name, "roadsign.kilt");
            give_item_to_player(name, "shoes.boots");
            give_item_to_player(name, "jackhammer");
            give_item_to_player(name, "chainsaw");
            give_item_to_player(name, "lowgradefuel", 50);
        }
        private void fullKitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            give_item_to_player(GetFromDT(1), "rifle.ak");
            give_item_to_player(GetFromDT(1), "weapon.mod.holosight");
            give_item_to_player(GetFromDT(1), "weapon.mod.lasersight");
            give_item_to_player(GetFromDT(1), "ammo.rifle", 128);
            give_item_to_player(GetFromDT(1), "syringe.medical", 6);
            give_item_to_player(GetFromDT(1), "bandage", 10);
            give_item_to_player(GetFromDT(1), "metal.facemask");
            give_item_to_player(GetFromDT(1), "metal.plate.torso");
            give_item_to_player(GetFromDT(1), "tactical.gloves");
            give_item_to_player(GetFromDT(1), "pants");
            give_item_to_player(GetFromDT(1), "hoodie");
            give_item_to_player(GetFromDT(1), "roadsign.kilt");
            give_item_to_player(GetFromDT(1), "shoes.boots");
            give_item_to_player(GetFromDT(1), "jackhammer");
            give_item_to_player(GetFromDT(1), "chainsaw");
            give_item_to_player(GetFromDT(1), "lowgradefuel", 50);
        }

        private void simpleButton1_Click_1(object sender, EventArgs e)
        {
            Process.Start("https://github.com/KyleFardy/RCE-Admin");
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.com/invite/4wQyrjvCQS");
        }

        private void dataGridViewPlayers_SelectionChanged(object sender, EventArgs e)
        {
            if (Players.SelectedRows.Count > 0)
            {
                selectedPlayer = Players.SelectedRows[0].Index;
            }
        }

        private void customKit1ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        public class RustItem
        {
            public string Id { get; set; }
            public string Image { get; set; }
            public string DisplayName { get; set; }
            public string ShortName { get; set; }
            public string Category { get; set; }
        }
        static string FetchRustItems()
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    return webClient.DownloadString("https://cdn.void-dev.co/items.json");
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (customKit1AddItemamnt.Value <= 0)
            {
                XtraMessageBox.Show("Please Enter An Amount!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string jsonContent = FetchRustItems();
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    List<RustItem> items = JsonConvert.DeserializeObject<List<RustItem>>(jsonContent);
                    RustItem matchingItem = items.Find(item => item.ShortName == customKit1AddItem.Text);
                    if (matchingItem != null)
                    {
                        customKit1Box.Items.Add(
                            string.Format(
                                "{0}:{1}",
                                matchingItem.ShortName,
                                customKit1AddItemamnt.Value
                            )
                        );
                        customKit1AddItem.Text = string.Empty;
                    }
                    else
                    {
                        XtraMessageBox.Show(string.Format("{0} Is Not A Valid Item!", customKit1AddItem.Text), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                    XtraMessageBox.Show("There Was An Error Fetching The Rust Item List!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void autoMessages_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int selectedIndex = autoMessages.IndexFromPoint(e.Location);
                if (selectedIndex != ListBox.NoMatches)
                {
                    autoMessages.Items.RemoveAt(selectedIndex);

                    List<AutoMessage> messageData = new List<AutoMessage>();
                    foreach (var listBoxItem in autoMessages.Items)
                    {
                        messageData.Add(new AutoMessage { Message = listBoxItem.ToString() });
                    }
                    string jsonContent = JsonConvert.SerializeObject(messageData, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText("Settings/auto_messages.json", jsonContent);
                }
            }
        }

        private void customKit1Box_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int selectedIndex = customKit1Box.IndexFromPoint(e.Location);
                if (selectedIndex != ListBox.NoMatches)
                {
                    customKit1Box.Items.RemoveAt(selectedIndex);
                }
            }
        }

        private void customKit2Box_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int selectedIndex = customKit2Box.IndexFromPoint(e.Location);
                if (selectedIndex != ListBox.NoMatches)
                {
                    customKit2Box.Items.RemoveAt(selectedIndex);
                }
            }
        }

        private void customKit3Box_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int selectedIndex = customKit3Box.IndexFromPoint(e.Location);
                if (selectedIndex != ListBox.NoMatches)
                {
                    customKit3Box.Items.RemoveAt(selectedIndex);
                }
            }
        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            if (customKit2AddItemamnt.Value <= 0)
            {
                XtraMessageBox.Show("Please Enter An Amount!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string jsonContent = FetchRustItems();
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    List<RustItem> items = JsonConvert.DeserializeObject<List<RustItem>>(jsonContent);
                    RustItem matchingItem = items.Find(item => item.ShortName == customKit2AddItem.Text);
                    if (matchingItem != null)
                    {
                        customKit2Box.Items.Add(
                            string.Format(
                                "{0}:{1}",
                                matchingItem.ShortName,
                                customKit2AddItemamnt.Value
                            )
                        );
                        customKit2AddItem.Text = string.Empty;
                    }
                    else
                    {
                        XtraMessageBox.Show(string.Format("{0} Is Not A Valid Item!", customKit2AddItem.Text), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                    XtraMessageBox.Show("There Was An Error Fetching The Rust Item List!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void simpleButton7_Click(object sender, EventArgs e)
        {
            List<Kit> itemsData = new List<Kit>();
            foreach (var listBoxItem in customKit3Box.Items)
            {
                string[] parts = listBoxItem.ToString().Split(':');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out int amount))
                    {
                        itemsData.Add(new Kit { Item = parts[0], Amount = amount });
                    }
                }
            }
            string jsonContent = JsonConvert.SerializeObject(itemsData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("Kits/custom_kit3.json", jsonContent);
            XtraMessageBox.Show("Custom Kit 3 Has Been Saved!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public class Kit
        {
            public string Item { get; set; }
            public int Amount { get; set; }
        }
        public class LockedCrateGroup
        {
            public string Name { get; set; }
            public List<LockedCrate> Positions { get; set; }
        }
        public class LockedCrate
        {
            public string X { get; set; }
            public string Y { get; set; }
            public string Z { get; set; }
        }
        public class AutoMessage
        {
            public string Message { get; set; }
        }
        private void simpleButton4_Click(object sender, EventArgs e)
        {
            List<Kit> itemsData = new List<Kit>();
            foreach (var listBoxItem in customKit1Box.Items)
            {
                string[] parts = listBoxItem.ToString().Split(':');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out int amount))
                    {
                        itemsData.Add(new Kit { Item = parts[0], Amount = amount });
                    }
                }
            }
            string jsonContent = JsonConvert.SerializeObject(itemsData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("Kits/custom_kit1.json", jsonContent);
            XtraMessageBox.Show("Custom Kit 1 Has Been Saved!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            List<Kit> itemsData = new List<Kit>();
            foreach (var listBoxItem in customKit2Box.Items)
            {
                string[] parts = listBoxItem.ToString().Split(':');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out int amount))
                    {
                        itemsData.Add(new Kit { Item = parts[0], Amount = amount });
                    }
                }
            }
            string jsonContent = JsonConvert.SerializeObject(itemsData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("Kits/custom_kit2.json", jsonContent);
            XtraMessageBox.Show("Custom Kit 2 Has Been Saved!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void simpleButton8_Click(object sender, EventArgs e)
        {
            if (customKit3AddItemamnt.Value <= 0)
            {
                XtraMessageBox.Show("Please Enter An Amount!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string jsonContent = FetchRustItems();
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    List<RustItem> items = JsonConvert.DeserializeObject<List<RustItem>>(jsonContent);
                    RustItem matchingItem = items.Find(item => item.ShortName == customKit3AddItem.Text);
                    if (matchingItem != null)
                    {
                        customKit3Box.Items.Add(
                            string.Format(
                                "{0}:{1}",
                                matchingItem.ShortName,
                                customKit3AddItemamnt.Value
                            )
                        );
                        customKit3AddItem.Text = string.Empty;
                    }
                    else
                    {
                        XtraMessageBox.Show(string.Format("{0} Is Not A Valid Item!", customKit3AddItem.Text), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                    XtraMessageBox.Show("There Was An Error Fetching The Rust Item List!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void customKit1ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (TryLoadKit("Kits/custom_kit1.json", out List<Kit> custom_kit))
            {
                foreach (var itemData in custom_kit)
                {
                    give_item_to_player(GetFromDT(1), itemData.Item, itemData.Amount);
                }
            }
            else
            {
                XtraMessageBox.Show("Failed To Give Custom Kit 1, Have You Created One?", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void SpawnCrates(string selectedGroupName)
        {
            if (TryLoadCrates("Events/locked_crate_event.json", selectedGroupName, out List<LockedCrate> crates))
            {
                foreach (var crate in crates)
                {
                    WebSocketsWrapper.Send($"spawn codelocked {crate.X},{crate.Y},{crate.Z}");
                }
                WebSocketsWrapper.Send(string.Format("global.say Locked Crates Event Has Started At <color=green>{0}</color>!", selectedGroupName));
            }
            else
            {
                XtraMessageBox.Show("Failed To Load Positions!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool TryLoadCrates(string jsonPath, string groupName, out List<LockedCrate> crates)
        {
            crates = null;

            if (File.Exists(jsonPath))
            {
                var crateGroups = JsonConvert.DeserializeObject<List<LockedCrateGroup>>(File.ReadAllText(jsonPath));
                var selectedGroup = crateGroups.Find(group => group.Name == groupName);

                if (selectedGroup != null)
                {
                    crates = selectedGroup.Positions;
                    return true;
                }
            }

            return false;
        }
        bool TryLoadKit(string filePath, out List<Kit> kits)
        {
            kits = new List<Kit>();

            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                string jsonContent = File.ReadAllText(filePath);
                kits = JsonConvert.DeserializeObject<List<Kit>>(jsonContent);

                return kits != null && kits.Count > 0;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Error Reading Kit!\nReason : {ex.Message}", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void customKit2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryLoadKit("Kits/custom_kit2.json", out List<Kit> custom_kit))
            {
                foreach (var itemData in custom_kit)
                {
                    give_item_to_player(GetFromDT(1), itemData.Item, itemData.Amount);
                }
            }
            else
            {
                XtraMessageBox.Show("Failed To Give Custom Kit 2, Have You Created One?", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void customKit3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryLoadKit("Kits/custom_kit3.json", out List<Kit> custom_kit))
            {
                foreach (var itemData in custom_kit)
                {
                    give_item_to_player(GetFromDT(1), itemData.Item, itemData.Amount);
                }
            }
            else
            {
                XtraMessageBox.Show("Failed To Give Custom Kit 3, Have You Created One?", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void simpleButton9_Click(object sender, EventArgs e)
        {
            load_players();
        }

        private void simpleButton10_Click(object sender, EventArgs e)
        {
            ServerConsole.Clear();
        }

        private void simpleButton12_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send("heli.call");
        }
        public void spawn_heli(string name)
        {
            WebSocketsWrapper.Send(string.Format("heli.drop \"{0}\"", name));
        }
        private void simpleButton11_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Settings.InGameName))
            {
                spawn_heli(Settings.InGameName);
            }
            else
                XtraMessageBox.Show("Cant Spawn Attack Helicopter On You As You Have Not Set Your In Game Name!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void spawnHeliToolStripMenuItem_Click(object sender, EventArgs e)
        {
            spawn_heli(GetFromDT(1));
        }

        private void simpleButton14_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send("triggerevent event_cargoship");
        }

        private void simpleButton13_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send("stopevent event_cargoship");
        }

        private void simpleButton16_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send("triggerevent event_airdrop");
        }

        private void simpleButton15_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send("stopevent event_airdrop");
        }

        private void simpleButton18_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send("triggerevent event_cargoheli");
        }

        private void simpleButton17_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send("stopevent event_cargoheli");
        }

        private void simpleButton19_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(addAutoMessage.Text))
            {
                autoMessages.Items.Add(addAutoMessage.Text);
                addAutoMessage.Text = string.Empty;
            }
            else
                XtraMessageBox.Show("Enter A Message To Add!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void simpleButton21_Click(object sender, EventArgs e)
        {
            List<AutoMessage> messageData = new List<AutoMessage>();
            foreach (var listBoxItem in autoMessages.Items)
            {
                messageData.Add(new AutoMessage { Message = listBoxItem.ToString() });
            }
            string jsonContent = JsonConvert.SerializeObject(messageData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("Settings/auto_messages.json", jsonContent);
            save_settings();
            XtraMessageBox.Show("Auto Messages Have Been Saved!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void autoMessagesCheck_Click(object sender, EventArgs e)
        {
            autoMessagesCheck.Checked = !autoMessagesCheck.Checked;
            Settings.AutoMessages = autoMessagesCheck.Checked;
            save_settings();
        }

        private void InGameKillFeedCheck_Click(object sender, EventArgs e)
        {
            InGameKillFeedCheck.Checked = !InGameKillFeedCheck.Checked;
            Settings.InGameKillFeed = InGameKillFeedCheck.Checked;
            save_settings();
        }

        private void DiscordKillFeedCheck_Click(object sender, EventArgs e)
        {
            DiscordKillFeedCheck.Checked = !DiscordKillFeedCheck.Checked;
            Settings.DiscordKillFeed = DiscordKillFeedCheck.Checked;
            save_settings();
        }

        private void InGameEventFeedCheck_Click(object sender, EventArgs e)
        {
            InGameEventFeedCheck.Checked = !InGameEventFeedCheck.Checked;
            Settings.InGameEventFeed = InGameEventFeedCheck.Checked;
            save_settings();
        }

        private void DiscordEventFeedCheck_Click(object sender, EventArgs e)
        {
            DiscordEventFeedCheck.Checked = !DiscordEventFeedCheck.Checked;
            Settings.DiscordEventFeed = DiscordEventFeedCheck.Checked;
            save_settings();
        }

        private void InGameChatCheck_Click(object sender, EventArgs e)
        {
            InGameChatCheck.Checked = !InGameChatCheck.Checked;
            Settings.InGameChat = InGameChatCheck.Checked;
            save_settings();
        }

        private void DiscordChatCheck_Click(object sender, EventArgs e)
        {
            DiscordChatCheck.Checked = !DiscordChatCheck.Checked;
            Settings.DiscordChat = DiscordChatCheck.Checked;
            save_settings();
        }

        private void addToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("VIPID \"{0}\"", GetFromODT(1)));
        }

        private void removeToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveVIP \"{0}\"", GetFromODT(1)));
        }

        private void addToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("ModeratorID \"{0}\"", GetFromODT(1)));
        }

        private void removeToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveModerator \"{0}\"", GetFromODT(1)));
        }

        private void addToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("AdminID \"{0}\"", GetFromODT(1)));
        }

        private void removeToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveAdmin \"{0}\"", GetFromODT(1)));
        }

        private void addToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("OwnerID \"{0}\"", GetFromODT(1)));
        }

        private void removeToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("RemoveOwner \"{0}\"", GetFromODT(1)));
        }

        private void banToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("global.banid \"{0}\"", GetFromODT(1)));
        }

        private void uUnbanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("global.unban \"{0}\"", GetFromODT(1)));
        }

        [STAThread]
        private void copyNameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyFromODT(1);
            XtraMessageBox.Show(string.Format("Gamertag {0} Has Been Copied To Your Clipboard!", GetFromODT(1)), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void simpleButton22_Click(object sender, EventArgs e)
        {
            string origval = simpleButton22.Text;
            if (string.IsNullOrEmpty(Settings.InGameName))
            {
                XtraMessageBox.Show("Can't Spawn A Locked Crate On You As You Have Not Set Your In-Game Name!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            WebSocketsWrapper.Send(string.Format("printpos \"{0}\"", Settings.InGameName));
            simpleButton22.Text = "Spawning";
            await Task.Delay(1000);
            string pos = ServerConsole.ReadLastFewLines();
            if (ServerConsole.IsValidPrintPos(pos))
            {
                WebSocketsWrapper.Send(string.Format("spawn codelocked \"{0}\"", pos.Replace(" ", "")));
                simpleButton22.Text = "Spawned!";
            }
            else
            {
                XtraMessageBox.Show("Failed To Find Your Position, Try Again!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            simpleButton22.Text = origval;
        }
        private void simpleButton20_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send("puzzlereset");
        }
        private void SaveCratePosition(string jsonPath, string groupName, LockedCrate position)
        {
            List<LockedCrateGroup> crateGroups;
            if (File.Exists(jsonPath))
            {
                crateGroups = JsonConvert.DeserializeObject<List<LockedCrateGroup>>(File.ReadAllText(jsonPath));
            }
            else
            {
                crateGroups = new List<LockedCrateGroup>();
            }
            var existingGroup = crateGroups.Find(group => group.Name == groupName);
            if (existingGroup != null)
            {
                existingGroup.Positions.Add(position);
            }
            else
            {
                var newGroup = new LockedCrateGroup
                {
                    Name = groupName,
                    Positions = new List<LockedCrate> { position }
                };
                crateGroups.Add(newGroup);
            }
            string jsonContent = JsonConvert.SerializeObject(crateGroups, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(jsonPath, jsonContent);
        }

        private void AddCratePosition(string groupName, LockedCrate newPosition)
        {
            var existingGroup = crateGroups.Find(group => group.Name == groupName);

            if (existingGroup != null)
            {
                existingGroup.Positions.Add(newPosition);
            }
            else
            {
                var newGroup = new LockedCrateGroup
                {
                    Name = groupName,
                    Positions = new List<LockedCrate> { newPosition }
                };
                crateGroups.Add(newGroup);
            }
            if (!lockedCrateGroupName.Properties.Items.Contains(groupName))
            {
                lockedCrateGroupName.Properties.Items.Add(groupName);
            }
            lockedCrateGroupName.SelectedItem = groupName;
        }

        private void simpleButton24_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(newLocationName.Text) || string.IsNullOrEmpty(lockedCratePos.Text))
            {
                XtraMessageBox.Show("Please Fill Out All The Fields!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string[] parts = lockedCratePos.Text.Replace("(", "").Replace(")", "").Split(',');
            LockedCrate newPosition = new LockedCrate { X = parts[0], Y = parts[1], Z = parts[2] };
            SaveCratePosition("Events/locked_crate_event.json", newLocationName.Text, newPosition);
            AddCratePosition(newLocationName.Text, newPosition);
            UpdateLockedCratePositions();
        }

        private void simpleButton25_Click(object sender, EventArgs e)
        {
            if (lockedCrateGroupName.SelectedIndex >= 0)
            {
                var selectedGroupName = lockedCrateGroupName.SelectedItem.ToString();
                SpawnCrates(selectedGroupName);
            }
            else
            {
                XtraMessageBox.Show("Failed To Spawn Crates, Have You Selected A Location?", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void LockedCratePositions_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (LockedCratePositions.SelectedItem != null && lockedCrateGroupName.SelectedItem != null)
                {
                    string selectedPosition = LockedCratePositions.SelectedItem.ToString();
                    string groupName = lockedCrateGroupName.SelectedItem.ToString();

                    var positionParts = selectedPosition.Split(new[] { '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (positionParts.Length == 3)
                    {
                        string x = positionParts[0].Trim();
                        string y = positionParts[1].Trim();
                        string z = positionParts[2].Trim();
                        var existingGroup = crateGroups.Find(group => group.Name == groupName);
                        if (existingGroup != null)
                        {
                            var positionToRemove = existingGroup.Positions.FirstOrDefault(position =>
                                Convert.ToDouble(position.X) == Convert.ToDouble(x) &&
                                Convert.ToDouble(position.Y) == Convert.ToDouble(y) &&
                                Convert.ToDouble(position.Z) == Convert.ToDouble(z));
                            if (positionToRemove != null)
                            {
                                existingGroup.Positions.Remove(positionToRemove);
                                if (existingGroup.Positions.Count == 0)
                                {
                                    crateGroups.Remove(existingGroup);
                                }
                                string jsonContent = JsonConvert.SerializeObject(crateGroups, Newtonsoft.Json.Formatting.Indented);
                                File.WriteAllText("Events/locked_crate_event.json", jsonContent);
                                LockedCratePositions.Items.Remove(LockedCratePositions.SelectedItem);
                            }
                        }
                    }
                }
            }
        }
        private void trackBarControl1_Properties_ValueChanged(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("env.time \"{0}\"", trackBarControl1.Value));
        }
        private List<LockedCrateGroup> crateGroups;
        private void UpdateLockedCratePositions()
        {
            LockedCratePositions.Items.Clear();
            if (lockedCrateGroupName.SelectedIndex >= 0)
            {
                var selectedGroupName = lockedCrateGroupName.SelectedItem.ToString();
                var selectedGroup = crateGroups.Find(group => group.Name == selectedGroupName);
                if (selectedGroup != null)
                {
                    foreach (var cratePosition in selectedGroup.Positions)
                    {
                        LockedCratePositions.Items.Add($"({cratePosition.X}, {cratePosition.Y}, {cratePosition.Z})");
                    }
                }
            }
        }
        private void lockedCrateGroupName_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLockedCratePositions();
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("entity.deleteby \"{0}\"", GetFromDT(1)));
        }

        private void deleteAllEntitiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("entity.deleteby \"{0}\"", GetFromODT(1)));
        }
        static string[] FormatXYZString(string input)
        {
            string[] values = input.Trim('(', ')').Split(',');

            if (values.Length == 3)
            {
                double X = double.Parse(values[0].Trim());
                double Y = double.Parse(values[1].Trim());
                double Z = double.Parse(values[2].Trim());
                return new string[] { X.ToString(), Y.ToString(), Z.ToString() };
            }
            else
            {
                return null;
            }
        }
        private async void simpleButton23_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(newLocationName.Text))
            {
                XtraMessageBox.Show("Please Fill Out All The Fields!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            WebSocketsWrapper.Send(string.Format("printpos \"{0}\"", Settings.InGameName));
            await Task.Delay(1000);
            string pos = ServerConsole.ReadLastFewLines();
            if (ServerConsole.IsValidPrintPos(pos))
            {

                string[] parts = FormatXYZString(pos);
                LockedCrate newPosition = new LockedCrate { X = parts[0], Y = parts[1], Z = parts[2] };
                SaveCratePosition("Events/locked_crate_event.json", newLocationName.Text, newPosition);
                AddCratePosition(newLocationName.Text, newPosition);
                UpdateLockedCratePositions();
            }
            else
            {
                XtraMessageBox.Show("Failed To Find Your Position, Try Again!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void killPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("global.injure \"{0}\"", GetFromDT(1)));
        }
        public async Task check_update()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "AutoUpdateApp");
            HttpResponseMessage response = await client.GetAsync($"https://api.github.com/repos/KyleFardy/RCE-Admin/releases/latest");

            if (response.IsSuccessStatusCode)
            {
                JObject releaseInfo = JObject.Parse(await response.Content.ReadAsStringAsync());
                string latestVersion = releaseInfo["tag_name"].ToString();
                if (latestVersion != Settings.Version)
                {
                    XtraMessageBox.Show($"There Is An Update Available\n\nCurrent Version : {Settings.Version}\nNew Version : {latestVersion}\n\nStarting Download Now!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    using (var downloadClient = new HttpClient())
                    {

                        string downloadUrl = releaseInfo["assets"][0]["browser_download_url"].ToString();
                        var downloadResponse = await downloadClient.GetAsync(downloadUrl);
                        if (downloadResponse.IsSuccessStatusCode)
                        {
                            string filePath = "RCE Admin Setup.exe";
                            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await downloadResponse.Content.CopyToAsync(fileStream);
                            }
                            XtraMessageBox.Show("Update Successfully Downloaded, Please Follow The Setup Installer!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });
                            Process.GetCurrentProcess().Kill();
                        }
                        else
                        {
                            XtraMessageBox.Show("Failed To Download Update, Please Download From Github!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Process.Start(downloadUrl);
                        }
                    }
                }
            }
        }
        private async void simpleButton26_Click(object sender, EventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "AutoUpdateApp");
            HttpResponseMessage response = await client.GetAsync($"https://api.github.com/repos/KyleFardy/RCE-Admin/releases/latest");

            if (response.IsSuccessStatusCode)
            {
                JObject releaseInfo = JObject.Parse(await response.Content.ReadAsStringAsync());
                string latestVersion = releaseInfo["tag_name"].ToString();
                if (latestVersion != Settings.Version)
                {
                    XtraMessageBox.Show($"There Is An Update Available\n\nCurrent Version : {Settings.Version}\nNew Version : {latestVersion}\n\nStarting Download Now!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    using (var downloadClient = new HttpClient())
                    {

                        string downloadUrl = releaseInfo["assets"][0]["browser_download_url"].ToString();
                        var downloadResponse = await downloadClient.GetAsync(downloadUrl);
                        if (downloadResponse.IsSuccessStatusCode)
                        {
                            string filePath = "RCE Admin Setup.exe";
                            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await downloadResponse.Content.CopyToAsync(fileStream);
                            }
                            XtraMessageBox.Show("Update Successfully Downloaded, Please Follow The Setup Installer!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });
                            Process.GetCurrentProcess().Kill();
                        }
                        else
                        {
                            XtraMessageBox.Show("Failed To Download Update, Please Download From Github!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Process.Start(downloadUrl);
                        }
                    }
                }
                else
                {
                    XtraMessageBox.Show("You Are Using The Latest Version!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                XtraMessageBox.Show("Failed To Check If There Is Update Available!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void simpleButton27_Click(object sender, EventArgs e)
        {
            save_settings("#cc402a");
            ChangeAccentPaintColor(HexToColor("#cc402a"));
        }
        private void colorPickEdit1_EditValueChanged(object sender, EventArgs e)
        {
            Color new_theme = HexToColor(ColorTranslator.ToHtml((Color)colorPickEdit1.EditValue));
            groupControl21.AppearanceCaption.BorderColor = new_theme;
        }

        private void simpleButton28_Click(object sender, EventArgs e)
        {
            save_settings(ColorTranslator.ToHtml((Color)colorPickEdit1.EditValue));
            ChangeAccentPaintColor(HexToColor(ColorTranslator.ToHtml((Color)colorPickEdit1.EditValue)));
        }
        private class MyRenderer : ToolStripProfessionalRenderer
        {
            private Color hoverColor = HexToColor(Settings.Theme);

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected)
                {
                    using (SolidBrush brush = new SolidBrush(hoverColor))
                    {
                        e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
                    }
                }
                else
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }
            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                if (e.Item.Selected)
                {
                    using (SolidBrush brush = new SolidBrush(hoverColor))
                    {
                        e.Graphics.FillRectangle(brush, e.ImageRectangle);
                    }
                }
                else
                {
                    base.OnRenderItemCheck(e);
                }
            }
        }
        private void contextMenuStrip1_Opened(object sender, EventArgs e)
        {
            contextMenuStrip1.Renderer = new MyRenderer();
        }

        private void contextMenuStrip2_Opened(object sender, EventArgs e)
        {
            contextMenuStrip2.Renderer = new MyRenderer();
        }
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("global.unban \"{0}\"", GetFromBDT(1)));
        }

        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("entity.deleteby \"{0}\"", GetFromBDT(1)));
        }

        private void contextMenuStrip3_Opened(object sender, EventArgs e)
        {
            contextMenuStrip3.Renderer = new MyRenderer();
        }

        private async void simpleButton31_Click(object sender, EventArgs e)
        {
            var selectedPlayers = new List<CheckedListBoxItem>(Form1.CratePlayers.CheckedItems.Cast<CheckedListBoxItem>());
            if (selectedPlayers.Count == 0)
            {
                XtraMessageBox.Show("Select A Player First!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string crate_type;
            switch (spawnCrateType.SelectedItem)
            {
                case "Locked Crate":
                    crate_type = "codelocked";
                    break;
                case "Elite Crate":
                    crate_type = "crate_elite";
                    break;
                case "Heli Crate":
                    crate_type = "heli_crate";
                    break;
                case "Brad Crate":
                    crate_type = "bradley_crate";
                    break;
                case "Basic Crate":
                    crate_type = "crate_basic";
                    break;
                case "Normal Crate":
                    crate_type = "crate_normal";
                    break;
                case "Normal Crate 2":
                    crate_type = "crate_normal2";
                    break;
                case "Normal Crate 2 Food":
                    crate_type = "crate_normal2_food";
                    break;
                case "Normal Crate 2 Medical":
                    crate_type = "crate_normal2_medical";
                    break;
                case "Tool Crate":
                    crate_type = "crate_tools";
                    break;
                case "Advanced Underwater Crate":
                    crate_type = "crate_underwater_advanced";
                    break;
                case "Basic Underwater Crate":
                    crate_type = "crate_underwater_basic";
                    break;
                case "Loot Barrel 1":
                    crate_type = "loot-barrel-1";
                    break;
                case "Loot Barrel Alt 1":
                    crate_type = "loot_barrel_1";
                    break;
                case "Loot Barrel 2":
                    crate_type = "loot-barrel-2";
                    break;
                case "Loot Barrel Alt 2":
                    crate_type = "loot_barrel_2";
                    break;
                case "":
                case "Select A Crate":
                default:
                    XtraMessageBox.Show("Please Select A Crate Type!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }
            foreach (CheckedListBoxItem player in selectedPlayers)
            {
                WebSocketsWrapper.Send(string.Format("printpos \"{0}\"", player.Value.ToString()));
                await Task.Delay(1000);
                string pos = ServerConsole.ReadLastFewLines();

                if (ServerConsole.IsValidPrintPos(pos))
                {
                    string[] position = FormatXYZString(pos);
                    WebSocketsWrapper.Send($"spawn {crate_type} {position[0]},{position[1]},{position[2]}");
                    Form1.CratePlayers.SetItemChecked(Form1.CratePlayers.Items.IndexOf(player), false);
                    spawnAnimalType.Text = "Select A Crate Type";
                }
                else
                {
                    XtraMessageBox.Show(string.Format("Failed To Find {0}'s Position, Try Again!", player.Value.ToString()), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            XtraMessageBox.Show("All Crates Have Been Spawned!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void simpleButton30_Click(object sender, EventArgs e)
        {
            var selectedPlayers = new List<CheckedListBoxItem>(Form1.AnimalPlayers.CheckedItems.Cast<CheckedListBoxItem>());
            if (selectedPlayers.Count == 0)
            {
                XtraMessageBox.Show("Select A Player First!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string animal_type;
            switch (spawnAnimalType.SelectedItem)
            {
                case "Pig":
                    animal_type = "boar";
                    break;
                case "Bear":
                    animal_type = "bear";
                    break;
                case "Horse":
                    animal_type = "testridablehorse";
                    break;
                case "Shark":
                    animal_type = "simpleshark";
                    break;
                case "Chicken":
                    animal_type = "chicken";
                    break;
                case "Deer":
                    animal_type = "stag";
                    break;
                case "Wolf":
                    animal_type = "wolf";
                    break;
                case "":
                case "Select An Animal":
                default:
                    XtraMessageBox.Show("Please Select An Animal!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }
            foreach (CheckedListBoxItem player in selectedPlayers)
            {
                WebSocketsWrapper.Send(string.Format("printpos \"{0}\"", player.Value.ToString()));
                await Task.Delay(1000);
                string pos = ServerConsole.ReadLastFewLines();

                if (ServerConsole.IsValidPrintPos(pos))
                {
                    string[] position = FormatXYZString(pos);
                    WebSocketsWrapper.Send($"spawn {animal_type} {position[0]},{position[1]},{position[2]}");
                    Form1.AnimalPlayers.SetItemChecked(Form1.AnimalPlayers.Items.IndexOf(player), false);
                    spawnAnimalType.Text = "Select An Animal";
                }
                else
                {
                    XtraMessageBox.Show(string.Format("Failed To Find {0}'s Position, Try Again!", player.Value.ToString()), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            XtraMessageBox.Show("All Crates Have Been Spawned!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
