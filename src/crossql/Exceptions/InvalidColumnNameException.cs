using System;

namespace crossql.Exceptions
{
    public class InvalidColumnNameException : Exception
    {
        public InvalidColumnNameException()
        {
        }

        public InvalidColumnNameException(string message) : base(message)
        {
        }
    }
}