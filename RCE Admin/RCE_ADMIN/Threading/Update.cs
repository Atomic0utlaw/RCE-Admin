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
        public static Thread BansThread;
        public static Thread BradThread;
        public static Thread HeliThread;
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
            if (BansThread == null || !BansThread.IsAlive)
            {
                BansThread = new Thread(new ThreadStart(UpdateBans));
                BansThread.Start();
            }
            if (BradThread == null || !BradThread.IsAlive)
            {
                BradThread = new Thread(new ThreadStart(CheckBrad));
                BradThread.Start();
            }
            if (HeliThread == null || !HeliThread.IsAlive)
            {
                HeliThread = new Thread(new ThreadStart(CheckHeli));
                HeliThread.Start();
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


            if (BansThread == null)
                return;
            BansThread.Abort();


            if (BradThread == null)
                return;
            BradThread.Abort();


            if (HeliThread == null)
                return;
            HeliThread.Abort();
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
        public static void UpdateBans()
        {
            while (WebSocketsWrapper.IsConnected())
            {
                WebSocketsWrapper.SendCommand("banlist");
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }
        public static void CheckBrad()
        {
            while (WebSocketsWrapper.IsConnected())
            {
                WebSocketsWrapper.SendCommand("find_entity bradley");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
        public static void CheckHeli()
        {
            while (WebSocketsWrapper.IsConnected())
            {
                WebSocketsWrapper.SendCommand("find_entity heli");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }
}
