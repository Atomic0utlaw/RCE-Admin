using System.Collections.Generic;
using Newtonsoft.Json;
using RCE_ADMIN.Interface;
using RCE_ADMIN.WebSockets.CustomPackets;

namespace RCE_ADMIN.Callbacks
{
    public static class PlayerList
    {
        public static List<Player> CurrentPlayers;
        public static void UpdatePlayers(string list)
        {
            if (!list.Contains("realm"))
            {
                CurrentPlayers = JsonConvert.DeserializeObject<List<Player>>(list);
                PlayerCounter.SetText(CurrentPlayers.Count);
                PlayerDataTable.Update(CurrentPlayers);
            }
        }
    }
}
