using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using WebSocketSharp;
using RCE_ADMIN.Interface;
using RCE_ADMIN.Threading;
using DevExpress.XtraEditors;

namespace RCE_ADMIN.WebSockets
{
    public static class WebSocketsWrapper
    {
        private static WebSocket webSocket;
        private static Random random;
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
