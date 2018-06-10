using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace crossql.sqlite
{
    public class DbConnectionProvider : IDbConnectionProvider
    {
        private readonly string _connectionString;
        private static readonly Func<string, string> _defaultSetupDbPath = s=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), s);
        
        public DbConnectionProvider(string sqlFile) : this(sqlFile, SqliteSettings.Default, _defaultSetupDbPath) { }
        public DbConnectionProvider(string sqlFile,SqliteSettings sqliteSettings) : this(sqlFile, sqliteSettings, _defaultSetupDbPath) { }
        public DbConnectionProvider(string sqlFile, Func<string,string> setupDbPath) : this(sqlFile, SqliteSettings.Default, setupDbPath) { }

        public DbConnectionProvider(string sqlFile, SqliteSettings sqliteSettings, Func<string, string> setupDbPath)
        {
            DatabasePath = setupDbPath.Invoke(sqlFile);
            var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder($"Data Source={DatabasePath}");

            _connectionString = sqliteConnectionStringBuilder.ConnectionString;
        }

        public string DatabasePath { get; }

        public async Task<IDbConnection> GetOpenConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }
    }
}