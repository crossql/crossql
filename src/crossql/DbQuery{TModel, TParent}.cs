using System;
using System.Linq.Expressions;
using crossql.Extensions;

namespace crossql
{
    public class DbQuery<TModel, TParent> : DbQuery<TModel>, IDbQuery<TModel, TParent> 
        where TModel : class, new()
        where TParent : class, new()
    {

        internal DbQuery(DbQuery<TModel> context, string joinTableName, Expression expression) 
            : base(context, joinTableName, expression) { }

        public IDbQuery<TModel, TParent, TJoinTo> Join<TJoinTo>(Expression<Func<TModel, TParent, object>> expression) where TJoinTo : class, new()
        {
            var joinTableName = typeof(TParent).BuildTableName();
            return new DbQuery<TModel, TParent, TJoinTo>(this, joinTableName, expression);
        }
    }
}