using System;
using System.Linq.Expressions;

namespace crossql
{
    public class DbQuery<TModel, TParent, TJoinTo> : DbQuery<TModel, TJoinTo>, IDbQuery<TModel, TParent, TJoinTo>
        where TModel : class, new()
        where TParent : class, new()
        where TJoinTo : class, new()
    {
        
        internal DbQuery(DbQuery<TModel> context) : base(context)
        {
            const string joinFormat = "{3}{0}{4}.{3}Id{4} == {3}{1}{4}.{3}{2}Id{4}";

//            _joinTableName = typeof(TJoinTo).BuildTableName();
//            var joinExpression = string.Format(joinFormat, TableName, _joinTableName, TableName.Singularize(),DbProvider.Dialect.OpenBrace,DbProvider.Dialect.CloseBrace);
//            // todo: change this from Inner to resolve from config or attributes
//            _joinExpression = BuildJoinExpression(JoinType.Inner, joinExpression);
        }

        public IDbQuery<TModel, TParent, TJoin> Join<TJoin>(Expression<Func<TParent, object>> func) where TJoin : class, new()
        {
            
            return new DbQuery<TModel, TParent, TJoin>(this);
        }

        public IDbQuery<TModel, TParent, TJoin> Join<TJoin>(Expression<Func<TModel, TParent, object>> func) where TJoin : class, new()
        {
            return new DbQuery<TModel, TParent, TJoin>(this);
        }
//
//        public new IDbQuery<TModel, TParent, TJoinTo> Where(Expression<Func<TJoinTo, bool>> func)
//        {
//            return this;
//        }
//
//        public new IDbQuery<TModel, TParent, TJoinTo> OrderBy(Expression<Func<TJoinTo, object>> func)
//        {
//            return this;
//        }
//
//        public new IDbQuery<TModel, TParent, TJoinTo> OrderByDescending(Expression<Func<TJoinTo, object>> func)
//        {
//            return this;
//        }
//        
//        public override string ToString() => string.Format(DbProvider.Dialect.SelectFromJoin, TableName, _joinExpression, GetExtendedWhereClause()).Trim();
    }
}