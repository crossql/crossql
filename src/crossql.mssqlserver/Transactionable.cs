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
            var tableName = typeof(TModel).GetTypeInfo().Name.BuildTableName();
            var fieldNameList = dbMapper.FieldNames;
            var commandParams = dbMapper.BuildDbParametersFrom(model);

            var insertParams = "@" + string.Join(",@", fieldNameList);
            var insertFields = string.Join(",", fieldNameList);
            var updateFields = string.Join(",", fieldNameList.Select(field => string.Format("[{0}] = @{0}", field)).ToList());
            var whereClause = string.Format(Dialect.Where, string.Format("{0} = @{0}", modelType.GetPrimaryKeyName()));

            var commandText = string.Format(Dialect.CreateOrUpdate,
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
                if (Transaction != null) Command.Transaction = Transaction;

                Command.CommandType = CommandType.Text;
                Command.CommandText = useStatement + commandText;
                parameters.ForEach(param=> Command.Parameters.Add(new SqlParameter(param.Key, param.Value ?? DBNull.Value)));
            
                Command.ExecuteNonQuery();
            });
        }

        internal void DisableUseStatement() => _useStatement = string.Empty;

        internal void EnableUseStatement() => _useStatement = string.Format(Dialect.UseDatabase, _databaseName);
    }
}