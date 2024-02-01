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
    public static class BanDataTable
    {
        public static void Update(List<Ban> list)
        {

            int selectedRowIndex = -1;
            if (Form1.Bans.InvokeRequired)
            {
                Form1.Bans.Invoke(new MethodInvoker(delegate {
                    selectedRowIndex = Form1.Bans.SelectedRows.Count > 0 ? Form1.Bans.SelectedRows[0].Index : -1;
                }));
            }
            else
            {
                selectedRowIndex = Form1.Bans.SelectedRows.Count > 0 ? Form1.Bans.SelectedRows[0].Index : -1;
            }
            foreach (var player in list)
            {
                int rowIndex = FindRowIndexByDisplayName(player.DisplayName);

                if (rowIndex == -1)
                {
                    AddNewEntry(Form1.Bans.Rows.Count + 1, player.DisplayName);
                }
                else
                {
                    UpdateRow(rowIndex, player.DisplayName);
                }
            }
            if (Form1.Bans.Rows.Count > 0)
            {
                DataGridViewRow lastRow = Form1.Bans.Rows[Form1.Bans.Rows.Count - 1];

                if (!lastRow.IsNewRow && !Form1.Bans.IsCurrentCellInEditMode)
                {
                    if (Form1.Bans.InvokeRequired)
                    {
                        Form1.Bans.Invoke(new MethodInvoker(delegate {
                            Form1.Bans.Rows.RemoveAt(Form1.Bans.Rows.Count - 1);
                        }));
                    }
                    else
                    {
                        Form1.Bans.Rows.RemoveAt(Form1.Bans.Rows.Count - 1);
                    }
                }
            }
            if (selectedRowIndex >= 0 && selectedRowIndex < Form1.Bans.Rows.Count)
            {
                if (Form1.Bans.InvokeRequired)
                {
                    Form1.Bans.Invoke(new MethodInvoker(delegate {
                        Form1.Bans.Rows[selectedRowIndex].Selected = true;
                    }));
                }
                else
                {
                    Form1.Bans.Rows[selectedRowIndex].Selected = true;
                }
            }
        }
        private static int FindRowIndexByDisplayName(string displayName)
        {
            foreach (DataGridViewRow row in Form1.Bans.Rows)
            {
                if (row.Cells["BanPlayerName"].Value != null && row.Cells["BanPlayerName"].Value.ToString() == displayName)
                {
                    return row.Index;
                }
            }
            return -1;
        }
        private static void UpdateRow(int rowIndex, string displayName)
        {
            if (Form1.Bans.InvokeRequired)
            {
                Form1.Bans.Invoke(new MethodInvoker(delegate {
                    Form1.Bans.Rows[rowIndex].SetValues(rowIndex + 1, displayName);
                }));
            }
            else
            {
                Form1.Bans.Rows[rowIndex].SetValues(rowIndex + 1, displayName);
            }
        }
        public static void AddNewEntry(int number, string playerName)
        {
            var row = new DataGridViewRow();
            row.CreateCells(Form1.Bans, number, playerName);
            if (Form1.Bans.InvokeRequired)
            {
                Form1.Bans.Invoke(new MethodInvoker(delegate {
                    Form1.Bans.Rows.Add(row);
                }));
            }
            else
            {
                Form1.Bans.Rows.Add(row);
            };
        }
    }
}
