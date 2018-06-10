using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using crossql.Extensions;
using Microsoft.Data.Sqlite;

namespace crossql.sqlite
{
    public class Transactionable:TransactionableBase
    {
        private readonly SqliteSettings _settings;

        public Transactionable(IDbConnectionProvider provider, IDialect dialect, SqliteSettings settings) : base(provider, dialect)
        {
            _settings = settings;
        }

        public override async Task CreateOrUpdate<TModel>(TModel model, IDbMapper<TModel> dbMapper)
        {
            var tableName = typeof(TModel).GetTypeInfo().Name.BuildTableName();
            var fieldNameList = dbMapper.FieldNames;
            var commandParams = dbMapper.BuildDbParametersFrom(model);

            var parameters = "@" + string.Join(",@", fieldNameList);
            var fields = string.Join(",", fieldNameList);
            var commandText = string.Format(Dialect.CreateOrUpdate, tableName, fields, parameters);

            await ExecuteNonQuery(commandText, commandParams).ConfigureAwait(false);
            await UpdateManyToManyRelationsAsync(model, tableName, dbMapper).ConfigureAwait(false);        }

        public override async Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters)
        {
            using (var command = (SqliteCommand)Connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                EnableForeignKeys(command);
                command.CommandText = commandText;
                parameters.ForEach(
                    parameter =>
                        command.Parameters.Add(new SqliteParameter(parameter.Key,
                            parameter.Value ?? DBNull.Value)));

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private void EnableForeignKeys(IDbCommand command)
        {
            if (_settings.EnforceForeignKeys)
            {
                command.CommandText = "PRAGMA foreign_keys=ON";
                command.ExecuteNonQuery();
            }
        }
    }
}