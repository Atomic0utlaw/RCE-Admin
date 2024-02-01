using System.Collections.Generic;
using Newtonsoft.Json;
using RCE_ADMIN.Interface;
using RCE_ADMIN.WebSockets.CustomPackets;

namespace RCE_ADMIN.Callbacks
{
    public static class BanList
    {
        public static List<Ban> CuurentBans;
        public static void UpdateBans(string list)
        {
            CuurentBans = JsonConvert.DeserializeObject<List<Ban>>(list);
            BanDataTable.Update(CuurentBans);
        }
    }
}
