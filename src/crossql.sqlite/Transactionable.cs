using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using crossql.Extensions;
using Microsoft.Data.Sqlite;

// ReSharper disable AccessToDisposedClosure

namespace crossql.sqlite
{
    public class Transactionable : TransactionableBase
    {
        public Transactionable(IDbConnectionProvider provider, IDialect dialect) : base(provider, dialect)
        {
        }

        public override async Task CreateOrUpdate<TModel>(TModel model, IDbMapper<TModel> dbMapper)
        {
            var tableName = typeof(TModel).BuildTableName();
            var fieldNameList = dbMapper.FieldNames;
            var commandParams = dbMapper.BuildDbParametersFrom(model);

            var parameters = "@" + string.Join(",@", fieldNameList);
            var fields = string.Join(",", fieldNameList);
            var commandText = string.Format(_Dialect.CreateOrUpdate, tableName, fields, parameters);

            await ExecuteNonQuery(commandText, commandParams).ConfigureAwait(false);
            await UpdateManyToManyRelationsAsync(model, tableName, dbMapper).ConfigureAwait(false);
        }

        public override async Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters)
        {
            using (var command = (SqliteCommand) _Connection.CreateCommand())
            {
                if (_Transaction != null) command.Transaction = (SqliteTransaction) _Transaction;

                command.CommandType = CommandType.Text;
                command.CommandText = commandText;
                parameters.ForEach(
                    parameter =>
                        command.Parameters.Add(new SqliteParameter(parameter.Key,
                            parameter.Value ?? DBNull.Value)));

                try
                {
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                catch
                {
                    _Transaction?.Rollback();
                    throw;
                }
            }
        }
    }
}