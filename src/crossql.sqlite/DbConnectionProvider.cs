using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using crossql;

namespace crossql.sqlite
{
    public class DbConnectionProvider : IDbConnectionProvider
    {
        private readonly string _connectionString;

        public DbConnectionProvider(string sqlFile, SqliteSettings sqliteSettings)
        {
            var connectionString = $"Data Source={sqlFile}";
            var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder(sqlFile)
                {
                
                    //CacheSize = sqliteSettings.CacheSize,
                    //JournalMode = GetJournalMode(sqliteSettings.JournalMode),
                    //PageSize = sqliteSettings.PageSize,
                    //DefaultTimeout = (int) sqliteSettings.DefaultTimeout.TotalMilliseconds,
                    //SynchronizationModes = GetSyncMode(sqliteSettings.SynchronizationModes),
                    //FailIfMissing = sqliteSettings.FailIfMissing,
                    //ReadOnly = sqliteSettings.ReadOnly,
                };
            
            _connectionString = sqliteConnectionStringBuilder.ConnectionString;
        }

        public DbConnectionProvider(string sqlFile)
        {
            var connectionString = $"Data Source={sqlFile}";
            _connectionString = connectionString;
        }

        public async Task<IDbConnection> GetOpenConnectionAsync()
        {
            var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }
    }
}