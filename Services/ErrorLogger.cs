using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace VKBot.Web.Services
{
    public class ErrorLogItem
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ErrorLevel { get; set; } = "ERROR";
        public string ErrorMessage { get; set; } = "";
        public string? StackTrace { get; set; }
        public long? UserId { get; set; }
        public string? Command { get; set; }
        public string? AdditionalData { get; set; }
    }

    public class ErrorLogger
    {
        private readonly string _connectionString;
        public ErrorLogger()
        {
            _connectionString = "Data Source=errors.db";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();
                var schema = @"CREATE TABLE IF NOT EXISTS error_logs (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    error_level TEXT,
                    error_message TEXT,
                    stack_trace TEXT,
                    user_id INTEGER,
                    command TEXT,
                    additional_data TEXT
                );";
                using var cmd = new SqliteCommand(schema, conn);
                cmd.ExecuteNonQuery();
            }
            catch { /* ignore init errors */ }
        }

        public async Task LogErrorAsync(Exception ex, string level = "ERROR", long? userId = null, string? command = null, object? additional = null)
        {
            try
            {
                await using var conn = new SqliteConnection(_connectionString);
                await conn.OpenAsync();
                var addJson = additional != null ? JsonSerializer.Serialize(additional) : null;
                var sql = @"INSERT INTO error_logs (error_level, error_message, stack_trace, user_id, command, additional_data)
                             VALUES (@level, @msg, @stack, @uid, @cmd, @add);";
                await using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@level", level);
                cmd.Parameters.AddWithValue("@msg", ex.Message);
                cmd.Parameters.AddWithValue("@stack", ex.StackTrace ?? string.Empty);
                cmd.Parameters.AddWithValue("@uid", userId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@cmd", command ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@add", addJson ?? (object)DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { /* fallback to console if needed */ }
        }

        public async Task<List<ErrorLogItem>> GetRecentErrorsAsync(int limit = 20)
        {
            var res = new List<ErrorLogItem>();
            try
            {
                await using var conn = new SqliteConnection(_connectionString);
                await conn.OpenAsync();
                var sql = @"SELECT id, timestamp, error_level, error_message, stack_trace, user_id, command, additional_data
                            FROM error_logs ORDER BY timestamp DESC LIMIT @limit";
                await using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@limit", limit);
                await using var rdr = await cmd.ExecuteReaderAsync();
                while (await rdr.ReadAsync())
                {
                    res.Add(new ErrorLogItem
                    {
                        Id = rdr.GetInt32(0),
                        Timestamp = rdr.GetDateTime(1),
                        ErrorLevel = rdr.GetString(2),
                        ErrorMessage = rdr.GetString(3),
                        StackTrace = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                        UserId = rdr.IsDBNull(5) ? null : (long?)rdr.GetInt64(5),
                        Command = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                        AdditionalData = rdr.IsDBNull(7) ? null : rdr.GetString(7)
                    });
                }
            }
            catch { }
            return res;
        }
    }
}
