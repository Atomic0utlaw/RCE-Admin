using System;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace RCE_ADMIN.Interface
{
    public static class ServerConsole
    {
        public static void AddNewEntry(string text)
        {
            if (text == "playerlist" || text.Contains("NOTE PANEL") || text.Contains("[CHAT]") || text.Contains("[ SAVE ]") || text.Contains("Hostname") || text.Contains("serverinfo")) { return; }
            if (Form1.Console.InvokeRequired)
            {
                Form1.Console.Invoke(new MethodInvoker(delegate {
                    Form1.Console.AppendText("[" + DateTime.Now.ToShortTimeString() + "] " + text + Environment.NewLine);
                    Form1.Console.Refresh();
                    Form1.Console.SelectionStart = Form1.Console.Text.Length;
                    Form1.Console.ScrollToCaret();
                }));
            }
            else
            {
                Form1.Console.AppendText("[" + DateTime.Now.ToShortTimeString() + "] " + text + Environment.NewLine);
                Form1.Console.Refresh();
                Form1.Console.SelectionStart = Form1.Console.Text.Length;
                Form1.Console.ScrollToCaret();
            };
        }
        public static void Clear()
        {
            if (Form1.Console.InvokeRequired)
            {
                Form1.Console.Invoke(new MethodInvoker(delegate {
                    Form1.Console.Clear();
                }));
            }
            else
            {
                Form1.Console.Clear();
            };
        }
        public static void Disable()
        {
            if (Form1.Console.InvokeRequired)
            {
                Form1.Console.Invoke(new MethodInvoker(delegate {
                    Form1.Console.Enabled = false;
                }));
            }
            else
            {
                Form1.Console.Enabled = false;
            };
        }
        public static void Enable()
        {
            if (Form1.Console.InvokeRequired)
            {
                Form1.Console.Invoke(new MethodInvoker(delegate {
                    Form1.Console.Enabled = true;
                }));
            }
            else
            {
                Form1.Console.Enabled = true;
            };
        }
    }
}
