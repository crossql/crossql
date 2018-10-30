using System;
using System.Linq.Expressions;

namespace crossql
{
    public interface IDbQuery<TModel, TPreviousJoin> : IDbQuery<TModel>
        where TModel : class, new() where TPreviousJoin : class, new()
    {
        IDbQuery<TModel, TPreviousJoin, TJoin> Join<TJoin>(Expression<Func<TModel, TPreviousJoin, object>> expression)
            where TJoin : class, new();

//        IDbQuery<TModel, TPreviousJoin> OrderBy(Expression<Func<TModel, TPreviousJoin, object>> orderByExpression);
//        IDbQuery<TModel, TPreviousJoin> OrderByDescending(Expression<Func<TPreviousJoin, object>> func);
//        IDbQuery<TModel, TPreviousJoin> Where(Expression<Func<TModel, TPreviousJoin, bool>> whereExpression);
//        IDbQuery<TModel, TPreviousJoin> Where(Expression<Func<TPreviousJoin, bool>> func);
//        IDbQuery<TModel, TPreviousJoin> OrderBy(Expression<Func<TPreviousJoin, object>> func);
    }
}