using System;
using System.Linq.Expressions;

namespace crossql.Exceptions
{
    internal class ExpressionBinaryOperatorNotSupportedException : Exception
    {
        private const string _OperatorNotSupported = "The binary operator '{0}' is not supported.";

        public ExpressionBinaryOperatorNotSupportedException(BinaryExpression expression)
            : base(string.Format(_OperatorNotSupported, expression.NodeType))
        {
        }
    }
}