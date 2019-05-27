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
    public class DbProvider : DbProviderBase, IDisposable
    {
        private readonly DbConnectionProvider _connectionProvider;
        private readonly string _sqliteDatabasePath;

        public DbProvider():this(DbConnectionProvider.Default) { }
        public DbProvider(IDbConnectionProvider connectionProvider) : this(connectionProvider, null) { }

        public DbProvider(IDbConnectionProvider connectionProvider,  Action<DbConfiguration> config) : base(config)
        {
            _connectionProvider = (DbConnectionProvider)connectionProvider;
            _sqliteDatabasePath = _connectionProvider.DatabasePath;
        }

        public bool InMemory => _connectionProvider.InMemory;
        public sealed override IDialect Dialect => _Dialect ?? (_Dialect = new SqliteDialect());

        public override Task<bool> CheckIfDatabaseExists() 
            => Task.FromResult(_connectionProvider.InMemory || File.Exists(_sqliteDatabasePath));

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
            if (!_connectionProvider.InMemory)
            {
                var file = File.Create(_sqliteDatabasePath);
                file.Close();   
            }
            return Task.CompletedTask;
        }

        public override Task DropDatabase()
        {
            _connectionProvider.Dispose();
            if (!_connectionProvider.InMemory)
                File.Delete(_sqliteDatabasePath);
            return Task.CompletedTask;
        }

        public override async Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters)
        {
            using (var transactionable = new Transactionable(_connectionProvider, Dialect))
            {
                await transactionable.Initialize(false);
                await transactionable.ExecuteNonQuery(commandText, parameters);
            }
        }

        public override async Task<TResult> ExecuteReader<TResult>(string commandText, IDictionary<string, object> parameters, Func<IDataReader, TResult> readerMapper)
        {
            var connection = await _connectionProvider.GetOpenConnection().ConfigureAwait(false);
            using (var command = (SqliteCommand) connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = commandText;
                parameters.ForEach(p => command.Parameters.Add(CreateParameter(p)));

                TResult result;
                using (var reader = await command.ExecuteReaderAsync())
                {
                    result = readerMapper(reader);
                }

                if (!_connectionProvider.InMemory)
                {
                    connection.Close();
                    connection.Dispose();
                }

                return result;
            }
        }

        public override async Task<TKey> ExecuteScalar<TKey>(string commandText, IDictionary<string, object> parameters)
        {
            var connection = await _connectionProvider.GetOpenConnection().ConfigureAwait(false);
            using (var command = (SqliteCommand) connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
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

                if (!_connectionProvider.InMemory)
                {
                    connection.Close();
                    connection.Dispose();
                }
                
                return (TKey) result;
            }
        }

        /// <summary>
        /// Backup your current database to a new one
        /// </summary>
        /// <param name="destinationConnectionProvider">Destination db connection provider</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown when attempting to back up to an in-memory database</exception>
        public async Task BackupDatabase(DbConnectionProvider destinationConnectionProvider)
        {
            if(destinationConnectionProvider.InMemory)
                throw new NotSupportedException("You cannot backup to an in-memory database.");
           
            if(File.Exists(destinationConnectionProvider.DatabasePath))
                File.Delete(destinationConnectionProvider.DatabasePath);
            
            var file = File.Create(destinationConnectionProvider.DatabasePath);
            file.Close();
            
            var destinationConnection = await destinationConnectionProvider.GetOpenConnection().ConfigureAwait(false);
            var destination = (SqliteConnection) destinationConnection;

            var sourceConnection = await _connectionProvider.GetOpenConnection();
            var source = (SqliteConnection) sourceConnection;
            
            source.BackupDatabase(destination);
            destination.Close();
            destination.Dispose();
        }
        
        public async Task RunInMemoryTransaction(Func<ITransactionable, Task> transaction)
        {
            var memoryDbConnectionProvider = new DbConnectionProvider(":memory:");
            var memoryConnection = await memoryDbConnectionProvider.GetOpenConnection();
            var memoryDbProvider = new DbProvider(memoryDbConnectionProvider);
            var currentConnection = await _connectionProvider.GetOpenConnection();
            var currentSqliteConnection = (SqliteConnection) currentConnection;
            
            currentSqliteConnection.BackupDatabase((SqliteConnection)memoryConnection);

            await memoryDbProvider.RunInTransaction(transaction);

            _connectionProvider.Dispose();
            await memoryDbProvider.BackupDatabase(_connectionProvider);
        }

        public void Dispose()
        {
            _connectionProvider?.Dispose();
        }

        protected override TransactionableBase GetNewTransaction() => new Transactionable(_connectionProvider, Dialect);

        private static SqliteParameter CreateParameter(KeyValuePair<string, object> parameter) => new SqliteParameter(parameter.Key,
            parameter.Value ?? DBNull.Value);
    }
}