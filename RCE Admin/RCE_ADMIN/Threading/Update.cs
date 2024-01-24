using System;
using System.Runtime.InteropServices;
using System.Threading;

using RCE_ADMIN.WebSockets;

namespace RCE_ADMIN.Threading
{
    public static class Update
    {
        public static Thread PlayerThread;
        public static Thread InfoThread;
        public static void StartThreads()
        {
            if (PlayerThread == null || !PlayerThread.IsAlive)
            {
                PlayerThread = new Thread(new ThreadStart(UpdatePlayers));
                PlayerThread.Start();
            }
            if (InfoThread == null || !InfoThread.IsAlive)
            {
                InfoThread = new Thread(new ThreadStart(UpdateInfo));
                InfoThread.Start();
            }
        }
        public static void StopThreads()
        {
            if (PlayerThread == null)
                return;
            PlayerThread.Abort();


            if (InfoThread == null)
                return;
            InfoThread.Abort();
        }
        public static void UpdatePlayers()
        {
            while (WebSocketsWrapper.IsConnected())
            {
                WebSocketsWrapper.SendCommand("playerlist");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
        public static void UpdateInfo()
        {
            while (WebSocketsWrapper.IsConnected())
            {
                WebSocketsWrapper.SendCommand("serverinfo");
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }
    }
}
