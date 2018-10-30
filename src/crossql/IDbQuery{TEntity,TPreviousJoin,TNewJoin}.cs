using System;
using System.Linq.Expressions;

namespace crossql
{
    public interface IDbQuery<TEntity, TPreviousJoin, TNewJoin>
        where TEntity : class, new()
        where TPreviousJoin : class, new()
        where TNewJoin : class, new()
    {
        IDbQuery<TEntity, TPreviousJoin, TJoin> Join<TJoin>(Expression<Func<TPreviousJoin, object>> func)
            where TJoin : class, new();

        IDbQuery<TEntity, TPreviousJoin, TJoin> Join<TJoin>(Expression<Func<TEntity, TPreviousJoin, object>> func)
            where TJoin : class, new();

//        IDbQuery<TEntity, TPreviousJoin, TNewJoin> Where(Expression<Func<TNewJoin, bool>> func);
//        IDbQuery<TEntity, TPreviousJoin, TNewJoin> OrderBy(Expression<Func<TNewJoin, object>> func);
//        IDbQuery<TEntity, TPreviousJoin, TNewJoin> OrderByDescending(Expression<Func<TNewJoin, object>> func);
    }
}