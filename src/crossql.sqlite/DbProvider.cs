using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using crossql.Config;
using crossql.Extensions;
using crossql.Helpers;
using Microsoft.Data.Sqlite;

namespace crossql.sqlite
{
    public class DbProvider : DbProviderBase
    {
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly string _sqliteDatabasePath;
        private readonly SqliteSettings _settings;
        private IDialect _dialect;

        private void EnableForeignKeys(IDbCommand command)
        {
            if (!_settings.EnforceForeignKeys) return;
            
            command.CommandText = "PRAGMA foreign_keys=ON";
            command.ExecuteNonQuery();
        }
        public DbProvider(IDbConnectionProvider connectionProvider):this(connectionProvider,new SqliteSettings())
        {
        }

        public DbProvider(IDbConnectionProvider connectionProvider,SqliteSettings settings):this(connectionProvider,settings, null)
        {
        }

        public DbProvider(IDbConnectionProvider connectionProvider, SqliteSettings settings, Action<DbConfiguration> config):base(config)
        {
            _settings = settings;
            _connectionProvider = connectionProvider;
            _sqliteDatabasePath = ((DbConnectionProvider) connectionProvider).DatabasePath;
        }

        public override Task<bool> CheckIfDatabaseExists()
        {
            var exists = File.Exists(_sqliteDatabasePath);
            return Task.FromResult(exists);        }

        public override async Task<bool> CheckIfTableColumnExists(string tableName, string columnName)
        {
            var columnSql = await ExecuteScalar<string>(string.Format(Dialect.CheckTableColumnExists, tableName)).ConfigureAwait(false);
            return columnSql.Contains($"[{columnName}]");        }

        public override async Task<bool> CheckIfTableExists(string tableName)
        {
            var count = await ExecuteScalar<int>(string.Format(Dialect.CheckTableExists, tableName)).ConfigureAwait(false);
            return count > 0;        }

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

        public override async Task<TResult> ExecuteReader<TResult>(string commandText, IDictionary<string, object> parameters, Func<IDataReader, TResult> readerMapper)
        {
            using (var connection = await _connectionProvider.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var command = (SqliteCommand)connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                EnableForeignKeys(command);
                command.CommandText = commandText;
                parameters.ForEach(
                    parameter =>
                        command.Parameters.Add(new SqliteParameter(parameter.Key,
                            parameter.Value ?? DBNull.Value)));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    return readerMapper(reader);
                }
            }        }

        public override async Task<TKey> ExecuteScalar<TKey>(string commandText, IDictionary<string, object> parameters)
        {
            using (var connection = await _connectionProvider.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var command = (SqliteCommand)connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                EnableForeignKeys(command);
                command.CommandText = commandText;
                parameters.ForEach(
                    parameter =>
                        command.Parameters.Add(new SqliteParameter(parameter.Key,
                            parameter.Value ?? DBNull.Value)));

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
                if (typeof(TKey) == typeof(Guid))
                {
                    return (TKey)(object)new Guid((byte[])result);
                }

                if (typeof(TKey) == typeof(int))
                {
                    if (int.TryParse(result?.ToString(), out var intResult)) return (TKey) (object) intResult;
                    return (TKey)(object)0;
                }

                if (typeof(TKey) == typeof(DateTime))
                {
                    if (DateTime.TryParse(result.ToString(), out var dateTimeResult)) return (TKey) (object) dateTimeResult;
                    return (TKey)(object)DateTimeHelper.MinSqlValue;
                }

                return (TKey)result;
            }        }

        public override async Task RunInTransaction(Func<ITransactionable, Task> dbChange)
        {
            using (var transaction = new Transactionable(_connectionProvider, Dialect, _settings))
            {
                try
                {
                    await transaction.Initialize(true);
                    await dbChange(transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }        }

        public override async Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters)
        {
            using (var transactionable = new Transactionable(_connectionProvider, Dialect, _settings))
            {
                await transactionable.Initialize(false);
                await transactionable.ExecuteNonQuery(commandText, parameters);
            }
        }

        public override async Task Update<TModel>(TModel model, IDbMapper<TModel> dbMapper)
        {
            using (var transaction = new Transactionable(_connectionProvider, Dialect, _settings))
            {
                await transaction.Initialize(false);
                await transaction.Update(model, dbMapper);
            }
        }

        public override async Task CreateOrUpdate<TModel>(TModel model, IDbMapper<TModel> dbMapper)
        {
            using (var transaction = new Transactionable(_connectionProvider, Dialect, _settings))
            {
                await transaction.Initialize(false);
                await transaction.CreateOrUpdate(model,dbMapper);
            }
        }

        public override async Task Delete<TModel>(Expression<Func<TModel, bool>> expression)
        {
            using (var transactionable = new Transactionable(_connectionProvider, Dialect, _settings))
            {
                await transactionable.Initialize(false);
                await transactionable.Delete(expression);
            }
        }

        public sealed override IDialect Dialect => _dialect ?? (_dialect = new SqliteDialect());

        public override async  Task Create<TModel>(TModel model, IDbMapper<TModel> dbMapper)
        {
            using (var transaction = new Transactionable(_connectionProvider, Dialect, _settings))
            {
                await transaction.Initialize(false);
                await transaction.Create(model, dbMapper);
            }
        }
    }
}