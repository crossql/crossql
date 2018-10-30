using System;
using System.Linq.Expressions;

namespace crossql
{
    public interface IDbQuery<TModel, TParent> : IDbQuery<TModel>
        where TModel : class, new() where TParent : class, new()
    {
        IDbQuery<TModel, TParent, TJoinTo> Join<TJoinTo>(Expression<Func<TModel, TParent, object>> expression)
            where TJoinTo : class, new();
    }
}