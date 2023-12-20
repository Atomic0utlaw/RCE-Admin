using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCE_ADMIN.Callbacks;

namespace RCE_ADMIN.WebSockets
{
    public static class Listener
    {
        public static Dictionary<int, string> Listeners = new Dictionary<int, string>();
        public static List<string> NeedListener = new List<string>()
        {
            "playerlist"
        };
        public static void ProcessMessage(Packet packet)
        {
            var command = Listeners[packet.Identifier];
            Listeners.Remove(packet.Identifier);

            switch (command.ToLower())
            {
                case "playerlist":
                    PlayerList.UpdatePlayers(packet.Message);
                    break;
            }
        }
    }
}
