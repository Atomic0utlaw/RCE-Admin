using System;
using System.Linq;
using System.Text.RegularExpressions;
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
                    Form1.ConsoleTab.Enabled = false;
                    Form1.PlayersTab.Enabled = false;
                    Form1.EventsTab.Enabled = false;
                }));
            }
            else
            {
                Form1.Console.Enabled = false;
                Form1.ConsoleTab.Enabled = false;
                Form1.PlayersTab.Enabled = false;
                Form1.EventsTab.Enabled = false;
            };
        }
        public static void Enable()
        {
            if (Form1.Console.InvokeRequired)
            {
                Form1.Console.Invoke(new MethodInvoker(delegate {
                    Form1.Console.Enabled = true;
                    Form1.ConsoleTab.Enabled = true;
                    Form1.PlayersTab.Enabled = true;
                    Form1.EventsTab.Enabled = true;
                }));
            }
            else
            {
                Form1.Console.Enabled = true;
                Form1.ConsoleTab.Enabled = true;
                Form1.PlayersTab.Enabled = true;
                Form1.EventsTab.Enabled = true;
            };
        }
        public static bool IsValidPrintPos(string inputString)
        {
            string pattern = @"\((-?\d+(\.\d+)?), (-?\d+(\.\d+)?), (-?\d+(\.\d+)?)\)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(inputString);
            return match.Success;
        }

        public static string GetPos(string inputString)
        {
            string pattern = @"\((-?\d+(\.\d+)?), (-?\d+(\.\d+)?), (-?\d+(\.\d+)?)\)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(inputString);
            if (match.Success)
            {
                int matchIndex = match.Index;
                return inputString.Substring(matchIndex);
            }
            return inputString;
        }
        public static string ReadLastFewLines()
        {
            string[] lines = { };
            string pos = "";
            if (Form1.Console.InvokeRequired)
            {
                Form1.Console.Invoke(new MethodInvoker(delegate {
                    lines = Form1.Console.Lines;

                }));
            }
            else
            {
                lines = Form1.Console.Lines;
            };
            string[] lastLines = lines.Skip(Math.Max(0, lines.Length - 10)).ToArray();
            foreach (string line in lastLines)
            {
                if (IsValidPrintPos(line))
                {
                    pos = GetPos(line);
                }
            }
            return pos;
        }
    }
}
