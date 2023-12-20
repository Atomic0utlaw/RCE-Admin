using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Newtonsoft.Json;

namespace RCE_ADMIN
{
    public class Settings
    {
        private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        [JsonProperty("ServerAddress")]
        public string ServerAddress { get; set; }
        [JsonProperty("ServerPort")]
        public string ServerPort { get; set; }
        [JsonProperty("ServerPassword")]
        public string ServerPassword { get; set; }
        public Settings(string serverAddress, string serverPort, string serverPassword)
        {
            ServerAddress = serverAddress;
            ServerPort = serverPort;
            ServerPassword = serverPassword;
        }
        public static void Write(Settings settings)
        {
            if (!File.Exists(FilePath))
                File.Create(FilePath).Close();
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
            XtraMessageBox.Show("Successfully Saved Settings!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static Settings Read()
        {
            if (!File.Exists(FilePath))
            {
                XtraMessageBox.Show("Couldn't Find A Configuration File, A New One Will Be Created", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Write(new Settings("localhost", "28016", "password"));
            }
            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath));
            return settings;
        }
    }
}
