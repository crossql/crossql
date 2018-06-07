using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using crossql.Config;
using crossql.Extensions;
using crossql;

namespace crossql.sqlite
{
    public abstract class SqliteDbProviderBase : DbProviderBase
    {
        protected const string RootSqlScriptPath = "FutureState.AppCore.Data.Sqlite.SqlScripts.";
        private IDialect _dialect;

        protected SqliteDbProviderBase(IDbConnectionProvider connectionProvider):base(connectionProvider) { }

        protected SqliteDbProviderBase(IDbConnectionProvider connectionProvider, Action<DbConfiguration> config) : base(connectionProvider, config) { }

        public sealed override IDialect Dialect => _dialect ?? (_dialect = new SqliteDialect());

        public override async Task CreateOrUpdate<TModel>(TModel model, IDbMapper<TModel> dbMapper)
        {
            var tableName = typeof(TModel).GetTypeInfo().Name.BuildTableName();
            var fieldNameList = dbMapper.FieldNames;
            var commandParams = dbMapper.BuildDbParametersFrom(model);

            var parameters = "@" + string.Join(",@", fieldNameList);
            var fields = string.Join(",", fieldNameList);
            var commandText = string.Format(Dialect.CreateOrUpdate, tableName, fields, parameters);

            await ExecuteNonQuery(commandText, commandParams).ConfigureAwait(false);
            await UpdateManyToManyRelationsAsync(model, tableName, dbMapper).ConfigureAwait(false);
        }

        public override async Task<string> LoadSqlFile<TDbProvider>(string fileName)
        {
            var sqlStatement = string.Empty;
            
            using ( var resourceStream = typeof( TDbProvider ).GetTypeInfo().Assembly.GetManifestResourceStream( RootSqlScriptPath + fileName ) )
            {
                if (resourceStream != null)
                {
                    sqlStatement = await new StreamReader(resourceStream).ReadToEndAsync().ConfigureAwait(false);
                }
            }

            return sqlStatement;
        }
    }
}