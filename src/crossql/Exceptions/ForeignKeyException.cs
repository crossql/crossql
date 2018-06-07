using System;

namespace crossql.Exceptions
{
    public class ForeignKeyException : Exception
    {
        public ForeignKeyException(string message) : base(message)
        {
        }
    }
}