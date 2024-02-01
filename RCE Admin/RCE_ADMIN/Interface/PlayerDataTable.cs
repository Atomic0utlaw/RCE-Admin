using System;
using System.Windows.Forms;
using System.Collections.Generic;

using RCE_ADMIN.WebSockets.CustomPackets;
using static System.Net.Mime.MediaTypeNames;
using DevExpress.XtraEditors.SyntaxEditor;
using Newtonsoft.Json;
using RCE_ADMIN.Callbacks;
using System.Linq;
using static DevExpress.Utils.Drawing.Helpers.NativeMethods;

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
            RemoveDisconnectedPlayersFromCheckedListBox(list);
            List<string> currentItems = new List<string>();

            if (Form1.CratePlayers.InvokeRequired)
            {
                Form1.CratePlayers.Invoke(new MethodInvoker(delegate {
                    foreach (DevExpress.XtraEditors.Controls.CheckedListBoxItem checkedItem in Form1.CratePlayers.Items)
                    {
                        currentItems.Add(checkedItem.Value.ToString());
                    }
                }));
            }
            else
            {
                foreach (DevExpress.XtraEditors.Controls.CheckedListBoxItem checkedItem in Form1.CratePlayers.Items)
                {
                    currentItems.Add(checkedItem.Value.ToString());
                }
            }
            if (Form1.AnimalPlayers.InvokeRequired)
            {
                Form1.AnimalPlayers.Invoke(new MethodInvoker(delegate {
                    foreach (DevExpress.XtraEditors.Controls.CheckedListBoxItem checkedItem in Form1.AnimalPlayers.Items)
                    {
                        currentItems.Add(checkedItem.Value.ToString());
                    }
                }));
            }
            else
            {
                foreach (DevExpress.XtraEditors.Controls.CheckedListBoxItem checkedItem in Form1.AnimalPlayers.Items)
                {
                    currentItems.Add(checkedItem.Value.ToString());
                }
            }
            foreach (var player in list)
            {
                var playerRepository = new PlayerDatabase();
                playerRepository.SavePlayer(player);

                int rowIndex = FindRowIndexByDisplayName(player.DisplayName);

                if (rowIndex == -1)
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(player.ConnectedSeconds);
                    AddNewEntry(Form1.Players.Rows.Count + 1, player.DisplayName, (int)player.Health, player.Ping, string.Format("{0} Hour(s), {1} Min(s), {2} Sec(s)", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));
                }
                else
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(player.ConnectedSeconds);
                    UpdateRow(rowIndex, player.DisplayName, (int)player.Health, player.Ping, string.Format("{0} Hour(s), {1} Min(s), {2} Sec(s)", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));
                }
                if (!currentItems.Contains(player.DisplayName))
                {
                    if (Form1.CratePlayers.InvokeRequired)
                    {
                        Form1.CratePlayers.Invoke(new MethodInvoker(delegate {
                            Form1.CratePlayers.Items.Add(new DevExpress.XtraEditors.Controls.CheckedListBoxItem(player.DisplayName));
                        }));
                    }
                    else
                    {
                        Form1.CratePlayers.Items.Add(new DevExpress.XtraEditors.Controls.CheckedListBoxItem(player.DisplayName));
                    }
                    if (Form1.AnimalPlayers.InvokeRequired)
                    {
                        Form1.AnimalPlayers.Invoke(new MethodInvoker(delegate {
                            Form1.AnimalPlayers.Items.Add(new DevExpress.XtraEditors.Controls.CheckedListBoxItem(player.DisplayName));
                        }));
                    }
                    else
                    {
                        Form1.AnimalPlayers.Items.Add(new DevExpress.XtraEditors.Controls.CheckedListBoxItem(player.DisplayName));
                    }
                }
            }
            foreach (DevExpress.XtraEditors.Controls.CheckedListBoxItem item in Form1.CratePlayers.Items)
            {
                string itemName = item.Value.ToString();
                if (!list.Any(player => player.DisplayName == itemName))
                {
                    if (Form1.CratePlayers.InvokeRequired)
                    {
                        Form1.CratePlayers.Invoke(new MethodInvoker(delegate {
                            Form1.CratePlayers.Items.Remove(item);
                        }));
                    }
                    else
                    {
                        Form1.CratePlayers.Items.Remove(item);
                    }
                }
            }
            foreach (DevExpress.XtraEditors.Controls.CheckedListBoxItem item in Form1.AnimalPlayers.Items)
            {
                string itemName = item.Value.ToString();
                if (!list.Any(player => player.DisplayName == itemName))
                {
                    if (Form1.AnimalPlayers.InvokeRequired)
                    {
                        Form1.AnimalPlayers.Invoke(new MethodInvoker(delegate {
                            Form1.AnimalPlayers.Items.Remove(item);
                        }));
                    }
                    else
                    {
                        Form1.AnimalPlayers.Items.Remove(item);
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
        private static void RemoveDisconnectedPlayersFromCheckedListBox(List<Player> updatedList)
        {
            List<string> connectedPlayers = updatedList.Select(player => player.DisplayName).ToList();

            for (int i = Form1.CratePlayers.Items.Count - 1; i >= 0; i--)
            {
                string displayName = Form1.CratePlayers.Items[i].ToString();

                if (!connectedPlayers.Contains(displayName))
                {
                    if (Form1.CratePlayers.InvokeRequired)
                    {
                        Form1.CratePlayers.Invoke(new MethodInvoker(delegate {
                            Form1.CratePlayers.Items.RemoveAt(i);
                        }));
                    }
                    else
                    {
                        Form1.CratePlayers.Items.RemoveAt(i);
                    }
                }
            }
            for (int i = Form1.AnimalPlayers.Items.Count - 1; i >= 0; i--)
            {
                string displayName = Form1.AnimalPlayers.Items[i].ToString();

                if (!connectedPlayers.Contains(displayName))
                {
                    if (Form1.AnimalPlayers.InvokeRequired)
                    {
                        Form1.AnimalPlayers.Invoke(new MethodInvoker(delegate {
                            Form1.AnimalPlayers.Items.RemoveAt(i);
                        }));
                    }
                    else
                    {
                        Form1.AnimalPlayers.Items.RemoveAt(i);
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
        private static void UpdateRow(int rowIndex, string displayName, int health, int ping, string formattedTime)
        {
            if (Form1.Players.InvokeRequired)
            {
                Form1.Players.Invoke(new MethodInvoker(delegate {
                    Form1.Players.Rows[rowIndex].SetValues(rowIndex + 1, displayName, health + "/100", ping + " ms", formattedTime);
                }));
            }
            else
            {
                Form1.Players.Rows[rowIndex].SetValues(rowIndex + 1, displayName, health + "/100", ping + " ms", formattedTime);
            }
        }
        public static void AddNewEntry(int number,  string playerName, int health, int ping, string timeConnected)
        {
            var row = new DataGridViewRow();
            row.CreateCells(Form1.Players, number, playerName, health+"/100", ping+" ms", timeConnected);
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
