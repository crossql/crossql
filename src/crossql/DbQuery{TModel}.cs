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
        protected readonly IDbMapper<TModel> _DbMapper;
        protected readonly IDbProvider _DbProvider;
        protected string _OrderByClause;
        protected Dictionary<string, object> _Parameters;
        protected string _SkipTakeClause;
        protected readonly string _TableName;
        protected string _WhereClause;
        private OrderByExpressionVisitor _orderByExpressionVisitor;
        private WhereExpressionVisitor _whereExpressionVisitor;

        public DbQuery(IDbProvider dbProvider, IDbMapper<TModel> dbMapper)
        {
            _DbMapper = dbMapper;
            _TableName = typeof(TModel).BuildTableName();
            _DbProvider = dbProvider;
            _Parameters = new Dictionary<string, object>();
        }

        public Task<int> Count() => _DbProvider.ExecuteScalar<int>(ToStringCount(), _Parameters);

        public Task Delete() => _DbProvider.ExecuteNonQuery(ToStringDelete(), _Parameters);

        public IDbQuery<TModel, TJoinTo> Join<TJoinTo>() where TJoinTo : class, new() => new DbQuery<TModel, TJoinTo>(_DbProvider, JoinType.Left, _DbMapper);

        public IDbQuery<TModel, TJoinTo> ManyToManyJoin<TJoinTo>() where TJoinTo : class, new() => new DbQuery<TModel, TJoinTo>(_DbProvider, JoinType.ManyToMany, _DbMapper);

        public IDbQuery<TModel> OrderBy(Expression<Func<TModel, object>> expression, OrderDirection direction)
        {
            _orderByExpressionVisitor = new OrderByExpressionVisitor(_DbProvider.Dialect).Visit(expression);

            _OrderByClause = string.Format(
                _DbProvider.Dialect.OrderBy,
                _orderByExpressionVisitor.OrderByExpression,
                direction == OrderDirection.Ascending ? "ASC" : "DESC");

            return this;
        }

        public Task<IEnumerable<TResult>> Select<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc) => _DbProvider.ExecuteReader(ToString(), _Parameters, mapperFunc);

        public Task<IEnumerable<TModel>> Select() => Select(_DbMapper.BuildListFrom);

        public IDbQuery<TModel> SkipTake(int skip, int take)
        {
            _SkipTakeClause = string.Format(_DbProvider.Dialect.SkipTake, skip, take);
            return this;
        }

        public virtual string ToStringCount() => string.Format(_DbProvider.Dialect.SelectCountFrom, _TableName, GetExtendedWhereClause()).Trim();

        public virtual string ToStringDelete() => string.Format(_DbProvider.Dialect.DeleteFrom, _TableName, _WhereClause);

        public string ToStringTruncate() => string.Format(_DbProvider.Dialect.Truncate, _TableName);

        public void Truncate() => _DbProvider.ExecuteNonQuery(ToStringTruncate());

        public Task Update(TModel model) => Update(model, _DbMapper.BuildDbParametersFrom);

        public virtual Task Update(TModel model, Func<TModel, IDictionary<string, object>> mapToDbParameters)
        {
            var dbFields = _DbMapper.FieldNames
                .Where(field => field != "ID")
                .Select(field => string.Format("{1}{0}{2} = @{0}", field, _DbProvider.Dialect.OpenBrace,_DbProvider.Dialect.CloseBrace)).ToList();

            var whereClause = GetExtendedWhereClause();
            var commandText = string.Format(_DbProvider.Dialect.Update, _TableName, string.Join(",", dbFields),
                whereClause);
            var parameters = _Parameters.Union(mapToDbParameters(model))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return _DbProvider.ExecuteNonQuery(commandText, parameters);
        }

        public IDbQuery<TModel> Where(Expression<Func<TModel, bool>> expression)
        {
            _whereExpressionVisitor = new WhereExpressionVisitor(_Parameters, _DbProvider.Dialect).Visit(expression);
            _Parameters = _whereExpressionVisitor.Parameters;

            if (string.IsNullOrEmpty(_WhereClause))
                _WhereClause = string.Format(_DbProvider.Dialect.Where, _whereExpressionVisitor.WhereExpression);
            else
                _WhereClause += " AND " + _whereExpressionVisitor.WhereExpression;
            return this;
        }

        public override string ToString() => string.Format(_DbProvider.Dialect.SelectFrom, _TableName, GetExtendedWhereClause()).Trim();

        protected string GetExtendedWhereClause() => string.Join(" ", _WhereClause, _OrderByClause, _SkipTakeClause);
    }
}