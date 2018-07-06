using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using crossql.Extensions;

namespace crossql
{
    public class DbScalar<TModel, TReturnType> : IDbScalar<TModel, TReturnType> where TModel : class, new()
    {
        private readonly IDbProvider _dbProvider;
        private readonly string _propertyName;
        private readonly string _tableName;
        private Dictionary<string, object> _parameters;
        private string _whereClause;
        private WhereExpressionVisitor _whereExpressionVisitor;

        public DbScalar(IDbProvider dbProvider, Expression<Func<TModel, TReturnType>> propertyExpression)
        {
            _propertyName = GetPropertyName(propertyExpression);
            _dbProvider = dbProvider;
            _tableName = typeof(TModel).BuildTableName();
            _parameters = new Dictionary<string, object>();
        }

        public IDbScalar<TModel, TReturnType> Where(Expression<Func<TModel, object>> expression)
        {
            _whereExpressionVisitor = new WhereExpressionVisitor(_dbProvider.Dialect).Visit(expression);
            _whereClause = string.Format(_dbProvider.Dialect.Where, _whereExpressionVisitor.WhereExpression);
            _parameters = _whereExpressionVisitor.Parameters;

            return this;
        }

        public Task<TReturnType> MaxAsync() => _dbProvider.ExecuteScalar<TReturnType>(ToStringMax(), _parameters);

        public Task<TReturnType> MinAsync() => _dbProvider.ExecuteScalar<TReturnType>(ToStringMin(), _parameters);

        public Task<TReturnType> SumAsync() => _dbProvider.ExecuteScalar<TReturnType>(ToStringSum(), _parameters);

        public string ToStringMax() => string.Format(_dbProvider.Dialect.SelectMaxFrom, _tableName, _whereClause, _propertyName).Trim();

        public string ToStringMin() => string.Format(_dbProvider.Dialect.SelectMinFrom, _tableName, _whereClause, _propertyName).Trim();

        public string ToStringSum() => string.Format(_dbProvider.Dialect.SelectSumFrom, _tableName, _whereClause, _propertyName).Trim();

        private static MemberExpression GetMemberInfo(Expression method)
        {
            if (!(method is LambdaExpression lambda))
                throw new ArgumentNullException(nameof(method));

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
                memberExpr = ((UnaryExpression) lambda.Body).Operand as MemberExpression;
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
                memberExpr = lambda.Body as MemberExpression;

            if (memberExpr == null) 
                throw new ArgumentException("method");

            return memberExpr;
        }

        private static string GetPropertyName(Expression<Func<TModel, TReturnType>> propertyExpression) => GetMemberInfo(propertyExpression).Member.Name;
    }
}