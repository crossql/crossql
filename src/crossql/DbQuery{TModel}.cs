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
        private Dictionary<string, object> _whereParameters = new Dictionary<string, object>();
        private readonly IDbProvider _dbProvider;
        private readonly IDbMapper<TModel> _dbMapper;
        private Expression<Func<TModel, object>> _orderByExpression;
        private readonly string _tableName;
        private readonly DbQuery<TModel> _context;
        private bool _orderBySet;
        private string _orderBySortOrder;
        private readonly IList<Expression<Func<TModel, bool>>> _whereExpressions = new List<Expression<Func<TModel, bool>>>();
        private int _skip;
        private int _take;
        private bool _hasSkipTake;

        private Dictionary<string, object> WhereParameters => _context?._whereParameters ?? _whereParameters;
        private Expression<Func<TModel, object>> OrderByExpression => _context?._orderByExpression ?? _orderByExpression;
        private IList<Expression<Func<TModel, bool>>> WhereExpressions => _context?._whereExpressions ?? _whereExpressions;
        private IDbProvider DbProvider => _context?._dbProvider ?? _dbProvider;
        
        public DbQuery(IDbProvider dbProvider, IDbMapper<TModel> dbMapper)
        {
            _dbMapper = dbMapper;
            _dbProvider = dbProvider;
           _tableName = typeof(TModel).BuildTableName();
        }

        /// <summary>
        /// Recursive constructor
        /// </summary>
        internal DbQuery(DbQuery<TModel> context)
        {
            _context = context;
        }
        
        public IDbQuery<TModel, TJoin> Join<TJoin>(Expression<Func<TModel, object>> expression) where TJoin : class, new() 
            => new DbQuery<TModel, TJoin>(this);

        public Task<int> Count() 
            => DbProvider.ExecuteScalar<int>(ToStringCount(), _whereParameters);

        public Task Delete() 
            => DbProvider.ExecuteNonQuery(ToStringDelete(), _whereParameters);

        public IDbQuery<TModel> OrderBy(Expression<Func<TModel, object>> expression)
        {
            SetOrderExpression("ASC", expression);
            return this;
        }

        public IDbQuery<TModel> OrderByDescending(Expression<Func<TModel, object>> expression)
        {
            SetOrderExpression("DESC", expression);
            return this;
        }

        public IDbQuery<TModel> SkipTake(int skip, int take)
        {
            _skip = skip;
            _take = take;
            _hasSkipTake = true;
            return this;
        }

        public IDbQuery<TModel> Where(Expression<Func<TModel, bool>> expression)
        {
            _whereExpressions.Add(expression);
            return this;
        }

        public Task<IEnumerable<TResult>> Select<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc) 
            => DbProvider.ExecuteReader(ToString(), _whereParameters, mapperFunc);

        public Task<IEnumerable<TModel>> Select() => Select(_dbMapper.BuildListFrom);

        public Task Update(TModel model) => Update(model, _dbMapper.BuildDbParametersFrom);

        public virtual Task Update(TModel model, Func<TModel, IDictionary<string, object>> mapToDbParameters)
        {
            var dbFields = _dbMapper.FieldNames
                .Where(field => field != "ID")
                .Select(field => string.Format("{1}{0}{2} = @{0}", field, DbProvider.Dialect.OpenBrace,DbProvider.Dialect.CloseBrace)).ToList();

            var whereClause = GenerateWhereClause();
            var commandText = string.Format(DbProvider.Dialect.Update, _tableName, string.Join(",", dbFields),
                whereClause);
            var parameters = _whereParameters.Union(mapToDbParameters(model))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return DbProvider.ExecuteNonQuery(commandText, parameters);
        }

        public void Truncate() => DbProvider.ExecuteNonQuery(ToStringTruncate());

        public override string ToString()
        {
            // skip take
            var skipTakeClause = GenerateSkipTake();

            // order by
            var orderByClause = GenerateOrderByClause();
            
            // where
            var whereClause = GenerateWhereClause();
            var whereOrderBySkipTakeClause = WhereOrderBySkipTake(whereClause, orderByClause, skipTakeClause);
            
              // todo: this is where we iterate over the DbQuery and generate the goods
              // var expressionVisitor = new JoinExpressionVisitor(DbProvider.Dialect).Visit(expression);
              // _joinExpression = BuildJoinExpression(JoinType.Inner, expressionVisitor.JoinExpression);

            return string.Format(DbProvider.Dialect.SelectFrom, _tableName, whereOrderBySkipTakeClause).Trim();
        }

        public virtual string ToStringCount() => string.Format(DbProvider.Dialect.SelectCountFrom, _tableName, GenerateWhereClause()).Trim();

        public virtual string ToStringDelete() => string.Format(DbProvider.Dialect.DeleteFrom, _tableName, GenerateWhereClause());

        public string ToStringTruncate() => string.Format(DbProvider.Dialect.Truncate, _tableName);

        private static string WhereOrderBySkipTake(string whereClause, string orderByClause, string skipTakeClause) 
            => string.Join(" ", whereClause, orderByClause, skipTakeClause);

        private string GenerateOrderByClause()
        {
            if (OrderByExpression == null) return string.Empty;
            
            var orderByExpressionVisitor = new OrderByExpressionVisitor(DbProvider.Dialect).Visit(_orderByExpression);

            return string.Format(
                DbProvider.Dialect.OrderBy,
                orderByExpressionVisitor.OrderByExpression, _orderBySortOrder);

        }

        private string GenerateWhereClause()
        {
            if (!WhereExpressions.Any()) return string.Empty;
            var whereClause = string.Empty;
            
            for (var index = 0; index < WhereExpressions.Count; index++)
            {
                var expression = WhereExpressions[index];
                var whereExpressionVisitor =
                    new WhereExpressionVisitor(WhereParameters, DbProvider.Dialect).Visit(expression);
                _whereParameters = whereExpressionVisitor.Parameters;

                if (string.IsNullOrEmpty(whereClause))
                    whereClause = string.Format(DbProvider.Dialect.Where, whereExpressionVisitor.WhereExpression);
                else
                    whereClause += " AND " + whereExpressionVisitor.WhereExpression;
            }

            return whereClause;
        }

        private string GenerateSkipTake() => _hasSkipTake 
            ? string.Format(DbProvider.Dialect.SkipTake, _skip, _take) 
            : string.Empty;
        
        private void SetOrderExpression(string sortOrder, Expression<Func<TModel,object>> expression)
        {
            if(_orderBySet) throw new ArgumentException("You cannot set OrderBy multiple times");
            _orderByExpression = expression;
            _orderBySet = true;
            _orderBySortOrder = sortOrder;        
        }

//        protected string BuildJoinExpression(JoinType joinType, string joinString)
//        {
//            if (joinType == JoinType.Inner)
//                return string.Format(DbProvider.Dialect.InnerJoin, _joinTableName, joinString);
//            if (joinType == JoinType.Left)
//                return string.Format(DbProvider.Dialect.LeftJoin, _joinTableName, joinString);
//
//            if (joinType == JoinType.ManyToMany)
//            {
//                var names = new[] {TableName, _joinTableName};
//                Array.Sort(names, StringComparer.CurrentCulture);
//                var manyManyTableName = string.Join("_", names);
//
//                var join = string.Format(DbProvider.Dialect.ManyToManyJoin,
//                    TableName, "Id", manyManyTableName, TableName.Singularize() + "Id", _joinTableName,
//                    _joinTableName.Singularize() + "Id");
//                return join;
//            }
//
//            throw new NotSupportedException("The join type you selected is not yet supported.");
//        }
    }
}