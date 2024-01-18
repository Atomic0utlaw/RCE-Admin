using System;
using System.Windows.Forms;
using System.Collections.Generic;

using RCE_ADMIN.WebSockets.CustomPackets;
using static System.Net.Mime.MediaTypeNames;
using DevExpress.XtraEditors.SyntaxEditor;
using Newtonsoft.Json;

namespace RCE_ADMIN.Interface
{
    public static class PlayerDataTable
    {
        public static void Update(List<Player> list)
        {
            if (Form1.Players.InvokeRequired)
            {
                Form1.Players.Invoke(new MethodInvoker(delegate {
                    Form1.Players.Rows.Clear();
                }));
            }
            else
            {
                Form1.Players.Rows.Clear();
            };

            for (var i = 0; i < list.Count; i++)
            {
                var player = list[i];
                TimeSpan timeSpan = TimeSpan.FromSeconds(player.ConnectedSeconds);
                AddNewEntry(i + 1, player.DisplayName, (int)player.Health, player.Address, player.Ping, string.Format("{0} Hour(s), {1} Min(s), {2} Sec(s)", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));
            }
            if (Form1.selectedPlayer >= 0 && Form1.selectedPlayer < Form1.Players.Rows.Count)
            {
                Form1.Players.Rows[Form1.selectedPlayer].Selected = true;
            }
        }
        public static void AddNewEntry(int number,  string playerName, int health, string address, int ping, string timeConnected)
        {
            var row = new DataGridViewRow();
            row.CreateCells(Form1.Players, number, playerName, health+"/100", ping+" ms", (string.IsNullOrEmpty(address) ? "N/A" : address), timeConnected);

            if (Form1.Players.InvokeRequired)
            {
                Form1.Players.Invoke(new MethodInvoker(delegate {
                    Form1.Players.Rows.Add(row);
                }));
            }
            else
            {
                Form1.Players.Rows.Add(row);
            };
        }
    }
}
