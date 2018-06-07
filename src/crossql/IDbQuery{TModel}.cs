using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace crossql
{
    public interface IDbQuery<TModel> where TModel : class, new()
    {
        Task<int> CountAsync();
        Task DeleteAsync();
        Task<TModel> FirstAsync();
        Task<TResult> FirstAsync<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc);
        Task<TModel> FirstOrDefaultAsync();
        Task<TResult> FirstOrDefaultAsync<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc);
        IDbQuery<TModel, TJoinTo> Join<TJoinTo>() where TJoinTo : class, new();
        Task<TModel> LastAsync();
        Task<TResult> LastAsync<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc);
        Task<TModel> LastOrDefaultAsync();
        Task<TResult> LastOrDefaultAsync<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc);
        IDbQuery<TModel, TJoinTo> ManyToManyJoin<TJoinTo>() where TJoinTo : class, new();
        IDbQuery<TModel> OrderBy(Expression<Func<TModel, object>> orderByExpression, OrderDirection direction);
        Task<IEnumerable<TModel>> SelectAsync();
        Task<IEnumerable<TResult>> SelectAsync<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc);
        Task<TModel> SingleAsync();
        Task<TResult> SingleAsync<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc);
        Task<TModel> SingleOrDefaultAsync();
        Task<TResult> SingleOrDefaultAsync<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc);
        IDbQuery<TModel> SkipTake(int skip, int take);
        Task<IList<TModel>> ToListAsync();
        Task<IList<TResult>> ToListAsync<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc);
        string ToStringCount();
        string ToStringDelete();
        string ToStringTruncate();
        void Truncate();
        Task UpdateAsync(TModel model);
        Task UpdateAsync(TModel model, Func<TModel, IDictionary<string, object>> mapToDbParameters);
        IDbQuery<TModel> Where(Expression<Func<TModel, bool>> whereExpression);
    }
}