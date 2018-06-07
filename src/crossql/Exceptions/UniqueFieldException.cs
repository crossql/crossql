using System;

namespace crossql.Exceptions
{
    public class UniqueFieldException : Exception
    {
        public UniqueFieldException()
        {
        }

        public UniqueFieldException(string message) : base(message)
        {
        }
    }
}