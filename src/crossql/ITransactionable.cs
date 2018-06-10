using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace crossql
{
    public interface ITransactionable
    {
        Task Create<TModel>(TModel model, IDbMapper<TModel> dbMapper) where TModel : class, new();
        Task CreateOrUpdate<TModel>(TModel model, IDbMapper<TModel> dbMapper) where TModel : class, new();
        Task Delete<TModel>(Expression<Func<TModel, bool>> expression) where TModel : class, new();
        Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters);
        Task Update<TModel>(TModel model, IDbMapper<TModel> dbMapper) where TModel : class, new();
    }
}