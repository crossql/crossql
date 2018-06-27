using System;
using System.Linq.Expressions;

namespace crossql.Exceptions
{
    public class ExpressionNotSupportedException : Exception
    {
        private const string _ExpressionNotSupported = "Unhandled expression type: '{0}'";

        public ExpressionNotSupportedException(Expression expression)
            : base(string.Format(_ExpressionNotSupported, expression.NodeType))
        {
        }
    }
}