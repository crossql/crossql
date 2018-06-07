using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace crossql
{
    public interface IDbProvider : IDataModifier
    {
        string DatabaseName { get; }
        IDialect Dialect { get; }
        Task<bool> CheckIfDatabaseExists();
        Task<bool> CheckIfTableColumnExists(string tableName, string columnName);
        Task<bool> CheckIfTableExists(string tableName);
        Task DropDatabase();
        Task<TResult> ExecuteReader<TResult>(string commandText, Func<IDataReader, TResult> readerMapper);
        Task<TResult> ExecuteReader<TResult>(string commandText, IDictionary<string, object> parameters, Func<IDataReader, TResult> readerMapper);
        Task<TKey> ExecuteScalar<TKey>(string commandText);
        Task<TKey> ExecuteScalar<TKey>(string commandText, IDictionary<string, object> parameters);
        Task<string> LoadSqlFile<TDbProvider>(string fileName);
        IDbQuery<TModel> Query<TModel>() where TModel : class, new();
        Task RunInTransaction(Func<IDataModifier,Task> dbChange);
        IDbScalar<TModel, TReturnType> Scalar<TModel, TReturnType>(Expression<Func<TModel, TReturnType>> propertyExpression) where TModel : class, new();

    }
}