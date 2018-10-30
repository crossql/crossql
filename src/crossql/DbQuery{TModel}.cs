using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using crossql.Extensions;
using crossql.Helpers;

namespace crossql
{
    public class DbQuery<TModel> : IDbQuery<TModel>
        where TModel : class, new()
    {
        // keep this around for posterity. it's the old join format
        // const string _joinFormat = "{3}{0}{4}.{3}Id{4} == {3}{1}{4}.{3}{2}Id{4}";
        
        // read only backing fields
        private readonly Expression _joinExpression;
        private readonly string _joinTableName;
        private readonly IDbProvider _dbProvider;
        private readonly IDbMapper<TModel> _dbMapper;
        private readonly string _tableName;
        private readonly DbQuery<TModel> _context;
        private readonly IList<Expression<Func<TModel, bool>>> _whereExpressions = new List<Expression<Func<TModel, bool>>>();

        // backing fields
        private Dictionary<string, object> _whereParameters;
        private Expression<Func<TModel, object>> _orderByExpression;
        private bool _orderBySet;
        private string _orderBySortOrder;
        private int _skip;
        private int _take;
        private bool _hasSkipTake;

        // Properties
        private Dictionary<string, object> WhereParameters => _whereParameters ?? _context?.WhereParameters;
        private Expression<Func<TModel, object>> OrderByExpression => _orderByExpression ?? _context?.OrderByExpression;
        private IList<Expression<Func<TModel, bool>>> WhereExpressions => _whereExpressions ?? _context?.WhereExpressions ;
        private IDbProvider DbProvider => _dbProvider ?? _context?.DbProvider;
        private string TableName => _tableName ?? _context?.TableName;
        
        /// <summary>
        /// Creates a new query by which to run off of the <see cref="IDbProvider"/>. 
        /// </summary>
        /// <param name="dbProvider">platform specific db provider</param>
        /// <param name="dbMapper">platform specific db mapper</param>
        /// <remarks>
        /// Queries are lazily evaluated once you call one of the execution methods.
        /// <see cref="Count"/>,
        /// <see cref="Select"/>,
        /// <see cref="Select{TResult}"/>,
        /// <see cref="Update(TModel)"/>,
        /// <see cref="Delete"/>,
        /// <see cref="Truncate"/>,
        /// or any one of the <see cref="ToString"/> methods.</remarks>
        public DbQuery(IDbProvider dbProvider, IDbMapper<TModel> dbMapper)
        {
            _dbMapper = dbMapper;
            _dbProvider = dbProvider;
            _whereParameters = new Dictionary<string, object>();
           _tableName = typeof(TModel).BuildTableName();
        }

        /// <summary>
        /// Recursive constructor used when creating new Joins.
        /// </summary>
        internal DbQuery(DbQuery<TModel> context, string joinTableName, Expression joinExpression)
        {
            _context = context;
            _joinTableName = joinTableName;
            _joinExpression = joinExpression;
        }

        // Public Methods
        public Task<int> Count() => DbProvider.ExecuteScalar<int>(ToStringCount(), _whereParameters);

        public Task<IEnumerable<TModel>> Select() => Select(_dbMapper.BuildListFrom);
        
        public Task<IEnumerable<TResult>> Select<TResult>(Func<IDataReader, IEnumerable<TResult>> mapperFunc) 
            => DbProvider.ExecuteReader(ToString(), _whereParameters, mapperFunc);

        public Task Update(TModel model) => Update(model, _dbMapper.BuildDbParametersFrom);

        public virtual Task Update(TModel model, Func<TModel, IDictionary<string, object>> mapToDbParameters)
        {
            // todo: move away from ID opinion, extract value from config first and fall back to ID if unavailable.
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
        
        public Task Delete() => DbProvider.ExecuteNonQuery(ToStringDelete(), _whereParameters);

        public Task Truncate() => DbProvider.ExecuteNonQuery(ToStringTruncate());
        
        public IDbQuery<TModel, TJoin> Join<TJoin>(Expression<Func<TModel, object>> expression) where TJoin : class, new()
        {
            var joinTableName = typeof(TJoin).BuildTableName();
            return new DbQuery<TModel, TJoin>(this, joinTableName, expression);
        }

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
            WhereExpressions.Add(expression);
            return this;
        }

        // ToString methods do the evaluation and return the final SQL string.
        public override string ToString()
        {
            // todo: check to see if the query is already cached. If so, short circuit this call and reuse it,
            // note: still have to extract the where parameters
            
            // join
            var joinVisitor = new JoinExpressionVisitor(DbProvider.Dialect);
            var joinClause = GenerateJoinClauseRecursive(this, joinVisitor);
            
            // where
            var whereVisitor = new WhereExpressionVisitor(WhereParameters, DbProvider.Dialect);
            // todo: if the query is already in the cache, we just have to visit for parameter values (WhereParameters)
            var whereClause = GenerateWhereClause(whereVisitor);
            
            // skip take
            var skipTakeClause = GenerateSkipTake();

            // order by
            var orderByClause = GenerateOrderByClause();
            
            var clause = GenerateFinalClause(joinClause, whereClause, orderByClause, skipTakeClause);
            return string.Format(DbProvider.Dialect.SelectFrom, TableName, clause).Trim();
        }

        public virtual string ToStringCount() => string.Format(DbProvider.Dialect.SelectCountFrom, _tableName, GenerateWhereClause()).Trim();

        public virtual string ToStringDelete() => string.Format(DbProvider.Dialect.DeleteFrom, _tableName, GenerateWhereClause());

        public string ToStringTruncate() => string.Format(DbProvider.Dialect.Truncate, _tableName);

        // Private Methods
        private static string GenerateFinalClause(string joinClause, string whereClause, string orderByClause, string skipTakeClause) 
            =>  string.Join("\n", joinClause, whereClause, orderByClause, skipTakeClause);

        private static string GenerateJoinClauseRecursive(DbQuery<TModel> context, JoinExpressionVisitor visitor, string joinClause = "")
        {
            if (!(context._context is null))
            {
                joinClause += GenerateJoinClauseRecursive(context._context, visitor, joinClause);
            }
            
            if (!(context._joinExpression is null))
            {
                visitor.Visit(context._joinExpression);
                joinClause += $"\n{BuildJoinExpression(JoinType.Inner, visitor.JoinExpression, context)}";
            }

            return joinClause;
        }
        
        private string GenerateOrderByClause()
        {
            if (OrderByExpression == null) return string.Empty;
            
            var orderByExpressionVisitor = new OrderByExpressionVisitor(DbProvider.Dialect).Visit(_orderByExpression);

            return string.Format(
                DbProvider.Dialect.OrderBy,
                orderByExpressionVisitor.OrderByExpression, _orderBySortOrder);

        }

        private string GenerateWhereClause() =>
            GenerateWhereClause(new WhereExpressionVisitor(WhereParameters, DbProvider.Dialect));
        
        private string GenerateWhereClause(WhereExpressionVisitor whereVisitor)
        {
            if (!WhereExpressions.Any()) return string.Empty;
            var whereClause = string.Empty;

            for (var index = 0; index < WhereExpressions.Count; index++)
            {
                var expression = WhereExpressions[index];
                whereVisitor.Visit(expression);
                _whereParameters = whereVisitor.Parameters;

                if (string.IsNullOrEmpty(whereClause))
                    whereClause = string.Format(DbProvider.Dialect.Where, whereVisitor.WhereExpression);
                else
                    whereClause += $" AND {whereVisitor.WhereExpression}";
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

        private static string BuildJoinExpression(JoinType joinType, string joinString, DbQuery<TModel> context)
        {
            var joinTableName = context._joinTableName;
            var dbProvider = context.DbProvider;
            
            if (joinType == JoinType.Inner)
                return string.Format(dbProvider.Dialect.InnerJoin, joinTableName, joinString);
            if (joinType == JoinType.Left)
                return string.Format(dbProvider.Dialect.LeftJoin, joinTableName, joinString);

            if (joinType == JoinType.ManyToMany)
            {
                // OPINION: many to many tables are sorted alphabetically, joined by an underscore
                var names = new[] {context.TableName, joinTableName};
                Array.Sort(names, StringComparer.CurrentCulture);
                var manyManyTableName = string.Join("_", names);

                var join = string.Format(dbProvider.Dialect.ManyToManyJoin,
                    context.TableName, "Id", manyManyTableName, context.TableName.Singularize() + "Id", joinTableName,
                    joinTableName.Singularize() + "Id");
                return join;
            }

            throw new NotSupportedException("The join type you selected is not yet supported.");
        }
    }
}