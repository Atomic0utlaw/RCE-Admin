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
                    Health REAL,
                    Kills INTEGER DEFAULT 0,
                    Deaths INTEGER DEFAULT 0
                )");
            }
            bool killsColumnExists;
            bool deathsColumnExists;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    // Check if Kills and Deaths columns exist
                    command.CommandText = "PRAGMA table_info('Player')";
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        killsColumnExists = false;
                        deathsColumnExists = false;

                        while (reader.Read())
                        {
                            string columnName = reader["name"].ToString();
                            if (columnName.Equals("Kills", StringComparison.OrdinalIgnoreCase))
                            {
                                killsColumnExists = true;
                            }
                            else if (columnName.Equals("Deaths", StringComparison.OrdinalIgnoreCase))
                            {
                                deathsColumnExists = true;
                            }
                        }
                    }
                    if (!killsColumnExists)
                    {
                        command.CommandText = "ALTER TABLE Player ADD COLUMN Kills INTEGER DEFAULT 0";
                        command.ExecuteNonQuery();
                    }

                    if (!deathsColumnExists)
                    {
                        command.CommandText = "ALTER TABLE Player ADD COLUMN Deaths INTEGER DEFAULT 0";
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
        }
        public void AddKill(string player_name)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "UPDATE Player SET Kills = Kills + 1 WHERE DisplayName = @DisplayName";
                    command.Parameters.AddWithValue("@DisplayName", player_name);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        public void AddDeath(string player_name)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "UPDATE Player SET Deaths = Deaths + 1 WHERE DisplayName = @DisplayName";
                    command.Parameters.AddWithValue("@DisplayName", player_name);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
        public int GetKillsByDisplayName(string displayName)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT Kills FROM Player WHERE DisplayName = @DisplayName";
                    command.Parameters.AddWithValue("@DisplayName", displayName);

                    object result = command.ExecuteScalar();

                    connection.Close();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        public int GetDeathsByDisplayName(string displayName)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT Deaths FROM Player WHERE DisplayName = @DisplayName";
                    command.Parameters.AddWithValue("@DisplayName", displayName);

                    object result = command.ExecuteScalar();

                    connection.Close();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
        public double GetKillDeathRatioByDisplayName(string displayName)
        {
            int kills = GetKillsByDisplayName(displayName);
            int deaths = GetDeathsByDisplayName(displayName);
            if (deaths != 0)
            {
                return (double)kills / deaths;
            }
            else
            {
                return 0;
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
                            @ConnectedSeconds, @ViolationLevel, @CurrentLevel, @UnspentXp, @Health, 0, 0
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
                        dataTable.Rows.Add(
                            i,
                            player.DisplayName
                        );
                        i++;
                    }
                }
            }
        }
    }
}
