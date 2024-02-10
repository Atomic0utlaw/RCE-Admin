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
using DevExpress.Xpo.DB.Helpers;
using DiscordRPC;
using static RCE_ADMIN.Form1;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using DevExpress.XtraEditors;
using static RCE_ADMIN.Callbacks.PlayerDatabase;

namespace RCE_ADMIN.Callbacks
{
    public class PlayerDatabase
    {
        private readonly string connectionString;
        private readonly DatabaseType databaseType;

        public enum DatabaseType
        {
            SQLite,
            MySQL
        }
        public PlayerDatabase()
        {
            switch (Settings.SQLType)
            {
                case "mysql":
                    this.databaseType = PlayerDatabase.DatabaseType.MySQL;
                    this.connectionString = string.Format(
                        "Server={0};Database={4};Uid={2};Pwd={3};Port={1};",
                        Settings.MySQLHost,
                        Settings.MySQLPort,
                        Settings.MySQLUsername,
                        Settings.MySQLPassword,
                        Settings.MySQLDatabaseName
                     );
                    break;
                case "sqlite":
                    this.databaseType = PlayerDatabase.DatabaseType.SQLite;
                    this.connectionString = "Data Source=players.db;Version=3;";
                    break;
                default:
                    this.databaseType = PlayerDatabase.DatabaseType.SQLite;
                    this.connectionString = "Data Source=players.db;Version=3;";
                    break;
            }
            InitializeDatabase();
        }
        private IDbConnection CreateConnection(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SQLite:
                    return new SQLiteConnection("Data Source=players.db;Version=3;");

                case DatabaseType.MySQL:
                    return new MySqlConnection(string.Format(
                        "Server={0};Database={4};Uid={2};Pwd={3};Port={1};",
                        Settings.MySQLHost,
                        Settings.MySQLPort,
                        Settings.MySQLUsername,
                        Settings.MySQLPassword,
                        Settings.MySQLDatabaseName
                     ));

                default:
                    throw new InvalidOperationException("Invalid Database Type");
            }
        }
        private IDbConnection CreateConnection()
        {
            switch (databaseType)
            {
                case DatabaseType.SQLite:
                    return new SQLiteConnection(connectionString);

                case DatabaseType.MySQL:
                    return new MySqlConnection(connectionString);

                default:
                    throw new InvalidOperationException("Invalid Database Type");
            }
        }
        private void AddParameter(IDbCommand command, string parameterName, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
        private bool IsValidMySQLConnection(IDbConnection connection)
        {
            try
            {
                connection.Execute("SELECT 1");
                return true;
            }
            catch
            {
                return false;
            }
        }
        private string GetMySqlError(IDbConnection connection)
        {
            if (connection is MySqlConnection mySqlConnection)
            {
                return mySqlConnection.ServerVersion;
            }
            else
            {
                return "Unknown MySQL Error";
            }
        }

        private void InitializeDatabase()
        {
            using (var connection = CreateConnection())
            {
                try
                {
                    connection.Open();

                    if (Settings.SQLType == "sqlite")
                    {
                        // SQLite
                        connection.Execute(@"
                        CREATE TABLE IF NOT EXISTS Player (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            SteamId TEXT NULL,
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
                            NPCKills INTEGER DEFAULT 0,
                            NPCDeaths INTEGER DEFAULT 0,
                            Deaths INTEGER DEFAULT 0
                        )");
                    }
                    else if (Settings.SQLType == "mysql")
                    {
                        // MySQL
                        if (IsValidMySQLConnection(connection))
                        {
                            connection.Execute(@"
                            CREATE TABLE IF NOT EXISTS Player (
                                ID INT AUTO_INCREMENT PRIMARY KEY,
                                SteamId VARCHAR(255) NULL,
                                OwnerSteamId VARCHAR(255) NULL,
                                DisplayName VARCHAR(255),
                                Ping INT,
                                Address VARCHAR(255) NULL,
                                ConnectedSeconds INT,
                                ViolationLevel DOUBLE NULL,
                                CurrentLevel DOUBLE NULL,
                                UnspentXp DOUBLE NULL,
                                Health DOUBLE,
                                Kills INT DEFAULT 0,
                                NPCKills INT DEFAULT 0,
                                NPCDeaths INT DEFAULT 0,
                                Deaths INT DEFAULT 0
                            )");
                        }
                        else
                        {
                            XtraMessageBox.Show($"MySQL Connection Error: {GetMySqlError(connection)}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        XtraMessageBox.Show("MySQL Connection Error: Unsupported SQL type", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show($"MySQL Connection Error: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connection.Close();
                }
            }

            bool npckillsColumnExists;
            bool killsColumnExists;
            bool deathsColumnExists;
            bool kitsColumnExists;
            bool lastRedeemedColumnExists;
            bool limitColumnExists;
            bool npcdeathsColumnExists;

            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    if (databaseType == DatabaseType.SQLite)
                    {
                        command.CommandText = "PRAGMA table_info('Player')";
                    }
                    else if (databaseType == DatabaseType.MySQL)
                    {
                        command.CommandText = "SHOW COLUMNS FROM Player";
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        npckillsColumnExists = false;
                        killsColumnExists = false;
                        deathsColumnExists = false;
                        kitsColumnExists = false;
                        lastRedeemedColumnExists = false;
                        limitColumnExists = false;
                        npcdeathsColumnExists = false;

                        DataTable schemaTable = reader.GetSchemaTable();
                        if (schemaTable != null)
                        {
                            foreach (DataRow row in schemaTable.Rows)
                            {
                                string columnName = row["ColumnName"].ToString();

                                if (columnName.Equals("NPCKills", StringComparison.OrdinalIgnoreCase))
                                {
                                    npckillsColumnExists = true;
                                }
                                else if (columnName.Equals("NPCDeaths", StringComparison.OrdinalIgnoreCase))
                                {
                                    npcdeathsColumnExists = true;
                                }
                                else if (columnName.Equals("Kills", StringComparison.OrdinalIgnoreCase))
                                {
                                    killsColumnExists = true;
                                }
                                else if (columnName.Equals("Deaths", StringComparison.OrdinalIgnoreCase))
                                {
                                    deathsColumnExists = true;
                                }
                                else if (columnName.Equals("Kits", StringComparison.OrdinalIgnoreCase))
                                {
                                    kitsColumnExists = true;
                                }
                                else if (columnName.Equals("LastRedeemed", StringComparison.OrdinalIgnoreCase))
                                {
                                    lastRedeemedColumnExists = true;
                                }
                                else if (columnName.Equals("KitLimit", StringComparison.OrdinalIgnoreCase))
                                {
                                    limitColumnExists = true;
                                }
                            }
                        }
                    }

                    // Now you can safely check and add columns for SQLite
                    if (!killsColumnExists && Settings.SQLType == "sqlite")
                    {
                        command.CommandText = "PRAGMA table_info('Player')";
                        using (var pragmaReader = command.ExecuteReader())
                        {
                            bool columnExists = false;
                            while (pragmaReader.Read())
                            {
                                string pragmaColumnName = pragmaReader["name"].ToString();
                                if (pragmaColumnName.Equals("Kills", StringComparison.OrdinalIgnoreCase))
                                {
                                    columnExists = true;
                                    break;
                                }
                            }

                            if (!columnExists)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN Kills INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    // Now you can safely check and add columns for MySQL
                    if (!killsColumnExists && Settings.SQLType == "mysql")
                    {
                        command.CommandText = "SHOW COLUMNS FROM Player LIKE 'Kills'";
                        object result = command.ExecuteScalar();

                        if (result == null || result == DBNull.Value)
                        {
                            command.CommandText = "ALTER TABLE Player ADD COLUMN Kills INTEGER DEFAULT 0";
                            command.ExecuteNonQuery();
                        }
                    }
                    // Check and add columns for NPCKills
                    if (!npckillsColumnExists)
                    {
                        if (Settings.SQLType == "sqlite")
                        {
                            command.CommandText = "PRAGMA table_info('Player')";
                            using (SQLiteDataReader pragmaReader = (SQLiteDataReader)command.ExecuteReader())
                            {
                                while (pragmaReader.Read())
                                {
                                    string columnName = pragmaReader["name"].ToString();
                                    if (columnName.Equals("NPCKills", StringComparison.OrdinalIgnoreCase))
                                    {
                                        npckillsColumnExists = true;
                                        break;
                                    }
                                }
                            }

                            if (!npckillsColumnExists)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN NPCKills INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                        else if (Settings.SQLType == "mysql")
                        {
                            command.CommandText = "SHOW COLUMNS FROM Player LIKE 'NPCKills'";
                            object result = command.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN NPCKills INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    if (!npcdeathsColumnExists)
                    {
                        if (Settings.SQLType == "sqlite")
                        {
                            command.CommandText = "PRAGMA table_info('Player')";
                            using (SQLiteDataReader pragmaReader = (SQLiteDataReader)command.ExecuteReader())
                            {
                                while (pragmaReader.Read())
                                {
                                    string columnName = pragmaReader["name"].ToString();
                                    if (columnName.Equals("NPCDeaths", StringComparison.OrdinalIgnoreCase))
                                    {
                                        npcdeathsColumnExists = true;
                                        break;
                                    }
                                }
                            }

                            if (!npcdeathsColumnExists)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN NPCDeaths INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                        else if (Settings.SQLType == "mysql")
                        {
                            command.CommandText = "SHOW COLUMNS FROM Player LIKE 'NPCDeaths'";
                            object result = command.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN NPCDeaths INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    // Check and add columns for Deaths
                    if (!deathsColumnExists)
                    {
                        if (Settings.SQLType == "sqlite")
                        {
                            command.CommandText = "PRAGMA table_info('Player')";
                            using (SQLiteDataReader pragmaReader = (SQLiteDataReader)command.ExecuteReader())
                            {
                                while (pragmaReader.Read())
                                {
                                    string columnName = pragmaReader["name"].ToString();
                                    if (columnName.Equals("Deaths", StringComparison.OrdinalIgnoreCase))
                                    {
                                        deathsColumnExists = true;
                                        break;
                                    }
                                }
                            }

                            if (!deathsColumnExists)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN Deaths INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                        else if (Settings.SQLType == "mysql")
                        {
                            command.CommandText = "SHOW COLUMNS FROM Player LIKE 'Deaths'";
                            object result = command.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN Deaths INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    // Check and add columns for Kits
                    if (!kitsColumnExists)
                    {
                        if (Settings.SQLType == "sqlite")
                        {
                            command.CommandText = "PRAGMA table_info('Player')";
                            using (SQLiteDataReader pragmaReader = (SQLiteDataReader)command.ExecuteReader())
                            {
                                while (pragmaReader.Read())
                                {
                                    string columnName = pragmaReader["name"].ToString();
                                    if (columnName.Equals("Kits", StringComparison.OrdinalIgnoreCase))
                                    {
                                        kitsColumnExists = true;
                                        break;
                                    }
                                }
                            }

                            if (!kitsColumnExists)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN Kits INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                        else if (Settings.SQLType == "mysql")
                        {
                            command.CommandText = "SHOW COLUMNS FROM Player LIKE 'Kits'";
                            object result = command.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN Kits INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    // Check and add columns for LastRedeemed
                    if (!lastRedeemedColumnExists)
                    {
                        if (Settings.SQLType == "sqlite")
                        {
                            command.CommandText = "PRAGMA table_info('Player')";
                            using (SQLiteDataReader pragmaReader = (SQLiteDataReader)command.ExecuteReader())
                            {
                                while (pragmaReader.Read())
                                {
                                    string columnName = pragmaReader["name"].ToString();
                                    if (columnName.Equals("LastRedeemed", StringComparison.OrdinalIgnoreCase))
                                    {
                                        lastRedeemedColumnExists = true;
                                        break;
                                    }
                                }
                            }

                            if (!lastRedeemedColumnExists)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN LastRedeemed INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                        else if (Settings.SQLType == "mysql")
                        {
                            command.CommandText = "SHOW COLUMNS FROM Player LIKE 'LastRedeemed'";
                            object result = command.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN LastRedeemed INTEGER DEFAULT 0";
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    // Check and add columns for KitLimit
                    if (!limitColumnExists)
                    {
                        if (Settings.SQLType == "sqlite")
                        {
                            command.CommandText = "PRAGMA table_info('Player')";
                            using (SQLiteDataReader pragmaReader = (SQLiteDataReader)command.ExecuteReader())
                            {
                                while (pragmaReader.Read())
                                {
                                    string columnName = pragmaReader["name"].ToString();
                                    if (columnName.Equals("KitLimit", StringComparison.OrdinalIgnoreCase))
                                    {
                                        limitColumnExists = true;
                                        break;
                                    }
                                }
                            }

                            if (!limitColumnExists)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN KitLimit INTEGER DEFAULT 1";
                                command.ExecuteNonQuery();
                            }
                        }
                        else if (Settings.SQLType == "mysql")
                        {
                            command.CommandText = "SHOW COLUMNS FROM Player LIKE 'KitLimit'";
                            object result = command.ExecuteScalar();

                            if (result == null || result == DBNull.Value)
                            {
                                command.CommandText = "ALTER TABLE Player ADD COLUMN KitLimit INTEGER DEFAULT 1";
                                command.ExecuteNonQuery();
                            }
                        }
                    }


                }
                connection.Close();
            }
        }

        public void AddKill(string player_name)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Player SET Kills = Kills + 1 WHERE DisplayName = @DisplayName";
                    AddParameter(command, "@DisplayName", player_name);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void AddNPCKill(string player_name)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Player SET NPCKills = NPCKills + 1 WHERE DisplayName = @DisplayName";
                    AddParameter(command, "@DisplayName", player_name);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void AddNPCDeath(string player_name)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Player SET NPCDeaths = NPCDeaths + 1 WHERE DisplayName = @DisplayName";
                    AddParameter(command, "@DisplayName", player_name);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        public void UpdateKitInfo(string player_name, bool aloud, int limit)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Player SET Kits = @Kits, LastRedeemed = @LastRedeemed, KitLimit = KitLimit + @KitLimit WHERE DisplayName = @DisplayName";
                    AddParameter(command, "@DisplayName", player_name);
                    AddParameter(command, "@Kits", aloud ? "1" : "0");
                    AddParameter(command, "@LastRedeemed", DateTime.Now);
                    AddParameter(command, "@KitLimit", limit);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        public void AddDeath(string player_name)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Player SET Deaths = Deaths + 1 WHERE DisplayName = @DisplayName";
                    AddParameter(command, "@DisplayName", player_name);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public int GetKillsByDisplayName(string displayName)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                var result = connection.QueryFirstOrDefault("SELECT Kills FROM Player WHERE DisplayName = @DisplayName", new { DisplayName = displayName });

                if (result != null)
                {
                    return result.Kills;
                }
                else
                {
                    return 0;
                }
            }
        }


        public string[] GetPlayerKitsInfo(string displayName)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                var result = connection.QueryFirstOrDefault("SELECT Kits, LastRedeemed, KitLimit FROM Player WHERE DisplayName = @DisplayName", new { DisplayName = displayName });

                if (result != null)
                {
                    int kits = Convert.ToInt32(result.Kits);
                    DateTime lastRedeemed = Convert.ToDateTime(result.LastRedeemed);
                    int kitLimit = Convert.ToInt32(result.KitLimit);

                    return new string[] { kits.ToString(), lastRedeemed.Date.ToString(), kitLimit.ToString() };
                }
                else
                {
                    return new string[] { string.Empty, string.Empty, string.Empty };
                }
            }
        }


        public string[] GetKillStatsByName(string displayName)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                var result = connection.QueryFirstOrDefault("SELECT NPCKills, NPCDeaths, Kills, Deaths FROM Player WHERE DisplayName = @DisplayName", new { DisplayName = displayName });

                if (result != null)
                {
                    int kills = result.Kills != null ? Convert.ToInt32(result.Kills) : 0;
                    int deaths = result.Deaths != null ? Convert.ToInt32(result.Deaths) : 0;
                    int npc_kills = result.NPCKills != null ? Convert.ToInt32(result.NPCKills) : 0;
                    int npc_deaths = result.NPCDeaths != null ? Convert.ToInt32(result.NPCDeaths) : 0;

                    double killDeathRatio = deaths != 0 ? (double)kills / deaths : 0.0;
                    double npcKillDeathRatio = npc_deaths != 0 ? (double)npc_kills / npc_deaths : 0.0;

                    return new string[] { kills.ToString(), deaths.ToString(), killDeathRatio.ToString(), npc_kills.ToString(), npc_deaths.ToString(), npcKillDeathRatio.ToString() };
                }
                else
                {
                    return new string[] { "", "", "", "", "", "" };
                }
            }
        }
        public int GetDeathsByDisplayName(string displayName)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                var result = connection.QueryFirstOrDefault<int?>("SELECT Deaths FROM Player WHERE DisplayName = @DisplayName", new { DisplayName = displayName });

                if (result != null)
                {
                    return result.Value;
                }
                else
                {
                    return 0;
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
        public void MigrateData()
        {
            using (var sqliteConnection = CreateConnection(DatabaseType.SQLite))
            using (var mysqlConnection = CreateConnection(DatabaseType.MySQL))
            {
                sqliteConnection.Open();
                mysqlConnection.Open();
                var players = sqliteConnection.Query<Player>("SELECT * FROM Player").ToList();
                foreach (var player in players)
                {
                    InsertPlayer(mysqlConnection, player);
                }
                XtraMessageBox.Show("All Data Has Been Migrated To The MySQL Database!", "RCE Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        static void InsertPlayer(IDbConnection connection, Player player)
        {
            var existingPlayer = connection.QueryFirstOrDefault<Player>("SELECT * FROM Player WHERE DisplayName = @DisplayName", new { DisplayName = player.DisplayName });

            if (existingPlayer == null)
            {
                connection.Execute(@"
                    INSERT INTO Player (SteamId, OwnerSteamId, DisplayName, Ping, Address, ConnectedSeconds, ViolationLevel, CurrentLevel, UnspentXp, Health, Kills, NPCKills, NPCDeaths, Deaths)
                    VALUES (@SteamId, @OwnerSteamId, @DisplayName, @Ping, @Address, @ConnectedSeconds, @ViolationLevel, @CurrentLevel, @UnspentXp, @Health, @Kills, @NPCKills, @NPCDeaths, @Deaths)", player);
            }
            else
            {
                connection.Execute(@"
                UPDATE Player 
                SET OwnerSteamId = @OwnerSteamId,
                    DisplayName = @DisplayName,
                    Ping = @Ping,
                    Address = @Address,
                    ConnectedSeconds = @ConnectedSeconds,
                    ViolationLevel = @ViolationLevel,
                    CurrentLevel = @CurrentLevel,
                    UnspentXp = @UnspentXp,
                    Health = @Health,
                    Kills = @Kills,
                    NPCKills = @NPCKills,
                    NPCDeaths = @NPCDeaths,
                    Deaths = @Deaths
                WHERE DisplayName = @DisplayName", player);
            }
        }

        public void SavePlayer(Player player)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var existingPlayer = connection.QueryFirstOrDefault<Player>("SELECT * FROM Player WHERE DisplayName = @DisplayName", new { player.DisplayName });
                if (existingPlayer == null)
                {
                    connection.Execute(@"
                        INSERT INTO Player (SteamId, OwnerSteamId, DisplayName, Ping, Address, ConnectedSeconds, ViolationLevel, CurrentLevel, UnspentXp, Health, Kills, NPCKills, NPCDeaths, Deaths) VALUES (@SteamId, @OwnerSteamId, @DisplayName, @Ping, @Address, @ConnectedSeconds, @ViolationLevel, @CurrentLevel, @UnspentXp, @Health, 0, 0, 0, 0)", player);
                }
                else
                {
                    connection.Execute(@"
                        UPDATE Player SET
                            SteamId = @SteamId,
                            OwnerSteamId = @OwnerSteamId,
                            DisplayName = @DisplayName,
                            Ping = @Ping,
                            Address = @Address,
                            ConnectedSeconds = ConnectedSeconds + 1,
                            ViolationLevel = @ViolationLevel,
                            CurrentLevel = @CurrentLevel,
                            UnspentXp = @UnspentXp,
                            Health = @Health
                        WHERE DisplayName = @DisplayName", player);
                }
                connection.Close();
            }
        }

        public void GetAllPlayers(DataGridView dataTable)
        {
            dataTable.Rows.Clear();
            using (var connection = CreateConnection())
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

                connection.Close();
            }
        }
    }
}
