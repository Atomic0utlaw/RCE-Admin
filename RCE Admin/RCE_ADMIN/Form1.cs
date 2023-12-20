using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress;
using RCE_ADMIN.Interface;
using RCE_ADMIN.WebSockets;
using System.Diagnostics;

namespace RCE_ADMIN
{
    public partial class Form1 : XtraForm
    {
        public static Settings Settings;
        public static BarStaticItem Status;
        public static RichTextBox Console;
        public static BarStaticItem Counter;
        public static DataGridView Players;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Settings = Settings.Read();
            Status = toolStripStatusLabelRight;
            Console = richTextBoxConsole;
            Counter = toolStripStatusLabelCounter;
            Players = dataGridViewPlayers;
            textBoxAddress.Text = Settings.ServerAddress;
            textBoxPort.Text = Settings.ServerPort;
            textBoxPassword.Text = Settings.ServerPassword;
            ServerConsole.Disable();
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
                XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
        public string GetFromDT(int i)
        {
            try
            {
                if (Players.Rows[Players.CurrentRow.Index].Cells[i].Value.ToString() != "")
                {
                    return Players.Rows[Players.CurrentRow.Index].Cells[i].Value.ToString();
                }
                else
                {
                    XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return null;
                }
            }
            catch (NullReferenceException)
            {
                XtraMessageBox.Show("Refresh The List To Obtain The Clients Information!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
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
        private void buttonSave_Click(object sender, EventArgs e)
        {
            Settings.Write(new Settings(textBoxAddress.Text, textBoxPort.Text, textBoxPassword.Text));
            Settings = Settings.Read();
        }
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Connect();
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
            WebSocketsWrapper.Send($"global.say {textBoxBroadcast.Text}");
            textBoxBroadcast.Text = "";
        }
        private void textBoxCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((ConsoleKey)e.KeyChar == ConsoleKey.Enter)
                WebSocketsWrapper.SendCommand(textBoxCommand.Text);
            textBoxBroadcast.Text = "";
        }
        private void textBoxBroadcast_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((ConsoleKey)e.KeyChar == ConsoleKey.Enter)
                WebSocketsWrapper.Send($"global.say {textBoxBroadcast.Text}");
            textBoxBroadcast.Text = "";
        }
        private void buttonClear_Click(object sender, EventArgs e)
        {
            ServerConsole.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void copyNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyFromDT(1);
            XtraMessageBox.Show(string.Format("Gamertag {0} Has Been Copied To Your Clipboard!", GetFromDT(1)), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void kickPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("kick [0]", GetFromDT(1)));
        }

        private void banPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebSocketsWrapper.Send(string.Format("banid [0]", GetFromDT(1)));
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            Process.Start("https://prnt.sc/Qc-uU0PHiPx8");
            XtraMessageBox.Show(string.Format("Go To Your Servers Console On GPortal{0}Press CTRL, SHIFT & I To Open Inspect Element{0}Click On The ngapi/ Requests Until You Find The One With The Sameish Output At The Screenshot We Just Opened{0}Follow The Screenshot For The RCON Password!", Environment.NewLine), "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
    }
}
