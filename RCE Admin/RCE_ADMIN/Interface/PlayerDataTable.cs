using System;
using System.Windows.Forms;
using System.Collections.Generic;

using RCE_ADMIN.WebSockets.CustomPackets;
using static System.Net.Mime.MediaTypeNames;
using DevExpress.XtraEditors.SyntaxEditor;
using Newtonsoft.Json;
using RCE_ADMIN.Callbacks;
using System.Linq;

namespace RCE_ADMIN.Interface
{
    public static class PlayerDataTable
    {
        public static void Update(List<Player> list)
        {
            int selectedRowIndex = -1;
            if (Form1.Players.InvokeRequired)
            {
                Form1.Players.Invoke(new MethodInvoker(delegate {
                    selectedRowIndex = Form1.Players.SelectedRows.Count > 0 ? Form1.Players.SelectedRows[0].Index : -1;
                }));
            }
            else
            {
                selectedRowIndex = Form1.Players.SelectedRows.Count > 0 ? Form1.Players.SelectedRows[0].Index : -1;
            }
            RemoveDisconnectedPlayers(list);
            foreach (var player in list)
            {
                var playerRepository = new PlayerDatabase();
                playerRepository.SavePlayer(player);

                int rowIndex = FindRowIndexByDisplayName(player.DisplayName);

                if (rowIndex == -1)
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(player.ConnectedSeconds);
                    AddNewEntry(Form1.Players.Rows.Count + 1, player.DisplayName, (int)player.Health, player.Address, player.Ping, string.Format("{0} Hour(s), {1} Min(s), {2} Sec(s)", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));
                }
                else
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(player.ConnectedSeconds);
                    UpdateRow(rowIndex, player.DisplayName, (int)player.Health, player.Address, player.Ping, string.Format("{0} Hour(s), {1} Min(s), {2} Sec(s)", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));
                }
            }
            if (Form1.Players.Rows.Count > 0)
            {
                DataGridViewRow lastRow = Form1.Players.Rows[Form1.Players.Rows.Count - 1];

                if (!lastRow.IsNewRow && !Form1.Players.IsCurrentCellInEditMode)
                {
                    if (Form1.Players.InvokeRequired)
                    {
                        Form1.Players.Invoke(new MethodInvoker(delegate {
                            Form1.Players.Rows.RemoveAt(Form1.Players.Rows.Count - 1);
                        }));
                    }
                    else
                    {
                        Form1.Players.Rows.RemoveAt(Form1.Players.Rows.Count - 1);
                    }
                }
            }
            if (selectedRowIndex >= 0 && selectedRowIndex < Form1.Players.Rows.Count)
            {
                if (Form1.Players.InvokeRequired)
                {
                    Form1.Players.Invoke(new MethodInvoker(delegate {
                        Form1.Players.Rows[selectedRowIndex].Selected = true;
                    }));
                }
                else
                {
                    Form1.Players.Rows[selectedRowIndex].Selected = true;
                }
            }
        }
        private static void RemoveDisconnectedPlayers(List<Player> updatedList)
        {
            List<string> connectedPlayers = updatedList.Select(player => player.DisplayName).ToList();
            foreach (DataGridViewRow row in Form1.Players.Rows)
            {
                if (row.Cells["PlayerName"].Value != null)
                {
                    string displayName = row.Cells["PlayerName"].Value.ToString();

                    if (!connectedPlayers.Contains(displayName))
                    {
                        if (Form1.Players.InvokeRequired)
                        {
                            Form1.Players.Invoke(new MethodInvoker(delegate {
                                Form1.Players.Rows.Remove(row);
                            }));
                        }
                        else
                        {
                            Form1.Players.Rows.Remove(row);
                        }
                    }
                }
            }
        }
        private static int FindRowIndexByDisplayName(string displayName)
        {
            foreach (DataGridViewRow row in Form1.Players.Rows)
            {
                if (row.Cells["PlayerName"].Value != null && row.Cells["PlayerName"].Value.ToString() == displayName)
                {
                    return row.Index;
                }
            }
            return -1;
        }
        private static void UpdateRow(int rowIndex, string displayName, int health, string address, int ping, string formattedTime)
        {
            if (Form1.Players.InvokeRequired)
            {
                Form1.Players.Invoke(new MethodInvoker(delegate {
                    Form1.Players.Rows[rowIndex].SetValues(rowIndex + 1, displayName, health + "/100", ping + " ms", (string.IsNullOrEmpty(address) ? "N/A" : address), formattedTime);
                }));
            }
            else
            {
                Form1.Players.Rows[rowIndex].SetValues(rowIndex + 1, displayName, health + "/100", ping + " ms", (string.IsNullOrEmpty(address) ? "N/A" : address), formattedTime);
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
