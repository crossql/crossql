using System;
using System.Linq.Expressions;

namespace crossql
{
    // ReSharper disable once UnusedTypeParameter
    public interface IDbQuery<TModel, TGrandParent, TParent> : IDbQuery<TModel, TParent>
        where TModel : class, new()
        where TGrandParent : class, new()
        where TParent : class, new()
    {
        IDbQuery<TModel, TGrandParent, TJoin> Join<TJoin>(Expression<Func<TGrandParent, object>> expression)
            where TJoin : class, new();

        IDbQuery<TModel, TGrandParent, TJoin> Join<TJoin>(Expression<Func<TModel, TGrandParent, object>> expression)
            where TJoin : class, new();
    }
}