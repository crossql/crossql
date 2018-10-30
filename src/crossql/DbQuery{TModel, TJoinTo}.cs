using System;
using System.Linq.Expressions;

namespace crossql
{
    public class DbQuery<TModel, TPreviousJoin> : DbQuery<TModel>, IDbQuery<TModel, TPreviousJoin> 
        where TModel : class, new()
        where TPreviousJoin : class, new()
    {

        const string _joinFormat = "{3}{0}{4}.{3}Id{4} == {3}{1}{4}.{3}{2}Id{4}";

        internal DbQuery(DbQuery<TModel> context) : base(context)
        {

//            _joinTableName = typeof(TPreviousJoin).BuildTableName();
//            var joinExpression = string.Format(_joinFormat, TableName, _joinTableName, TableName.Singularize(),DbProvider.Dialect.OpenBrace,DbProvider.Dialect.CloseBrace);
//            // todo: change this from Inner to resolve from config or attributes
//            _joinExpression = BuildJoinExpression(JoinType.Inner, joinExpression);
        }

        public IDbQuery<TModel, TPreviousJoin, TJoin> Join<TJoin>(Expression<Func<TModel, TPreviousJoin, object>> expression) where TJoin : class, new() 
            => new DbQuery<TModel,TPreviousJoin,TJoin>(this);
//
//        public IDbQuery<TModel, TPreviousJoin> OrderBy(Expression<Func<TModel, TPreviousJoin, object>> orderByExpression)
//        {
//            return this;
//        }
//
//        public IDbQuery<TModel, TPreviousJoin> OrderByDescending(Expression<Func<TPreviousJoin, object>> func)
//        {
//            return this;
//        }
//
//        public IDbQuery<TModel, TPreviousJoin> OrderBy(Expression<Func<TModel, TPreviousJoin, object>> orderByExpression,
//            OrderDirection direction)
//        {
//            _orderByExpressionVisitor = new OrderByExpressionVisitor(DbProvider.Dialect).Visit(orderByExpression);
//
//            OrderByClause = string.Format(
//                DbProvider.Dialect.OrderBy,
//                _orderByExpressionVisitor.OrderByExpression,
//                direction == OrderDirection.Ascending ? "ASC" : "DESC");
//
//            return this;
//        }
//
//        public new IDbQuery<TModel, TPreviousJoin> SkipTake(int skip, int take)
//        {
//            base.SkipTake(skip, take);
//            return this;
//        }
//
//        public override string ToStringCount() => string.Format(DbProvider.Dialect.SelectCountFromJoin, TableName, _joinExpression, GetExtendedWhereClause()).Trim();
//
//        public override string ToStringDelete() => string.Format(DbProvider.Dialect.DeleteFromJoin, TableName, _joinExpression, WhereClause);
//
//        public override Task Update(TModel model, Func<TModel, IDictionary<string, object>> mapToDbParameters)
//        {
//            var dbFields = DbMapper.FieldNames
//                .Where(field => field != "ID")
//                .Select(field => string.Format("{1}{0}{2} = @{0}", field, DbProvider.Dialect.OpenBrace,DbProvider.Dialect.CloseBrace))
//                .ToList();
//
//            var whereClause = GetExtendedWhereClause();
//            var commandText = string.Format(DbProvider.Dialect.UpdateJoin, TableName, string.Join(",", dbFields),
//                _joinExpression, whereClause);
//            var parameters = Parameters.Union(mapToDbParameters(model))
//                .ToDictionary(pair => pair.Key, pair => pair.Value);
//
//            return DbProvider.ExecuteNonQuery(commandText, parameters);
//        }
//
//        public IDbQuery<TModel, TPreviousJoin> Where(Expression<Func<TModel, TPreviousJoin, bool>> expression)
//        {
//            _whereExpressionVisitor = new WhereExpressionVisitor(Parameters, DbProvider.Dialect).Visit(expression);
//            Parameters = _whereExpressionVisitor.Parameters;
//
//            if (string.IsNullOrEmpty(WhereClause))
//                WhereClause = string.Format(DbProvider.Dialect.Where, _whereExpressionVisitor.WhereExpression);
//            else
//                WhereClause += " AND " + _whereExpressionVisitor.WhereExpression;
//            return this;
//        }
//
//        public IDbQuery<TModel, TPreviousJoin> Where(Expression<Func<TPreviousJoin, bool>> func)
//        {
//            return this;        
//        }
//
//        public IDbQuery<TModel, TPreviousJoin> OrderBy(Expression<Func<TPreviousJoin, object>> func)
//        {
//            return this;
//        }
//
//        public override string ToString() => string.Format(DbProvider.Dialect.SelectFromJoin, TableName, _joinExpression, GetExtendedWhereClause()).Trim();
    }
}