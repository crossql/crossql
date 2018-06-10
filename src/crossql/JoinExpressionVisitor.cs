using System;
using System.Linq.Expressions;
using System.Text;
using crossql.Exceptions;
using crossql.Extensions;

namespace crossql
{
    public class JoinExpressionVisitor
    {
        private readonly StringBuilder _strings;

        public JoinExpressionVisitor()
        {
            _strings = new StringBuilder();
        }

        public string JoinExpression => _strings.ToString().Trim();

        public JoinExpressionVisitor Visit(Expression expression)
        {
            VisitExpression(expression);
            return this;
        }

        protected virtual Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType != ExpressionType.Equal) throw new ExpressionNotSupportedException(binaryExpression);

            VisitExpression(binaryExpression.Left);
            _strings.Append(" = ");
            VisitExpression(binaryExpression.Right);

            return binaryExpression;
        }

        private string BuildColumnName(Expression expression) =>
            expression is MemberExpression memberExpression ? BuildColumnName(memberExpression.Expression) + memberExpression.Member.Name : "";

        private string BuildTableName(Expression expression) =>
            expression is MemberExpression memberExpression ? BuildTableName(memberExpression.Expression) : expression.Type.BuildTableName();

        private void VisitExpression(Expression expression)
        {
            if (expression == null)
                throw new NullReferenceException();

            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    VisitLambda((LambdaExpression) expression);
                    return;
                case ExpressionType.MemberAccess:
                    VisitMemberAccess((MemberExpression) expression);
                    return;
                case ExpressionType.Convert:
                    VisitUnary((UnaryExpression) expression);
                    return;
                case ExpressionType.Equal:
                    VisitBinary((BinaryExpression) expression);
                    return;
                default:
                    throw new ExpressionNotSupportedException(expression);
            }
        }

        private void VisitLambda(LambdaExpression expression) => Visit(expression.Body);

        private void VisitMemberAccess(MemberExpression expression)
        {
            var tableName = BuildTableName(expression);
            var columnName = BuildColumnName(expression);
            _strings.AppendFormat("[{0}].[{1}]", tableName, columnName);
        }

        private void VisitUnary(UnaryExpression expression) => Visit(expression.Operand);
    }
}