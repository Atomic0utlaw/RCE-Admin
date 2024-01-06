using DevExpress.XtraEditors;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RCE_ADMIN
{
    static class Program
    {
        [STAThread]
        static async Task Main() 
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            string currentVersion = "v1.2";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "AutoUpdateApp");
            HttpResponseMessage response = await client.GetAsync($"https://api.github.com/repos/KyleFardy/RCE-Admin/releases/latest");

            if (response.IsSuccessStatusCode)
            {
                JObject releaseInfo = JObject.Parse(await response.Content.ReadAsStringAsync());
                string latestVersion = releaseInfo["tag_name"].ToString();
                if (latestVersion != currentVersion)
                {
                    XtraMessageBox.Show($"There Is An Update Available\n\nCurrent Version : {currentVersion}\nNew Version : {latestVersion}\n\nStarting Download Now!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    using (var downloadClient = new HttpClient())
                    {

                        string downloadUrl = releaseInfo["assets"][0]["browser_download_url"].ToString();
                        var downloadResponse = await downloadClient.GetAsync(downloadUrl);
                        if (downloadResponse.IsSuccessStatusCode)
                        {
                            string filePath = "RCE Admin Setup.exe";
                            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await downloadResponse.Content.CopyToAsync(fileStream);
                            }
                            XtraMessageBox.Show("Update Successfully Downloaded, Please Follow The Setup Installer!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });
                            Process.GetCurrentProcess().Kill();
                        }
                        else
                        {
                            XtraMessageBox.Show("Failed To Download Update, Please Download From Github!", "RCE Admin - Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Process.Start(downloadUrl);
                        }
                    }
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                }
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}
