using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;
using RCE_ADMIN.WebSockets.CustomPackets;
using System.Data;
using System.Windows.Forms;

namespace RCE_ADMIN.Callbacks
{
    public class PlayerDatabase
    {
        private readonly string connectionString;

        public PlayerDatabase()
        {
            connectionString = $"Data Source=players.db;Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Player (
                    SteamId TEXT PRIMARY KEY,
                    OwnerSteamId TEXT,
                    DisplayName TEXT,
                    Ping INTEGER,
                    Address TEXT,
                    ConnectedSeconds INTEGER,
                    ViolationLevel REAL,
                    CurrentLevel REAL,
                    UnspentXp REAL,
                    Health REAL
                )");
            }
        }

        public void SavePlayer(Player player)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var existingPlayer = connection.QueryFirstOrDefault<Player>("SELECT * FROM Player WHERE DisplayName = @DisplayName", new { player.DisplayName });

                if (existingPlayer == null)
                {
                     connection.Execute(@"
                        INSERT INTO Player VALUES (
                            @SteamId, @OwnerSteamId, @DisplayName, @Ping, @Address,
                            @ConnectedSeconds, @ViolationLevel, @CurrentLevel, @UnspentXp, @Health
                        )", player);
                }
                else
                {
                    connection.Execute(@"
                        UPDATE Player SET
                            OwnerSteamId = @OwnerSteamId,
                            DisplayName = @DisplayName,
                            Ping = @Ping,
                            Address = @Address,
                            ConnectedSeconds = @ConnectedSeconds,
                            ViolationLevel = @ViolationLevel,
                            CurrentLevel = @CurrentLevel,
                            UnspentXp = @UnspentXp,
                            Health = @Health
                        WHERE DisplayName = @DisplayName", player);
                }
            }
        }
        public void GetAllPlayers(DataGridView dataTable)
        {
            dataTable.Rows.Clear();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var players = connection.Query<Player>("SELECT * FROM Player").ToList();
                if (players.Any())
                {
                    int i = 1;
                    foreach (var player in players)
                    {
                        TimeSpan timeSpan = TimeSpan.FromSeconds(player.ConnectedSeconds);
                        dataTable.Rows.Add(
                            i,
                            player.DisplayName,
                            string.Format("{0} Hour(s), {1} Min(s), {2} Sec(s)", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds)
                        );
                        i++;
                    }
                }
            }
        }
    }
}
