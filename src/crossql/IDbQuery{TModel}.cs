using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace crossql
{
    public interface IDbQuery<TModel> 
        where TModel : class, new()
    {
        Task<int> Count();
        Task Delete();
        
        IDbQuery<TModel, TJoin> Join<TJoin>(Expression<Func<TModel, object>> expression) 
            where TJoin : class, new();
    
        IDbQuery<TModel> OrderBy(Expression<Func<TModel, object>> expression);
        IDbQuery<TModel> OrderByDescending(Expression<Func<TModel, object>> expression);
        
        Task<IEnumerable<TModel>> Select();
        Task<IEnumerable<TResult>> Select<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc);
        IDbQuery<TModel> SkipTake(int skip, int take);
        string ToStringCount();
        string ToStringDelete();
        string ToStringTruncate();
        void Truncate();
        Task Update(TModel model);
        Task Update(TModel model, Func<TModel, IDictionary<string, object>> mapToDbParameters);
        IDbQuery<TModel> Where(Expression<Func<TModel, bool>> whereExpression);
    }
}