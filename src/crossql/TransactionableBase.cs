using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using crossql.Attributes;
using crossql.Extensions;

namespace crossql
{
    public abstract class TransactionableBase : ITransactionable, ITransactionRunner, IDisposable
    {
        protected readonly IDialect _Dialect;
        protected readonly IDbConnectionProvider _Provider;
        protected IDbConnection _Connection;
        protected IDbTransaction _Transaction;

        protected TransactionableBase(IDbConnectionProvider provider, IDialect dialect)
        {
            _Provider = provider;
            _Dialect = dialect;
        }

        public void Commit() => _Transaction.Commit();

        public async Task Create<TModel>(TModel model, IDbMapper<TModel> dbMapper) where TModel : class, new()
        {
            var tableName = typeof(TModel).BuildTableName();
            var fieldNameList = dbMapper.FieldNames;
            var commandParams = dbMapper.BuildDbParametersFrom(model);

            var parameters = "@" + string.Join(",@", fieldNameList);
            var fields = string.Join(",", fieldNameList);
            var commandText = string.Format(_Dialect.InsertInto, tableName, fields, parameters);

            await ExecuteNonQuery(commandText, commandParams).ConfigureAwait(false);

            await UpdateManyToManyRelationsAsync(model, tableName, dbMapper).ConfigureAwait(false);
        }

        public abstract Task CreateOrUpdate<TModel>(TModel model, IDbMapper<TModel> dbMapper) where TModel : class, new();

        public Task Delete<TModel>(Expression<Func<TModel, bool>> expression) where TModel : class, new()
        {
            var visitor = new WhereExpressionVisitor(_Dialect).Visit(expression);

            // this is a hard delete. soft deletes will happen in the repository layer.
            var tableName = typeof(TModel).BuildTableName();
            var whereClause = string.Format(_Dialect.Where, visitor.WhereExpression);
            var commandText = string.Format(_Dialect.DeleteFrom, tableName, whereClause);

            return ExecuteNonQuery(commandText, visitor.Parameters);
        }

        public void Dispose()
        {
            _Connection?.Dispose();
            _Transaction?.Dispose();
        }

        public abstract Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters);

        public async Task Initialize(bool useTransaction)
        {
            _Connection = await _Provider.GetOpenConnection();

            if (useTransaction)
                _Transaction = _Connection.BeginTransaction();
        }

        public void Rollback() => _Transaction.Rollback();

        public async Task Update<TModel>(TModel model, IDbMapper<TModel> dbMapper) where TModel : class, new()
        {
            var modelType = typeof(TModel);
            var identifierName = modelType.GetPrimaryKeyName();

            var tableName = typeof(TModel).BuildTableName();
            var fieldNameList = dbMapper.FieldNames;
            var commandParams = dbMapper.BuildDbParametersFrom(model);

            var setFieldText = fieldNameList.Select(field => string.Format("{1}{0}{2} = @{0}", field,_Dialect.OpenBrace,_Dialect.CloseBrace)).ToList();
            var whereClause = string.Format(_Dialect.Where, string.Format("{0} = @{0}", identifierName));
            var commandText = string.Format(_Dialect.Update, tableName, string.Join(",", setFieldText),
                whereClause);

            await ExecuteNonQuery(commandText, commandParams).ConfigureAwait(false);
            await UpdateManyToManyRelationsAsync(model, tableName, dbMapper).ConfigureAwait(false);
        }

        /// <summary>
        ///     Updates all Join Tables based on the <see cref="ManyToManyAttribute" />
        /// </summary>
        /// <typeparam name="TModel">Object model Type</typeparam>
        /// <param name="model">Actual object model</param>
        /// <param name="tableName">The name of the table</param>
        /// <param name="dbMapper">Used to map the data in the model object to parameters to be used in database calls</param>
        protected async Task UpdateManyToManyRelationsAsync<TModel>(TModel model, string tableName, IDbMapper<TModel> dbMapper) where TModel : class, new()
        {
            var primaryKey = model.GetType().GetPrimaryKeyName();
            var leftModel = dbMapper.BuildDbParametersFrom(model).FirstOrDefault(k => k.Key == primaryKey);
            var leftKey = typeof(TModel).Name.Replace("Model", string.Empty) + primaryKey;
            var parameters = new Dictionary<string, object> {{"@" + leftKey, leftModel.Value}};
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
                var deleteWhereClause = string.Format(_Dialect.Where, string.Format("{0} = @{0}", leftKey));
                var deleteCommandText = string.Format(_Dialect.DeleteFrom, joinTableName, deleteWhereClause);
                // Delete ALL records in the Join table associated with the `leftModel`
                await ExecuteNonQuery(deleteCommandText, parameters).ConfigureAwait(false);

                var manyToManyCollection = collection.PropertyType.GenericTypeArguments.FirstOrDefault();
                var listValues = (IEnumerable<object>) collection.GetValue(model, null);
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
                        var fieldsToInsert = string.Format(_Dialect.JoinFields, leftKey, rightKey);
                        // "[{0}], [{1}]"
                        var parametersToSet = string.Format(_Dialect.JoinParameters, leftKey, rightKey);
                        // "@{0}, @{1}"
                        var insertCommandText = string.Format(_Dialect.InsertInto, joinTableName,
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
            var names = new[] {tableName, joinTableName};
            Array.Sort(names, StringComparer.CurrentCulture);
            return string.Join("_", names);
        }
    }
}