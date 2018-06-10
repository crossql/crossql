using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using crossql;

namespace crossql.sqlite
{
    public class DbConnectionProvider : IDbConnectionProvider
    {
        private readonly string _connectionString;
        
        public DbConnectionProvider(string sqlFile):this(sqlFile, SqliteSettings.Default){  }

        public DbConnectionProvider(string sqlFile, SqliteSettings sqliteSettings)
        {
            DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), sqlFile);            
            var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder($"Data Source={DatabasePath}")
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

        public string DatabasePath { get; }
            
        public async Task<IDbConnection> GetOpenConnectionAsync()
        {
            var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }
    }
}