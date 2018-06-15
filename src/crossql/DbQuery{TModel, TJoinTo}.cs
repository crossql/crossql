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
            const string joinFormat = "[{0}].[Id] == [{1}].[{2}Id]";

            _joinType = joinType;
            _joinTableName = typeof(TJoinTo).BuildTableName();
            var joinExpression = string.Format(joinFormat, TableName, _joinTableName, TableName.Singularize());
            _joinExpression = BuildJoinExpression(joinType, joinExpression);
        }

        public IDbQuery<TModel, TJoinTo> On(Expression<Func<TModel, TJoinTo, object>> joinExpression)
        {
            if (_joinType == JoinType.ManyToMany)
                throw new NotSupportedException("The join type you selected is not compatible with the On statement.");

            _joinExpressionVisitor = new JoinExpressionVisitor().Visit(joinExpression);
            _joinExpression = BuildJoinExpression(_joinType, _joinExpressionVisitor.JoinExpression);
            return this;
        }

        public IDbQuery<TModel, TJoinTo> OrderBy(Expression<Func<TModel, TJoinTo, object>> orderByExpression,
            OrderDirection direction)
        {
            _orderByExpressionVisitor = new OrderByExpressionVisitor().Visit(orderByExpression);

            OrderByClause = string.Format(
                DbProvider.Dialect.OrderBy,
                _orderByExpressionVisitor.OrderByExpression,
                direction == OrderDirection.Ascending ? "ASC" : "DESC");

            return this;
        }

        public new IDbQuery<TModel, TJoinTo> SkipTake(int skip, int take)
        {
            base.SkipTake(skip, take);
            return this;
        }

        public override string ToStringCount() => string.Format(DbProvider.Dialect.SelectCountFromJoin, TableName, _joinExpression, GetExtendedWhereClause()).Trim();

        public override string ToStringDelete() => string.Format(DbProvider.Dialect.DeleteFromJoin, TableName, _joinExpression, WhereClause);

        public override Task Update(TModel model, Func<TModel, IDictionary<string, object>> mapToDbParameters)
        {
            var dbFields = DbMapper.FieldNames
                .Where(field => field != "ID")
                .Select(field => string.Format("[{0}] = @{0}", field))
                .ToList();

            var whereClause = GetExtendedWhereClause();
            var commandText = string.Format(DbProvider.Dialect.UpdateJoin, TableName, string.Join(",", dbFields),
                _joinExpression, whereClause);
            var parameters = Parameters.Union(mapToDbParameters(model))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return DbProvider.ExecuteNonQuery(commandText, parameters);
        }

        public IDbQuery<TModel, TJoinTo> Where(Expression<Func<TModel, TJoinTo, bool>> expression)
        {
            _whereExpressionVisitor = new WhereExpressionVisitor(Parameters).Visit(expression);
            Parameters = _whereExpressionVisitor.Parameters;

            if (string.IsNullOrEmpty(WhereClause))
                WhereClause = string.Format(DbProvider.Dialect.Where, _whereExpressionVisitor.WhereExpression);
            else
                WhereClause += " AND " + _whereExpressionVisitor.WhereExpression;
            return this;
        }

        public override string ToString() => string.Format(DbProvider.Dialect.SelectFromJoin, TableName, _joinExpression, GetExtendedWhereClause()).Trim();

        private string BuildJoinExpression(JoinType joinType, string joinString)
        {
            if (joinType == JoinType.Inner)
                return string.Format(DbProvider.Dialect.InnerJoin, _joinTableName, joinString);
            if (joinType == JoinType.Left)
                return string.Format(DbProvider.Dialect.LeftJoin, _joinTableName, joinString);

            if (joinType == JoinType.ManyToMany)
            {
                var names = new[] {TableName, _joinTableName};
                Array.Sort(names, StringComparer.CurrentCulture);
                var manyManyTableName = string.Join("_", names);

                var join = string.Format(DbProvider.Dialect.ManyToManyJoin,
                    TableName, "Id", manyManyTableName, TableName.Singularize() + "Id", _joinTableName,
                    _joinTableName.Singularize() + "Id");
                return join;
            }

            throw new NotSupportedException("The join type you selected is not yet supported.");
        }
    }
}