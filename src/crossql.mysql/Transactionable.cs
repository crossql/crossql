using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using crossql.Extensions;
using MySql.Data.MySqlClient;

// ReSharper disable AccessToDisposedClosure

namespace crossql.mysql
{
    public class Transactionable : TransactionableBase
    {
        private readonly string _databaseName;
        private string _useStatement;

        public Transactionable(IDbConnectionProvider provider, IDialect dialect, string databaseName) : base(provider, dialect)
        {
            _databaseName = databaseName;
            EnableUseStatement();
        }

        public override async Task CreateOrUpdate<TModel>(TModel model, IDbMapper<TModel> dbMapper)
        {
            var modelType = typeof(TModel);
            var tableName = typeof(TModel).BuildTableName();
            var fieldNameList = dbMapper.FieldNames;
            var commandParams = dbMapper.BuildDbParametersFrom(model);

            var insertParams = "@" + string.Join(",@", fieldNameList);
            var insertFields = string.Join(",", fieldNameList);
            var updateFields = string.Join(",", fieldNameList.Select(field => string.Format("`{0}` = @{0}", field)).ToList());
            var whereClause = string.Format(_Dialect.Where, string.Format("{0} = @{0}", modelType.GetPrimaryKeyName()));

            var commandText = string.Format(_Dialect.CreateOrUpdate,
                tableName,
                insertFields,
                insertParams,
                updateFields);
            await ExecuteNonQuery(commandText, commandParams).ConfigureAwait(false);
            await UpdateManyToManyRelationsAsync(model, tableName, dbMapper).ConfigureAwait(false);
        }

        public override Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters) => ExecuteNonQuery(_useStatement, commandText, parameters);

        private Task ExecuteNonQuery(string useStatement, string commandText, IDictionary<string, object> parameters)
        {
            using (var command = _Connection.CreateCommand())
            {
                if (_Transaction != null) command.Transaction = _Transaction;

                command.CommandType = CommandType.Text;
                command.CommandText = useStatement + commandText;
                parameters.ForEach(param => command.Parameters.Add(new MySqlParameter(param.Key, param.Value ?? DBNull.Value)));

                try
                {
                    command.ExecuteNonQuery();
                }
                catch(Exception ex)
                {
                    _Transaction?.Rollback();
                    throw;
                }
            }
            return Task.CompletedTask;
        }

        internal void DisableUseStatement() => _useStatement = string.Empty;

        internal void EnableUseStatement() => _useStatement = string.Format(_Dialect.UseDatabase, _databaseName);
    }
}