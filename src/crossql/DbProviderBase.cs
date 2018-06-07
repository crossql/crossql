using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using crossql.Attributes;
using crossql.Config;
using crossql.Extensions;

namespace crossql
{
    public abstract class DbProviderBase : IDbProvider
    {
        private readonly IDbConnectionProvider _connectionProvider;

        protected DbProviderBase(IDbConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        protected DbProviderBase(IDbConnectionProvider connectionProvider, Action<DbConfiguration> config) : this(connectionProvider)
        {
            var dbConfig = new DbConfiguration(this);
            config(dbConfig);
        }

        public abstract Task<bool> CheckIfDatabaseExists();

        public abstract Task<bool> CheckIfTableColumnExists(string tableName, string columnName);

        public abstract Task<bool> CheckIfTableExists(string tableName);

        /// <summary>
        ///     Create a new record based on a Model
        /// </summary>
        /// <typeparam name="TModel">Model Type</typeparam>
        /// <param name="model">Model Object</param>
        /// <returns>The uniqueidentifier (Guid) of the newly created record.</returns>
        public Task Create<TModel>(TModel model) where TModel : class, new() => Create(model, new AutoDbMapper<TModel>());

        /// <summary>
        ///     Create a new record based on a Model
        /// </summary>
        /// <typeparam name="TModel">Model Type</typeparam>
        /// <param name="model">Model Object</param>
        /// <param name="dbMapper"></param>
        /// <returns>The uniqueidentifier (Guid) of the newly created record.</returns>
        public async Task Create<TModel>(TModel model,
            IDbMapper<TModel> dbMapper)
            where TModel : class, new()
        {
            using (var connection = await _connectionProvider.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var command = connection.CreateCommand())
            using(var transaction = new TransactionBase(connection, null,command, Dialect))
            {
                await transaction.Create(model, dbMapper);
            }
        }

        public abstract Task CreateDatabase();


        /// <summary>
        ///     Update the record if it doesn't exist, otherwise create a new one.
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="model">Model Object to create or update</param>
        public Task CreateOrUpdate<TModel>(TModel model) where TModel : class, new() => CreateOrUpdate(model, new AutoDbMapper<TModel>());


        /// <summary>
        ///     Update the record if it doesn't exist, otherwise create a new one.
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="model">Model Object to create or update</param>
        /// <param name="dbMapper">Used to map the data in the model object to parameters to be used in database calls</param>
        public abstract Task CreateOrUpdate<TModel>(TModel model,
            IDbMapper<TModel> dbMapper)
            where TModel : class, new();

        public string DatabaseName { get; protected set; }

        /// <summary>
        ///     Delete the Database Record based on an expression
        /// </summary>
        /// <typeparam name="TModel">Model Type</typeparam>
        /// <param name="expression">The expression to use for the query</param>
        /// <remarks>THIS IS A HARD DELETE. When you run this method, the record is GONE!</remarks>
        public async Task Delete<TModel>(Expression<Func<TModel, bool>> expression)
            where TModel : class, new()
        {
            using (var connection = await _connectionProvider.GetOpenConnectionAsync().ConfigureAwait(false))
            using(var command = connection.CreateCommand())
            using (var transaction = new TransactionBase(connection, null,command, Dialect))
            {
                await transaction.Delete(expression);
            }
        }

        // Database specific stuff
        public abstract IDialect Dialect { get; }

        public abstract Task DropDatabase();

        // Used For Updates and Deletes
        public Task ExecuteNonQuery(string commandText) => ExecuteNonQuery(commandText, new Dictionary<string, object>());

        public abstract Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters);

        // Used for Finds and Gets
        public Task<TResult> ExecuteReader<TResult>(string commandText, Func<IDataReader, TResult> readerMapper) => ExecuteReader(commandText, new Dictionary<string, object>(), readerMapper);

        public abstract Task<TResult> ExecuteReader<TResult>(string commandText,
            IDictionary<string, object> parameters, Func<IDataReader, TResult> readerMapper);

        // Used for Creates
        public Task<TKey> ExecuteScalar<TKey>(string commandText) => ExecuteScalar<TKey>(commandText, new Dictionary<string, object>());

        public abstract Task<TKey> ExecuteScalar<TKey>(string commandText, IDictionary<string, object> parameters);

        public abstract Task<string> LoadSqlFile<TDbProvider>(string fileName);
        /// <summary>
        ///     Query the Database for ALL records.
        /// </summary>
        /// <typeparam name="TModel">Model Type</typeparam>
        /// <returns>IEnumerable model</returns>
        public IDbQuery<TModel> Query<TModel>() where TModel : class, new() => new DbQuery<TModel>(this, new AutoDbMapper<TModel>());
        public abstract Task RunInTransaction(Func<ITransactionable, Task> dbChange);

        /// <summary>
        ///     Query the Database for ALL records.
        /// </summary>
        /// <typeparam name="TModel">Model Type</typeparam>
        /// <typeparam name="TReturnType">The scalar return value</typeparam>
        /// <param name="propertyExpression">The expression to use for the query</param>
        /// <returns>IEnumerable model</returns>
        public IDbScalar<TModel, TReturnType> Scalar<TModel, TReturnType>(Expression<Func<TModel, TReturnType>> propertyExpression)
            where TModel : class, new() => new DbScalar<TModel, TReturnType>(this, propertyExpression);


        /// <summary>
        ///     Update the Database Record of a specified model.
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="model">Model Object to update</param>
        public Task Update<TModel>(TModel model) where TModel : class, new() => Update(model, new AutoDbMapper<TModel>());

        /// <summary>
        ///     Update the Database Record of a specified model.
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="model">Model Object to update</param>
        /// <param name="dbMapper">Used to map the data in the model object to parameters to be used in database calls</param>
        public async Task Update<TModel>(TModel model,
            IDbMapper<TModel> dbMapper)
            where TModel : class, new()
        {
            using (var connection = await _connectionProvider.GetOpenConnectionAsync().ConfigureAwait(false))
            using (var command = connection.CreateCommand())
            using (var transaction = new TransactionBase(connection, null,command, Dialect))
            {
                await transaction.Update(model, dbMapper);
            }
        }
        
        /// <summary>
        ///     Updates all Join Tables based on the <see cref="ManyToManyAttribute" />
        /// </summary>
        /// <typeparam name="TModel">Object model Type</typeparam>
        /// <param name="model">Actual object model</param>
        /// <param name="tableName">The name of the table</param>
        /// <param name="dbMapper">Used to map the data in the model object to parameters to be used in database calls</param>
        protected async Task UpdateManyToManyRelationsAsync<TModel>(TModel model,
            string tableName, IDbMapper<TModel> dbMapper) where TModel : class, new()
        {
            var primaryKey = model.GetType().GetPrimaryKeyName();
            var leftModel = dbMapper.BuildDbParametersFrom(model).FirstOrDefault(k => k.Key == primaryKey);
            var leftKey = typeof(TModel).Name.Replace("Model", string.Empty) + primaryKey;
            var parameters = new Dictionary<string, object> { { "@" + leftKey, leftModel.Value } };
            var manyToManyFields =
                typeof(TModel).GetRuntimeProperties()
                    .Where(property => property.GetCustomAttributes(true).Any(a => a.GetType().Name == nameof(ManyToManyAttribute)));

            foreach (var collection in manyToManyFields)
            {
                //                if (!IsGenericList(collection))
                //                {
                //                    throw new ArgumentException("The property must be an ICollection<>");
                //                }

                var joinTableName = GetJoinTableName(tableName, collection.Name);
                var deleteWhereClause = string.Format(Dialect.Where, string.Format("{0} = @{0}", leftKey));
                var deleteCommandText = string.Format(Dialect.DeleteFrom, joinTableName, deleteWhereClause);
                // Delete ALL records in the Join table associated with the `leftModel`
                await ExecuteNonQuery(deleteCommandText, parameters).ConfigureAwait(false);

                var manyToManyCollection = collection.PropertyType.GenericTypeArguments.FirstOrDefault();
                var listValues = (IEnumerable<object>)collection.GetValue(model, null);
                if (listValues == null) continue;

                foreach (var value in listValues.Distinct())
                {
                    if (manyToManyCollection == null)
                        throw new ArgumentException();
                    var rightProperties = manyToManyCollection.GetRuntimeProperties();
                    var manyToManyCollectionName = manyToManyCollection.Name.Replace("Model", string.Empty);
                    foreach (var rightProperty in rightProperties)
                    {
                        var rightPropertyName = rightProperty.Name;
                        if (rightPropertyName != primaryKey)
                            continue; // short circuit the loop if we're not dealing with the primary key.
                        var rightKey = manyToManyCollectionName + rightPropertyName;
                        var rightValue = rightProperty.GetValue(value, null);
                        parameters.Add("@" + rightKey, rightValue);
                        var fieldsToInsert = string.Format(Dialect.JoinFields, leftKey, rightKey);
                        // "[{0}], [{1}]"
                        var parametersToSet = string.Format(Dialect.JoinParameters, leftKey, rightKey);
                        // "@{0}, @{1}"
                        var insertCommandText = string.Format(Dialect.InsertInto, joinTableName,
                            fieldsToInsert,
                            parametersToSet);
                        await ExecuteNonQuery(insertCommandText, parameters).ConfigureAwait(false);
                        // Remove the parameter for the next iteration.
                        parameters.Remove("@" + rightKey);
                    }
                }
            }
        }

        private static string GetJoinTableName(string tableName, string joinTableName)
        {
            var names = new[] { tableName, joinTableName };
            Array.Sort(names, StringComparer.CurrentCulture);
            return string.Join("_", names);
        }
    }
}