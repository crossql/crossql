using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using crossql.Extensions;
using crossql.Helpers;

namespace crossql
{
    public class DbQuery<TModel, TJoinTo> : DbQuery<TModel>, IDbQuery<TModel, TJoinTo> 
        where TModel : class, new()
        where TJoinTo : class, new()
    {
        private readonly string _joinTableName;
        private readonly JoinType _joinType;
        private string _joinExpression;
        private JoinExpressionVisitor _joinExpressionVisitor;
        private OrderByExpressionVisitor _orderByExpressionVisitor;
        private WhereExpressionVisitor _whereExpressionVisitor;

        // Called if its a ManyToMany join, we can grab the join conditions from out attributes, no need for expression
        public DbQuery(IDbProvider dbProvider, JoinType joinType, IDbMapper<TModel> dbMapper) : base(dbProvider, dbMapper)
        {
            const string joinFormat = "{3}{0}{4}.{3}Id{4} == {3}{1}{4}.{3}{2}Id{4}";

            _joinType = joinType;
            _joinTableName = typeof(TJoinTo).BuildTableName();
            var joinExpression = string.Format(joinFormat, _TableName, _joinTableName, _TableName.Singularize(),_DbProvider.Dialect.OpenBrace,_DbProvider.Dialect.CloseBrace);
            _joinExpression = BuildJoinExpression(joinType, joinExpression);
        }

        public IDbQuery<TModel, TJoinTo> On(Expression<Func<TModel, TJoinTo, object>> joinExpression)
        {
            if (_joinType == JoinType.ManyToMany)
                throw new NotSupportedException("The join type you selected is not compatible with the On statement.");

            _joinExpressionVisitor = new JoinExpressionVisitor(_DbProvider.Dialect).Visit(joinExpression);
            _joinExpression = BuildJoinExpression(_joinType, _joinExpressionVisitor.JoinExpression);
            return this;
        }

        public IDbQuery<TModel, TJoinTo> OrderBy(Expression<Func<TModel, TJoinTo, object>> orderByExpression,
            OrderDirection direction)
        {
            _orderByExpressionVisitor = new OrderByExpressionVisitor(_DbProvider.Dialect).Visit(orderByExpression);

            _OrderByClause = string.Format(
                _DbProvider.Dialect.OrderBy,
                _orderByExpressionVisitor.OrderByExpression,
                direction == OrderDirection.Ascending ? "ASC" : "DESC");

            return this;
        }

        public new IDbQuery<TModel, TJoinTo> SkipTake(int skip, int take)
        {
            base.SkipTake(skip, take);
            return this;
        }

        public override string ToStringCount() => string.Format(_DbProvider.Dialect.SelectCountFromJoin, _TableName, _joinExpression, GetExtendedWhereClause()).Trim();

        public override string ToStringDelete() => string.Format(_DbProvider.Dialect.DeleteFromJoin, _TableName, _joinExpression, _WhereClause);

        public override Task Update(TModel model, Func<TModel, IDictionary<string, object>> mapToDbParameters)
        {
            var dbFields = _DbMapper.FieldNames
                .Where(field => field != "ID")
                .Select(field => string.Format("{1}{0}{2} = @{0}", field, _DbProvider.Dialect.OpenBrace,_DbProvider.Dialect.CloseBrace))
                .ToList();

            var whereClause = GetExtendedWhereClause();
            var commandText = string.Format(_DbProvider.Dialect.UpdateJoin, _TableName, string.Join(",", dbFields),
                _joinExpression, whereClause);
            var parameters = _Parameters.Union(mapToDbParameters(model))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return _DbProvider.ExecuteNonQuery(commandText, parameters);
        }

        public IDbQuery<TModel, TJoinTo> Where(Expression<Func<TModel, TJoinTo, bool>> expression)
        {
            _whereExpressionVisitor = new WhereExpressionVisitor(_Parameters).Visit(expression);
            _Parameters = _whereExpressionVisitor.Parameters;

            if (string.IsNullOrEmpty(_WhereClause))
                _WhereClause = string.Format(_DbProvider.Dialect.Where, _whereExpressionVisitor.WhereExpression);
            else
                _WhereClause += " AND " + _whereExpressionVisitor.WhereExpression;
            return this;
        }

        public override string ToString() => string.Format(_DbProvider.Dialect.SelectFromJoin, _TableName, _joinExpression, GetExtendedWhereClause()).Trim();

        private string BuildJoinExpression(JoinType joinType, string joinString)
        {
            if (joinType == JoinType.Inner)
                return string.Format(_DbProvider.Dialect.InnerJoin, _joinTableName, joinString);
            if (joinType == JoinType.Left)
                return string.Format(_DbProvider.Dialect.LeftJoin, _joinTableName, joinString);

            if (joinType == JoinType.ManyToMany)
            {
                var names = new[] {_TableName, _joinTableName};
                Array.Sort(names, StringComparer.CurrentCulture);
                var manyManyTableName = string.Join("_", names);

                var join = string.Format(_DbProvider.Dialect.ManyToManyJoin,
                    _TableName, "Id", manyManyTableName, _TableName.Singularize() + "Id", _joinTableName,
                    _joinTableName.Singularize() + "Id");
                return join;
            }

            throw new NotSupportedException("The join type you selected is not yet supported.");
        }
    }
}