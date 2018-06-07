using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using crossql.Config;
using crossql.Extensions;
using crossql.Helpers;

namespace crossql.mssqlserver
{
    public class DbProvider : DbProviderBase
    {
        private static string _useStatement;
        private readonly IDbConnectionProvider _connectionProvider;
        private IDialect _dialect;

        public DbProvider(IDbConnectionProvider connectionProvider, string databaseName) : base(connectionProvider)
        {
            DatabaseName = databaseName;
            _connectionProvider = connectionProvider;
            _useStatement = string.Format(Dialect.UseDatabase, databaseName);
        }

        public DbProvider(IDbConnectionProvider connectionProvider, string databaseName, Action<DbConfiguration> dbConfig) : base(connectionProvider, dbConfig)
        {
            DatabaseName = databaseName;
            _connectionProvider = connectionProvider;
            _useStatement = string.Format(Dialect.UseDatabase, databaseName);
        }

        public override async Task CreateOrUpdate<TModel>(TModel model, IDbMapper<TModel> dbMapper)
        {
            using (var connection = await _connectionProvider.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var command = connection.CreateCommand())
            using (var transaction = new Transaction(connection, null, command, Dialect, DatabaseName))
            {
                await transaction.CreateOrUpdate(model, dbMapper);
            }
        }

        public sealed override IDialect Dialect => _dialect ?? (_dialect = new SqlServerDialect());

        public override async Task<string> LoadSqlFile<TDbProvider>(string fileName)
        {
            var sqlStatement = string.Empty;

            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            {
                if (resourceStream != null)
                {
                    sqlStatement = await new StreamReader(resourceStream).ReadToEndAsync().ConfigureAwait(false);
                }
            }

            return sqlStatement;
        }

        public override async Task RunInTransaction(Func<ITransactionable, Task> dbChange)
        {
            using (var connection = await _connectionProvider.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var trans = connection.BeginTransaction())
            using (var command = connection.CreateCommand())
            using (var transaction = new Transaction(connection, trans, command, Dialect, DatabaseName))
            {
                await dbChange(transaction);
            }
        }

        public override async Task<bool> CheckIfDatabaseExists() => await ExecuteScalarAsync<int>("", string.Format(Dialect.CheckDatabaseExists, DatabaseName)).ConfigureAwait(false) == 1;

        public override Task CreateDatabase() => ExecuteNonQuery("", string.Format(Dialect.CreateDatabase, DatabaseName), new Dictionary<string,object>());

        public override Task DropDatabase() => ExecuteNonQuery("", string.Format(Dialect.DropDatabase, DatabaseName), new Dictionary<string,object>());

        public override async Task<bool> CheckIfTableExists(string tableName) => await ExecuteScalar<int>(string.Format(Dialect.CheckTableExists, tableName)).ConfigureAwait(false) == 1;

        public override async Task<bool> CheckIfTableColumnExists(string tableName, string columnName) => await ExecuteScalar<int>(string.Format(Dialect.CheckTableColumnExists, tableName, columnName)).ConfigureAwait(false) == 1;

        public override Task<TResult> ExecuteReader<TResult>(string commandText, IDictionary<string, object> parameters, Func<IDataReader, TResult> readerMapper) => ExecuteReader(_useStatement, commandText, parameters, readerMapper);

        private async Task<TResult> ExecuteReader<TResult>(string useStatement, string commandText, IEnumerable<KeyValuePair<string, object>> parameters, Func<IDataReader, TResult> readerMapper)
        {
            using (var connection = await _connectionProvider.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = useStatement + commandText;
                parameters.ForEach(parameter => command.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value ?? DBNull.Value)));
                TResult result;
                using (var reader = command.ExecuteReader())
                {
                    result = readerMapper(reader);
                }
                return result;
            }
        }

        public override Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters) => ExecuteNonQuery(_useStatement, commandText, parameters);

        private async Task ExecuteNonQuery(string useStatement, string commandText, IDictionary<string, object> parameters)
        {
            using (var connection = await _connectionProvider.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var command = connection.CreateCommand())
            using (var transaction = new Transaction(connection, null, command, Dialect, DatabaseName))
            {
                await transaction.ExecuteNonQuery(useStatement + commandText, parameters);
            }
        }

        public override Task<TKey> ExecuteScalar<TKey>(string commandText, IDictionary<string, object> parameters) => ExecuteScalarAsync<TKey>(_useStatement, commandText, parameters);

        private Task<TKey> ExecuteScalarAsync<TKey>(string useStatement, string commandText) => ExecuteScalarAsync<TKey>(useStatement, commandText, new Dictionary<string, object>());

        private async Task<TKey> ExecuteScalarAsync<TKey>(string useStatement, string commandText, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            using (var connection = await _connectionProvider.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = useStatement + commandText;
                parameters.ForEach(
                    parameter =>
                        command.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value ?? DBNull.Value)));

                var result = command.ExecuteScalar();
                if (typeof(TKey) == typeof(int))
                    return (TKey)(result ?? 0);

                if (typeof(TKey) == typeof(DateTime))
                {
                    if (!DateTime.TryParse(result.ToString(), out var _))
                    {
                        return (TKey)(object)DateTimeHelper.MinSqlValue;
                    }

                    return (TKey)result;
                }

                return (TKey)result;
            }
        }
    }
}