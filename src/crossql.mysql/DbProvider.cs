using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using crossql.Config;
using crossql.Extensions;
using crossql.Helpers;
using MySql.Data.MySqlClient;

// ReSharper disable AccessToDisposedClosure

namespace crossql.mysql
{
    public class DbProvider : DbProviderBase
    {
        private static string _useStatement;
        private readonly IDbConnectionProvider _connectionProvider;

        public DbProvider(IDbConnectionProvider connectionProvider, string databaseName) : this(connectionProvider, databaseName, null) { }

        public DbProvider(IDbConnectionProvider connectionProvider, string databaseName, Action<DbConfiguration> dbConfig) : base(dbConfig)
        {
            DatabaseName = databaseName;
            _connectionProvider = connectionProvider;
            _useStatement = string.Format(Dialect.UseDatabase, databaseName);
        }

        public sealed override IDialect Dialect => _Dialect ?? (_Dialect = new MySqlDialect());

        public override async Task<bool> CheckIfDatabaseExists() =>
            await ExecuteScalar<int>("", string.Format(Dialect.CheckDatabaseExists, DatabaseName)).ConfigureAwait(false) == 1;

        public override async Task<bool> CheckIfTableColumnExists(string tableName, string columnName) =>
            await ExecuteScalar<int>(string.Format(Dialect.CheckTableColumnExists, tableName, columnName)).ConfigureAwait(false) == 1;

        public override async Task<bool> CheckIfTableExists(string tableName) =>
            await ExecuteScalar<int>(string.Format(Dialect.CheckTableExists, tableName)).ConfigureAwait(false) == 1;

        public override Task CreateDatabase() => ExecuteNonQuery("", string.Format(Dialect.CreateDatabase, DatabaseName), new Dictionary<string, object>());

        public override Task DropDatabase() => ExecuteNonQuery("", string.Format(Dialect.DropDatabase, DatabaseName), new Dictionary<string, object>());

        public override Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters) => ExecuteNonQuery(_useStatement, commandText, parameters);

        public override Task<TResult> ExecuteReader<TResult>(string commandText, IDictionary<string, object> parameters, Func<IDataReader, TResult> readerMapper) =>
            ExecuteReader(_useStatement, commandText, parameters, readerMapper);

        public override Task<TKey> ExecuteScalar<TKey>(string commandText, IDictionary<string, object> parameters) =>
            ExecuteScalar<TKey>(_useStatement, commandText, parameters);

        protected override TransactionableBase GetNewTransaction() => new Transactionable(_connectionProvider, Dialect, DatabaseName);

        private static MySqlParameter CreateSqlParameter(KeyValuePair<string, object> parameter) => new MySqlParameter(parameter.Key, parameter.Value ?? DBNull.Value);

        private async Task ExecuteNonQuery(string useStatement, string commandText, IDictionary<string, object> parameters)
        {
            using (var transactionable = new Transactionable(_connectionProvider, Dialect, DatabaseName))
            {
                try
                {
                    await transactionable.Initialize(false);
                    if (string.IsNullOrWhiteSpace(useStatement))
                        transactionable.DisableUseStatement();

                    await transactionable.ExecuteNonQuery(commandText, parameters);
                }
                finally
                {
                    transactionable.EnableUseStatement();
                }
            }
        }

        private async Task<TResult> ExecuteReader<TResult>(string useStatement, string commandText, IEnumerable<KeyValuePair<string, object>> parameters,
            Func<IDataReader, TResult> readerMapper)
        {
            using (var connection = await _connectionProvider.GetOpenConnection().ConfigureAwait(false))
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = useStatement + commandText;
                parameters.ForEach(p => command.Parameters.Add(CreateSqlParameter(p)));
                TResult result;
                using (var reader = command.ExecuteReader())
                {
                    result = readerMapper(reader);
                }

                return result;
            }
        }

        private Task<TKey> ExecuteScalar<TKey>(string useStatement, string commandText) =>
            ExecuteScalar<TKey>(useStatement, commandText, new Dictionary<string, object>());

        private async Task<TKey> ExecuteScalar<TKey>(string useStatement, string commandText, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            using (var connection = await _connectionProvider.GetOpenConnection().ConfigureAwait(false))
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = useStatement + commandText;
                parameters.ForEach(p => command.Parameters.Add(CreateSqlParameter(p)));

                var result = command.ExecuteScalar();
                if (typeof(TKey) == typeof(int))
                {
                    switch (result)
                    {
                        case null:
                            return (TKey) (object)0;
                        case int _:
                            return (TKey)result;
                        case long bigInt:
                            return (TKey)(object) int.Parse(bigInt.ToString());
                    }
                }
                
                if (typeof(TKey) == typeof(DateTime))
                {
                    if (!DateTime.TryParse(result.ToString(), out _)) return (TKey) (object) DateTimeHelper.MinSqlValue;
                    return (TKey)result;
                }

                return (TKey) result;
            }
        }
    }
}