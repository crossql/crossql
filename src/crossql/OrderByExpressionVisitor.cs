using System;
using System.Linq.Expressions;
using System.Text;
using crossql.Exceptions;
using crossql.Extensions;

namespace crossql
{
    public class OrderByExpressionVisitor
    {
        private readonly IDialect _dialect;
        private readonly StringBuilder _strings;

        public OrderByExpressionVisitor(IDialect dialect)
        {
            _dialect = dialect;
            _strings = new StringBuilder();
        }

        public string OrderByExpression => _strings.ToString().Trim();

        public OrderByExpressionVisitor Visit(Expression orderByExpression)
        {
            VisitExpression(orderByExpression);
            return this;
        }

        private void VisitExpression(Expression expression)
        {
            if (expression == null)
                throw new NullReferenceException();

            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    VisitLambda((LambdaExpression) expression);
                    break;
                case ExpressionType.MemberAccess:
                    VisitMemberAccess((MemberExpression) expression);
                    break;
                case ExpressionType.Convert:
                    if (expression is UnaryExpression exp)
                    {
                        VisitMemberAccess( (MemberExpression)exp.Operand );
                    }
                    break;
                default:
                    throw new ExpressionNotSupportedException(expression);
            }
        }

        private void VisitLambda(LambdaExpression expression)
        {
            var lambda = expression.Body;
            Visit(lambda);
        }

        private void VisitMemberAccess(MemberExpression expression)
        {
            var tableName = expression.Expression.Type.BuildTableName();
            _strings.AppendFormat( "{2}{0}{3}.{2}{1}{3}", tableName, expression.Member.Name,_dialect.OpenBrace,_dialect.CloseBrace );
        }
    }
}