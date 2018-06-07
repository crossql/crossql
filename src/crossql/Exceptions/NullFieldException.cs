using System;

namespace crossql.Exceptions
{
    public class NullFieldException : Exception
    {
        public NullFieldException()
        {
        }

        public NullFieldException(string message) : base(message)
        {
        }
    }
}