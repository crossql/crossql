using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using crossql.Config;
using crossql.Extensions;
using crossql.Helpers;
using Microsoft.Data.Sqlite;

// ReSharper disable AccessToDisposedClosure

namespace crossql.sqlite
{
    public class DbProvider : DbProviderBase
    {
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly SqliteSettings _settings;
        private readonly string _sqliteDatabasePath;
        private IDialect _dialect;

        public DbProvider(IDbConnectionProvider connectionProvider) : this(connectionProvider, new SqliteSettings()) { }

        public DbProvider(IDbConnectionProvider connectionProvider, SqliteSettings settings) : this(connectionProvider, settings, null) { }

        public DbProvider(IDbConnectionProvider connectionProvider, Action<DbConfiguration> config) : this(connectionProvider, SqliteSettings.Default, config) { }

        public DbProvider(IDbConnectionProvider connectionProvider, SqliteSettings settings, Action<DbConfiguration> config) : base(config)
        {
            _settings = settings;
            _connectionProvider = connectionProvider;
            _sqliteDatabasePath = ((DbConnectionProvider) connectionProvider).DatabasePath;
        }

        public sealed override IDialect Dialect => _dialect ?? (_dialect = new SqliteDialect());

        public override Task<bool> CheckIfDatabaseExists() => Task.FromResult(File.Exists(_sqliteDatabasePath));

        public override async Task<bool> CheckIfTableColumnExists(string tableName, string columnName)
        {
            var columnSql = await ExecuteScalar<string>(string.Format(Dialect.CheckTableColumnExists, tableName)).ConfigureAwait(false);
            return columnSql.Contains($"[{columnName}]");
        }

        public override async Task<bool> CheckIfTableExists(string tableName)
        {
            var count = await ExecuteScalar<int>(string.Format(Dialect.CheckTableExists, tableName)).ConfigureAwait(false);
            return count > 0;
        }

        public override Task CreateDatabase()
        {
            var file = File.Create(_sqliteDatabasePath);
            file.Close();
            return Task.CompletedTask;
        }

        public override Task DropDatabase()
        {
            File.Delete(_sqliteDatabasePath);
            return Task.CompletedTask;
        }

        public override async Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters)
        {
            using (var transactionable = new Transactionable(_connectionProvider, Dialect, _settings))
            {
                await transactionable.Initialize(false);
                await transactionable.ExecuteNonQuery(commandText, parameters);
            }
        }

        public override async Task<TResult> ExecuteReader<TResult>(string commandText, IDictionary<string, object> parameters, Func<IDataReader, TResult> readerMapper)
        {
            using (var connection = await _connectionProvider.GetOpenConnection().ConfigureAwait(false))
            using (var command = (SqliteCommand) connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                EnableForeignKeys(command);
                command.CommandText = commandText;
                parameters.ForEach(p => command.Parameters.Add(CreateParameter(p)));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    return readerMapper(reader);
                }
            }
        }

        public override async Task<TKey> ExecuteScalar<TKey>(string commandText, IDictionary<string, object> parameters)
        {
            using (var connection = await _connectionProvider.GetOpenConnection().ConfigureAwait(false))
            using (var command = (SqliteCommand) connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                EnableForeignKeys(command);
                command.CommandText = commandText;

                parameters.ForEach(p => command.Parameters.Add(CreateParameter(p)));

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
                if (typeof(TKey) == typeof(Guid)) return (TKey) (object) new Guid((byte[]) result);

                if (typeof(TKey) == typeof(int))
                {
                    if (int.TryParse(result?.ToString(), out var intResult)) return (TKey) (object) intResult;
                    return (TKey) (object) 0;
                }

                if (typeof(TKey) == typeof(DateTime))
                {
                    if (DateTime.TryParse(result.ToString(), out var dateTimeResult)) return (TKey) (object) dateTimeResult;
                    return (TKey) (object) DateTimeHelper.MinSqlValue;
                }

                return (TKey) result;
            }
        }

        protected override TransactionableBase GetNewTransaction() => new Transactionable(_connectionProvider, Dialect, _settings);

        private static SqliteParameter CreateParameter(KeyValuePair<string, object> parameter) => new SqliteParameter(parameter.Key,
            parameter.Value ?? DBNull.Value);

        private void EnableForeignKeys(IDbCommand command)
        {
            if (!_settings.EnforceForeignKeys) return;

            command.CommandText = "PRAGMA foreign_keys=ON";
            command.ExecuteNonQuery();
        }
    }
}