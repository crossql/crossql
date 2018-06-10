using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using crossql.Extensions;

namespace crossql
{
    public class DbQuery<TModel> : IDbQuery<TModel> 
        where TModel : class, new()
    {
        protected readonly IDbMapper<TModel> DbMapper;
        protected readonly IDbProvider DbProvider;
        protected string OrderByClause;
        protected Dictionary<string, object> Parameters;
        protected string SkipTakeClause;
        protected string TableName;
        protected string WhereClause;
        private OrderByExpressionVisitor _orderByExpressionVisitor;
        private WhereExpressionVisitor _whereExpressionVisitor;

        public DbQuery(IDbProvider dbProvider, IDbMapper<TModel> dbMapper)
        {
            DbMapper = dbMapper;
            TableName = typeof(TModel).BuildTableName();
            DbProvider = dbProvider;
            Parameters = new Dictionary<string, object>();
        }

        public Task<int> Count() => DbProvider.ExecuteScalar<int>(ToStringCount(), Parameters);

        public Task Delete() => DbProvider.ExecuteNonQuery(ToStringDelete(), Parameters);

        public IDbQuery<TModel, TJoinTo> Join<TJoinTo>() where TJoinTo : class, new() => new DbQuery<TModel, TJoinTo>(DbProvider, JoinType.Left, DbMapper);

        public IDbQuery<TModel, TJoinTo> ManyToManyJoin<TJoinTo>() where TJoinTo : class, new() => new DbQuery<TModel, TJoinTo>(DbProvider, JoinType.ManyToMany, DbMapper);

        public IDbQuery<TModel> OrderBy(Expression<Func<TModel, object>> expression, OrderDirection direction)
        {
            _orderByExpressionVisitor = new OrderByExpressionVisitor().Visit(expression);

            OrderByClause = string.Format(
                DbProvider.Dialect.OrderBy,
                _orderByExpressionVisitor.OrderByExpression,
                direction == OrderDirection.Ascending ? "ASC" : "DESC");

            return this;
        }

        public Task<IEnumerable<TResult>> Select<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc) => DbProvider.ExecuteReader(ToString(), Parameters, mapperFunc);

        public Task<IEnumerable<TModel>> Select() => Select(DbMapper.BuildListFrom);

        public IDbQuery<TModel> SkipTake(int skip, int take)
        {
            SkipTakeClause = string.Format(DbProvider.Dialect.SkipTake, skip, take);
            return this;
        }

        public virtual string ToStringCount() => string.Format(DbProvider.Dialect.SelectCountFrom, TableName, GetExtendedWhereClause()).Trim();

        public virtual string ToStringDelete() => string.Format(DbProvider.Dialect.DeleteFrom, TableName, WhereClause);

        public string ToStringTruncate() => string.Format(DbProvider.Dialect.Truncate, TableName);

        public void Truncate() => DbProvider.ExecuteNonQuery(ToStringTruncate());

        public Task Update(TModel model) => Update(model, DbMapper.BuildDbParametersFrom);

        public virtual Task Update(TModel model, Func<TModel, IDictionary<string, object>> mapToDbParameters)
        {
            var dbFields = DbMapper.FieldNames
                .Where(field => field != "ID")
                .Select(field => string.Format("[{0}] = @{0}", field)).ToList();

            var whereClause = GetExtendedWhereClause();
            var commandText = string.Format(DbProvider.Dialect.Update, TableName, string.Join(",", dbFields),
                whereClause);
            var parameters = Parameters.Union(mapToDbParameters(model))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return DbProvider.ExecuteNonQuery(commandText, parameters);
        }

        public IDbQuery<TModel> Where(Expression<Func<TModel, bool>> expression)
        {
            _whereExpressionVisitor = new WhereExpressionVisitor(Parameters).Visit(expression);
            Parameters = _whereExpressionVisitor.Parameters;

            if (string.IsNullOrEmpty(WhereClause))
                WhereClause = string.Format(DbProvider.Dialect.Where, _whereExpressionVisitor.WhereExpression);
            else
                WhereClause += " AND " + _whereExpressionVisitor.WhereExpression;
            return this;
        }

        public override string ToString() => string.Format(DbProvider.Dialect.SelectFrom, TableName, GetExtendedWhereClause()).Trim();

        protected string GetExtendedWhereClause() => string.Join(" ", WhereClause, OrderByClause, SkipTakeClause);
    }
}