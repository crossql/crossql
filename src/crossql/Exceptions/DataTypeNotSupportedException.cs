using System;

namespace crossql.Exceptions
{
    public class DataTypeNotSupportedException : Exception
    {
        public DataTypeNotSupportedException()
        {
        }

        public DataTypeNotSupportedException(string message) : base(message)
        {
        }
    }
}