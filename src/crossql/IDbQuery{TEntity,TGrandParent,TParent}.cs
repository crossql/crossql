using System;
using System.Linq.Expressions;

namespace crossql
{
    // ReSharper disable once UnusedTypeParameter
    public interface IDbQuery<TEntity, TGrandParent, TParent>
        where TEntity : class, new()
        where TGrandParent : class, new()
        where TParent : class, new()
    {
        IDbQuery<TEntity, TGrandParent, TJoin> Join<TJoin>(Expression<Func<TGrandParent, object>> expression)
            where TJoin : class, new();

        IDbQuery<TEntity, TGrandParent, TJoin> Join<TJoin>(Expression<Func<TEntity, TGrandParent, object>> expression)
            where TJoin : class, new();
    }
}