using System;
using System.Linq.Expressions;
using crossql.Extensions;

namespace crossql
{
    public class DbQuery<TModel, TGrandParent, TParent> : DbQuery<TModel, TParent>, IDbQuery<TModel, TGrandParent, TParent>
        where TModel : class, new()
        where TGrandParent : class, new()
        where TParent : class, new()
    {
        internal DbQuery(DbQuery<TModel>context, string joinTableName, Expression expression) 
            : base(context, joinTableName, expression) { }

        public IDbQuery<TModel, TGrandParent, TJoinTo> Join<TJoinTo>(Expression<Func<TGrandParent, object>> expression) where TJoinTo : class, new()
        {
            var joinTableName = typeof(TParent).BuildTableName();
            return new DbQuery<TModel, TGrandParent, TJoinTo>(this, joinTableName, expression);
        }

        public IDbQuery<TModel, TGrandParent, TJoinTo> Join<TJoinTo>(Expression<Func<TModel, TGrandParent, object>> expression) where TJoinTo : class, new()
        {
            var joinTableName = typeof(TParent).BuildTableName();
            return new DbQuery<TModel, TGrandParent, TJoinTo>(this, joinTableName, expression);
        }
    }
}