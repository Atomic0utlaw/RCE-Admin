using System;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace RCE_ADMIN.Interface
{
    public static class ServerConsole
    {
        public static void AddNewEntry(string text)
        {
            if (text == "playerlist") { return; }
            if (Form1.Console.InvokeRequired)
            {
                Form1.Console.Invoke(new MethodInvoker(delegate {
                    Form1.Console.AppendText("[" + DateTime.Now.ToShortTimeString() + "] " + text + Environment.NewLine);
                }));
            }
            else
            {
                Form1.Console.AppendText("[" + DateTime.Now.ToShortTimeString() + "] " + text + Environment.NewLine);
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
