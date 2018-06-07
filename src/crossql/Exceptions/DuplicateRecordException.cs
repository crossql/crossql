using System;

namespace crossql.Exceptions
{
    public class DuplicateRecordException : Exception
    {
        public DuplicateRecordException()
        {
        }

        public DuplicateRecordException(string message) : base(message)
        {
        }
    }
}