using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using crossql.Extensions;

namespace crossql.mssqlserver
{
    public class Transaction : TransactionBase
    {
        private readonly string _databaseName;
        private string _useStatement;

        public Transaction(IDbConnection connection, IDbTransaction transaction,IDbCommand command, IDialect dialect, string databaseName) : base(connection, transaction,command, dialect)
        {
            _databaseName = databaseName;
            EnableUseStatement();
        }

        public override async Task CreateOrUpdate<TModel>(TModel model, IDbMapper<TModel> dbMapper)
        {  
            var modelType = typeof(TModel);
            var tableName = typeof(TModel).GetTypeInfo().Name.BuildTableName();
            var fieldNameList = dbMapper.FieldNames;
            var commandParams = dbMapper.BuildDbParametersFrom(model);

            var insertParams = "@" + string.Join(",@", fieldNameList);
            var insertFields = string.Join(",", fieldNameList);
            var updateFields = string.Join(",", fieldNameList.Select(field => string.Format("[{0}] = @{0}", field)).ToList());
            var whereClause = string.Format(_dialect.Where, string.Format("{0} = @{0}", modelType.GetPrimaryKeyName()));

            var commandText = string.Format(_dialect.CreateOrUpdate,
                tableName,
                updateFields,
                whereClause,
                insertFields,
                insertParams);

            await ExecuteNonQuery(commandText, commandParams).ConfigureAwait(false);
            await UpdateManyToManyRelationsAsync(model, tableName, dbMapper).ConfigureAwait(false);
        }

        public override Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters) => ExecuteNonQuery(_useStatement, commandText, parameters);

        private async Task ExecuteNonQuery(string useStatement, string commandText, IDictionary<string, object> parameters)
        {
            await Task.Run(() => { 
                if (_command.Transaction != null) _command.Transaction = _transaction;

                _command.CommandType = CommandType.Text;
                _command.CommandText = useStatement + commandText;
                Parallel.ForEach(parameters, param => _command.Parameters.Add(new SqlParameter(param.Key, param.Value ?? DBNull.Value)));
            
                _command.ExecuteNonQuery();
            });
        }

        internal void DisableUseStatement()
        {
            _useStatement = "";

        }

        internal void EnableUseStatement()
        {
            _useStatement = string.Format(_dialect.UseDatabase, _databaseName);        }
    }
}