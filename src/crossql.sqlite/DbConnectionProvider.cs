using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace crossql.sqlite
{
    public class DbConnectionProvider : IDbConnectionProvider, IDisposable
    {
        private static readonly string[] _memory = { ":memory:", "file::memory:" };
        private static readonly Func<string, string> _defaultDbPath = s => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), s);
        private static readonly Func<string, string> _inMemoryDbPath = s => s.ToLowerInvariant().Equals("file::memory:", StringComparison.OrdinalIgnoreCase) ? $"{s}?cache=shared" : s;
        private readonly string _connectionString;
        private SqliteConnection _connection;

        /// <inheritdoc />
        /// <summary>
        ///     Sets up an SQLite connection string using default settings and the default file path
        ///     <see cref="F:System.Environment.SpecialFolder.ApplicationData" />.
        /// </summary>
        /// <param name="sqlFile">name of your sql file</param>
        public DbConnectionProvider(string sqlFile) : this(sqlFile, SqliteSettings.Default, _memory.Any(x => x.ToLowerInvariant().Contains(sqlFile.ToLowerInvariant())) ? _inMemoryDbPath : _defaultDbPath) { }

        /// <inheritdoc />
        /// <summary>
        ///     Sets up an SQLite connection string using custom settings and the default file path
        ///     <see cref="F:System.Environment.SpecialFolder.ApplicationData" />.
        /// </summary>
        /// <param name="sqlFile">name of your sql file</param>
        /// <param name="sqliteSettings">sqlite settings</param>
        public DbConnectionProvider(string sqlFile, SqliteSettings sqliteSettings) : this(sqlFile, sqliteSettings, _memory.Any(x => x.ToLowerInvariant().Contains(sqlFile.ToLowerInvariant())) ? _inMemoryDbPath : _defaultDbPath) { }

        /// <inheritdoc />
        /// <summary>
        ///     Sets up an SQLite connection string using default settings and invokes custom user code in order to build the file
        ///     path.
        /// </summary>
        /// <param name="sqlFile">name of your sql file</param>
        /// <param name="setupDbPath">custom Func to initialize your db file path</param>
        /// <example>
        ///     <![CDATA[
        /// // will put your database in the users Personal Documents directory.
        /// var dbConnectionProvider = new DbConnectionProvider("mydb.sqlite3", 
        ///     fileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName))
        /// ]]>
        /// </example>
        public DbConnectionProvider(string sqlFile, Func<string, string> setupDbPath) : this(sqlFile, SqliteSettings.Default, setupDbPath) { }

        /// <summary>
        ///     Sets up an SQLite connection string using custom settings and invokes custom user code in order to build the file
        ///     path.
        /// </summary>
        /// <param name="sqlFile">name of your sql file</param>
        /// <param name="sqliteSettings">sqlite settings</param>
        /// <param name="setupDbPath">custom Func to initialize your db file path</param>
        /// <example>
        ///     <![CDATA[
        /// // will put your database in the users Personal Documents directory.
        /// var dbConnectionProvider = new DbConnectionProvider("mydb.sqlite3", 
        ///     new SqliteSettings{ BrowsableConnectionString = false };
        ///     fileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName))
        /// ]]>
        /// </example>
        public DbConnectionProvider(string sqlFile, SqliteSettings sqliteSettings, Func<string, string> setupDbPath)
        {
            InMemory = _memory.Any(x => x.ToLowerInvariant().Contains(sqlFile.ToLowerInvariant()));

            var realDbPath = setupDbPath.Invoke(sqlFile);
            if (sqliteSettings.BrowsableConnectionString) DatabasePath = realDbPath;
            
            var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder($"Data Source={realDbPath}")
            {
                Cache = sqliteSettings.Cache,
                Mode = sqliteSettings.Mode,
                BrowsableConnectionString = sqliteSettings.BrowsableConnectionString
            };

            _connectionString = sqliteConnectionStringBuilder.ConnectionString;
        }

        /// <summary>
        ///     Gets the path to the database.
        /// </summary>
        public string DatabasePath { get; }
        
        /// <summary>
        ///     Gets whether or not the database is in memory
        /// </summary>
        public bool InMemory { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Creates a new <see cref="SqliteConnection" /> and opens it.
        /// </summary>
        /// <returns>returns an open  <see cref="SqliteConnection" /></returns>
        public async Task<IDbConnection> GetOpenConnection()
        {
            if (_connection is null)
            {
                _connection = new SqliteConnection(_connectionString);
            }
            await _connection.OpenAsync().ConfigureAwait(false);
            return _connection;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}